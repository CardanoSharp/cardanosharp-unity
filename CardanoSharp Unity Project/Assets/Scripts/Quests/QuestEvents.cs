using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class QuestEvents : Singleton<QuestEvents>
{
    public CardanoManager CardanoManager;

    public void StartQuest1()
    {
        CardanoManager.CreatePlayerWallet("Player");
        Debug.Log("Player wallet created");
    }

    public void CompleteQuest1()
    {
        Debug.Log("Quest 1 Completed!");
    }

    public void CompleteQuest3()
    {
        CardanoManager.MintNFT("Developer", "Player", "Sword", 1, new { Name = "Oathbringer", Stats = new { Atk = 100, Type = "2H Sword"}});
    }
}
