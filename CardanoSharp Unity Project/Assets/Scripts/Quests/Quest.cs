using Assets.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Custom/Quests")]
public class Quest : ScriptableObject
{
    public string id;
    [TextArea(1, 3)]
    public string questInfo;

    public startEvent OnStartEvent;
    public completeEvent OnCompleteEvent;
    
    //public GameEvent OnStart;
    //public GameEvent OnComplete;
    
    public enum startEvent //Add enum variables for every event case.
    {
        Nothing,
        StartQuest1
    };

    public enum completeEvent
    {
        Nothing,
        CompleteQuest1
    };

    public void OnStart() //Add case for every event.
    {
        switch (OnStartEvent)
        {
            case startEvent.Nothing:
                break;
            
            case startEvent.StartQuest1:
                QuestEvents.Instance.StartQuest1();
                break;
        }
    }

    public void OnComplete() //Add case for every event.
    {
        switch (OnCompleteEvent)
        {
            case completeEvent.Nothing:
                break;
            
            case completeEvent.CompleteQuest1:
                QuestEvents.Instance.CompleteQuest1();
                break;
        }
    }
}
