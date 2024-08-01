using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Gun : MonoBehaviour, IUse
{
    public void Use()
    {
        print("Shoot");
    }
}
