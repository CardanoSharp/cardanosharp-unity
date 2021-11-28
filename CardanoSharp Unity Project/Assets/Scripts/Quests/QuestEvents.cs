using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestEvents : MonoBehaviour
{
    public CardanoManager CardanoManager;

    public void StartQuest1()
    {
        Debug.Log("Quest 1 Started!");
    }

    public void CompleteQuest1()
    {
        Debug.Log("Quest 1 Completed!");
    }
}
