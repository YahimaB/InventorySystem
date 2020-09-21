using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class InventorySlot : MonoBehaviour
{
    [HideInInspector]
    public int ItemID = 0;

    public void SetItemData(int itemID, Sprite itemIcon)
    {
        ItemID = itemID;
        GetComponent<Image>().overrideSprite = itemIcon;
    }
}
