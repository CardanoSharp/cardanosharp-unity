using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class InventoryUI : MonoBehaviour
{
    public List<Item> testItems;

    [Header("Animation")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private float tweenDuration;

    [SerializeField] private Transform itemsParent;

    private InventorySlot[] slots;

    private Inventory inventory;

    private bool isOpen;

    private MouseLook mouseLook;



    private void Awake()
    {
        inventoryPanel.transform.localScale = Vector3.zero;
    }

    private void Start()
    {
        mouseLook = GameObject.FindObjectOfType<MouseLook>();
        slots = itemsParent.GetComponentsInChildren<InventorySlot>();
        inventory = Inventory.Instance;

        foreach(Item i in testItems)
        {
            inventory.Add(i);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) { OpenInventory(); }
    }

    void OpenInventory()
    {
        if (!isOpen)
        {
            mouseLook.FixView(true);
            inventoryPanel.transform.DOScale(Vector3.one, tweenDuration).SetEase(Ease.InQuad).SetUpdate(true).Play();
            isOpen = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            mouseLook.FixView(false);
            inventoryPanel.transform.DOScale(Vector3.zero, tweenDuration).SetEase(Ease.InQuad).SetUpdate(true).Play();
            isOpen = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void UpdateUI(Item item)
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if(i < inventory.items.Count)
            {
                if(slots[i].GetItem() == item || !slots[i].GetItem())
                {
                    slots[i].AddItem(inventory.items[i]);
                }
            }
            else { slots[i].ClearSlot(); }
        }
    }
}
