using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class Inventory : Singleton<Inventory>
{
    [SerializeField] private int inventorySpace;
    public List<Item> items;

    private InventoryUI inventoryUI;

    private void Start()
    {
        inventoryUI = GetComponent<InventoryUI>();
    }

    public void Add(Item item) 
    {
        if (!item.isDefaultItem)
        {
            if (items.Contains(item))
            {
                inventoryUI.UpdateUI(item);
            }
            else if (items.Count < inventorySpace)
            {
                items.Add(item);
                inventoryUI.UpdateUI(item);
            }
        }
    }

    public void Remove(Item item) 
    {
        items.Remove(item); 
    }
}
