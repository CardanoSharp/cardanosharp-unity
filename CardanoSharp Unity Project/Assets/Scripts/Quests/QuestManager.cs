using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Utils;

public class QuestManager : Singleton<QuestManager>
{
    [SerializeField] private List<Quest> quests;

    [SerializeField] private GameObject questPanel;
    [SerializeField] private TMP_Text questText;

    [SerializeField] private float fadeTextDuration;

    private int currentQuestIndex = 0;

    private void Start()
    {
        SetCurrentQuestInfo();
    }

    public void SetCurrentQuestInfo()
    {
        questText.text = quests[currentQuestIndex].questInfo;
        NewQuestAnimation();
    }

    [ContextMenu("Complete Quest")]
    public void CompleteCurrentQuest()
    {
        if (currentQuestIndex < quests.Count - 1)
        {
            currentQuestIndex++;
            SetCurrentQuestInfo();
        }
        else { Debug.Log("All quests finished"); }
    }

    void NewQuestAnimation()
    {
        questText.alpha = 0;
        questText.DOFade(1, fadeTextDuration).SetEase(Ease.Linear).Play();
    }

}
