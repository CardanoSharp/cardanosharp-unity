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

    public GameEvent OnStart;
    public GameEvent OnComplete;
}
