using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static DialogueObject;
using Utils;

public class DialogueUI : Singleton<DialogueUI>
{
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text speakerLabel;
    [SerializeField] private TMP_Text textLabel;
    [SerializeField] private Image rightSpeakerImage;
    [SerializeField] private Image leftSpeakerImage;

    //[SerializeField] private float waitToChangeText = 2f;

    private TypeEffect typeEffect;

    private DialogueTrigger trigger;

    private MouseLook mouseLook;

    private PlayerMovement player;

    private void Awake()
    {
        typeEffect = GetComponent<TypeEffect>();
        mouseLook = GameObject.FindObjectOfType<MouseLook>();
        player = GameObject.FindObjectOfType<PlayerMovement>();
    }

    private void Start()
    {
        CloseDialogue();
    }

    void SetSpeakerLabel(string speakerName) { speakerLabel.text = speakerName; }

    void SetRightSpeakerSprite(Sprite sprite) 
    {
        if (sprite)
        {
            rightSpeakerImage.sprite = sprite;
            leftSpeakerImage.enabled = false;
            rightSpeakerImage.enabled = true;
        }
        else { rightSpeakerImage.enabled = false; }
    }

    void SetLeftSpeakerSprite(Sprite sprite)
    {
        if (sprite)
        {
            leftSpeakerImage.sprite = sprite;
            rightSpeakerImage.enabled = false;
            leftSpeakerImage.enabled = true;
        }
        else { leftSpeakerImage.enabled = false; }
    }

    public void ShowDialogue(DialogueObject dialogueObject, DialogueTrigger t)
    {
        trigger = t;

        player.CanMove(false);
        mouseLook.FixView(true);

        dialogueBox.SetActive(true);

        StartCoroutine(StepThroughDialogue(dialogueObject));
    }

    private IEnumerator StepThroughDialogue(DialogueObject dialogueObject)
    {
        int dialogueIndex = 0;
        foreach (Speaker speaker in dialogueObject.Dialogue)
        {
            SetSpeakerLabel(dialogueObject.GetSpeaker(dialogueIndex));

            SetSpriteOnPosition(dialogueObject, dialogueIndex);

            dialogueIndex++;

            yield return typeEffect.Run(speaker.dialogue, textLabel);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            //yield return new WaitForSeconds(1f);
        }

        CloseDialogue();
    }

    void SetSpriteOnPosition(DialogueObject dialogueObject, int index)
    {
        switch (dialogueObject.GetSpritePosition(index))
        {
            case SpritePosition.Right:
                SetRightSpeakerSprite(dialogueObject.GetSprite(index));
                return;
            case SpritePosition.Left:
                SetLeftSpeakerSprite(dialogueObject.GetSprite(index));
                return;
        }
    }

    private void CloseDialogue()
    {
        if (trigger) 
        {
            trigger.EndDialogue();
            trigger = null;
        }

        player.CanMove(true);
        mouseLook.FixView(false);

        dialogueBox.SetActive(false);
        textLabel.text = "";

    }
}
