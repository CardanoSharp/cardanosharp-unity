using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestEvents : MonoBehaviour
{
    public CardanoManager CardanoManager;

    public void StartQuest1()
    {
        CardanoManager.CreateWallet("Player");
    }

    public void CompleteQuest1()
    {
        Debug.Log("Quest 1 Completed!");
    }

    public void CompleteQuest3()
    {
        CardanoManager.MintNFT("Sword");
    }
}
