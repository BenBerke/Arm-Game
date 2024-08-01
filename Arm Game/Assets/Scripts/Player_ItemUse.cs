using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_ItemUse : MonoBehaviour
{
    Player_ItemPickUp itemPickUp;

    private void Start()
    {
        itemPickUp = GetComponent<Player_ItemPickUp>();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) UseItemInHand();
    }

    void UseItemInHand()
    {
        if (itemPickUp.itemInHand == null) return;
        itemPickUp.itemInHand.GetComponent<IUse>().Use();
    }
}
