using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueObject dialogue;
    [SerializeField] private Camera dialogueCamera;

    [SerializeField] private bool completeQuest;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void StartDialogue()
    {
        DialogueUI.Instance.ShowDialogue(dialogue, this);
    }

    void SetCamera()
    {
        mainCamera.enabled = false;
        dialogueCamera.enabled = true;
    }

    public void EndDialogue()
    {
        dialogueCamera.enabled = false;
        mainCamera.enabled = true;

        if (completeQuest) { QuestManager.Instance.CompleteCurrentQuest(); }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider collision)
    {
        SetCamera();
        StartDialogue();
    }
}
