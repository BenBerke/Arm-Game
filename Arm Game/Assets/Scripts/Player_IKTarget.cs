using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_IKTarget : MonoBehaviour
{
    ///<summary>
    /// This script handles the IK effector of the player arm
    ///</summary>

    [SerializeField] GameObject target;
    public GameObject effector;
    public Transform shoulderPos;

    [SerializeField] LayerMask handCollidable;

    [HideInInspector] public Vector2 mouseWorldPos;

    public float followSpeed;

    [HideInInspector] public Rigidbody2D targetRb;

    private void Start()
    {
        targetRb = target.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        targetRb.velocity = (TargetPosition() - target.transform.position) * followSpeed;
    }
    Vector3 TargetPosition()
    {
        mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D lineHit = Physics2D.Linecast(shoulderPos.position, mouseWorldPos, handCollidable);
        if (lineHit.point == Vector2.zero) return mouseWorldPos;
        return lineHit.point;
    }
}
