using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Custom/Item")]
public class Item : ScriptableObject
{
    public string id = "New Item";
    public Sprite icon;
    public int amount = 1;
    public bool isDefaultItem;
}
