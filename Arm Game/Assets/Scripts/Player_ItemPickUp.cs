using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_ItemPickUp : MonoBehaviour
{
    /// <summary>
    /// This script handles picking up items from the ground
    /// </summary>

    [Tooltip("Circlecast circle radius")]
    [SerializeField] float circleRadius;
    [Tooltip("Max distance the player can pull items from")]
    [SerializeField] float grabDistance;
    [Tooltip("Distance from effector when it will snap to the hand")]
    [SerializeField] float snapToHandDistance;
    [Tooltip("Radius of the circle centered on the effector that checks for items. Higher priority than casts")]
    [SerializeField] float generalHandRadius;
    [Tooltip("Radius of the smaller circle centered on the effector that checks for items. Higher priority than generalHandRadius")]
    [SerializeField] float specificHandRadius;
    [SerializeField] float throwForce;

    [SerializeField] Transform handPos; // Hand pos because effector's position gets messed up for some reason

    [SerializeField] Vector2 maxThrowVelocity;


    // Cache
    Player_IKTarget ikTarget;
    Transform shoulder;
    Transform effector;
    SpringJoint2D chosenItem_Sj;
    Collider2D chosenItem_collider;
    Rigidbody2D chosenItem_Rb;
    Item_Script itemScript;

    GameObject chosenItem;
    GameObject lastChosenItem;
    public GameObject itemInHand;

    bool pickingUp;
    bool holdingItem;

    private void Start()
    {
        ikTarget = GetComponent<Player_IKTarget>();
        shoulder = ikTarget.shoulderPos;
        effector = ikTarget.effector.transform;
    }
    private void Update()
    {
        // Detect which item player is trying to reach
        if (!(pickingUp || holdingItem) && !Input.GetMouseButton(1)) 
        {
            chosenItem = ChosenItemToPickUp();
            if (lastChosenItem != null)
                if (chosenItem != lastChosenItem) lastChosenItem.GetComponent<SpriteRenderer>().color = Color.white;
            lastChosenItem = ChosenItemToPickUp();
        }

        if (chosenItem != null)
        {
            chosenItem_Sj = chosenItem.GetComponent<SpringJoint2D>();
            chosenItem_Rb = chosenItem.GetComponent<Rigidbody2D>();
            chosenItem_collider = chosenItem.GetComponent<PolygonCollider2D>();
            if (!holdingItem)
            chosenItem.GetComponent<SpriteRenderer>().color = Color.red;

            if (Input.GetMouseButtonDown(1))
            {
                FlyItem(true);
            }
            if (Input.GetMouseButtonUp(1))
            {
                FlyItem(false);
                if(holdingItem)
                GrabItem(false);
            }
            if (Input.GetMouseButton(1))
            {
                if (Vector2.Distance(handPos.position, chosenItem.transform.position) < snapToHandDistance && !holdingItem) GrabItem(true);
                chosenItem.transform.rotation = handPos.rotation;
            }
        }

    }

    /// <param name="fly"> if true, item will fly to the player. Else, stops</param>
    void FlyItem(bool fly)
    {
        itemScript = chosenItem.GetComponent<Item_Script>();
        chosenItem_Rb.constraints = RigidbodyConstraints2D.None;
        chosenItem_Sj.enabled = fly;
        pickingUp = fly;
        if (fly)
        {
            itemScript.chosenItem = true;
            chosenItem_Sj.connectedBody = effector.GetComponent<Rigidbody2D>();
            chosenItem_Rb.gravityScale = 0;
            return;
        }
        itemScript.chosenItem = false;
        chosenItem_Sj.connectedBody = null;
        chosenItem_Rb.gravityScale = 1;
        chosenItem_Rb.velocity /= 3f; // Slow down the item otherwise it goes too fast. Not mandatory but it is better this way
        chosenItem_Rb.isKinematic = false;
    }

    /// <param name="grab"> if true, grab item. Else, drop</param>
    public void GrabItem(bool grab)
    {
        itemScript.chosenItem = false;
        holdingItem = grab;
        chosenItem_Sj.enabled = !grab;
        chosenItem_Rb.isKinematic = grab;
        chosenItem_collider.isTrigger = grab;
        if (grab)
        {
            itemInHand = chosenItem;
            chosenItem.GetComponent<SpriteRenderer>().color = Color.white;
            chosenItem_Rb.velocity = Vector2.zero;
            chosenItem.transform.parent = handPos;
            chosenItem.transform.localPosition = Vector2.zero;
            chosenItem.transform.position = new Vector2(chosenItem.transform.position.x + itemScript.handOffset.x, chosenItem.transform.position.y + itemScript.handOffset.y);
            return;
        }
        itemInHand = null;
        chosenItem.transform.parent = null;
        chosenItem_Sj.enabled = false;

        Collider2D[] cols = Physics2D.OverlapCircleAll(chosenItem.transform.position, .03f);
        foreach(Collider2D col in cols)
        {
            if (col.gameObject.tag == "ItemSlot" && !col.GetComponent<Player_ItemSlot>().isFull) 
            {
                // Put the item in slot
                GameObject itemSlot = col.gameObject;
                chosenItem.GetComponent<SpriteRenderer>().color = Color.white; // temporary
                chosenItem.transform.parent = itemSlot.transform;
                chosenItem.transform.localPosition = new Vector2(itemScript.itemSlotOffset.x, itemScript.itemSlotOffset.y);
                chosenItem.transform.localRotation = Quaternion.Euler(0, 0, itemScript.itemSlotRotation);
                chosenItem_Rb.constraints = RigidbodyConstraints2D.FreezeAll;
                chosenItem_Sj.enabled = false;
                chosenItem_Rb.isKinematic = true;
                chosenItem_collider.isTrigger = true;
            }
        }

        // Calculate the velocity of the item when thrown
        float throwVelocityX = ikTarget.targetRb.velocity.x * throwForce / ikTarget.followSpeed;
        float throwVelocityY = ikTarget.targetRb.velocity.y * throwForce / ikTarget.followSpeed;
        Vector2 finalVelocity = new Vector2(Mathf.Min(throwVelocityX, maxThrowVelocity.x), Mathf.Min(throwVelocityY, maxThrowVelocity.y));
        chosenItem_Rb.velocity = finalVelocity;
    }
    GameObject ChosenItemToPickUp()
    {
        Collider2D smallGrabCircle = Physics2D.OverlapCircle(effector.position, specificHandRadius);
        Collider2D largeGrabCircle = Physics2D.OverlapCircle(effector.position, generalHandRadius);
        RaycastHit2D circleCast = Physics2D.CircleCast(effector.position, circleRadius, effector.position - shoulder.position, grabDistance);
        RaycastHit2D rayCast = Physics2D.Raycast(effector.position, effector.position - shoulder.position);

        Collider2D[] collidersFound = { smallGrabCircle, largeGrabCircle, rayCast.collider, circleCast.collider };

        foreach(Collider2D col in collidersFound) 
            if (col != null && col.GetComponent<Item_Script>() != null) return col.gameObject;
        return null;
    }
}
