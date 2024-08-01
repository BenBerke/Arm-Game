using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Script : MonoBehaviour
{
    /// <summary>
    /// This script should be on each item
    /// </summary>
    public Vector2 handOffset;
    public Vector2 itemSlotOffset;
    [Range(0, 360)]
    public float itemSlotRotation;
    [HideInInspector] public bool chosenItem;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (chosenItem && collision.gameObject.tag == "Player")
            collision.gameObject.GetComponent<Player_ItemPickUp>().GrabItem(true);
    }
}

public interface IUse
{
    void Use();
}
