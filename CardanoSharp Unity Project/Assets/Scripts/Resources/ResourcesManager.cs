using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesManager : MonoBehaviour
{
    [SerializeField] private int goldAmount;
    [SerializeField] private int ingotsAmount;

    private ResourcesUI resourcesUI;

    private void Awake()
    {
        resourcesUI = GetComponent<ResourcesUI>();
    }

    private void Start()
    {
        resourcesUI.UpdateResourcesText(goldAmount, ingotsAmount);
    }
}
