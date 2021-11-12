using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourcesUI : MonoBehaviour
{
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text ingotText;

    private ResourcesManager resources;

    private void Awake()
    {
        resources = GetComponent<ResourcesManager>();
    }

    public void UpdateResourcesText(int gold, int ingot)
    {
        goldText.text = gold.ToString();
        ingotText.text = ingot.ToString();
    }
}
