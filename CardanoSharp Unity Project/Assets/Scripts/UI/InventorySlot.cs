using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    private Item item;
    [SerializeField]private int amount = 0;

    public void AddItem(Item newItem)
    {
        if (!item)
        {
            item = newItem;

            icon.sprite = item.icon;
            icon.enabled = true;

            amount = newItem.amount;
            return;
        }
        else if (item == newItem)
        {
            amount += newItem.amount;
        }
    }

    public void ClearSlot()
    {
        item = null;

        icon.sprite = null;
        icon.enabled = false;

        amount = 0;
    }

    public Item GetItem() { return item; }
}
