using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Custom/Dialogue")]
public class DialogueObject : ScriptableObject
{
    public enum SpritePosition { Right, Left };

    [System.Serializable]
    public class Speaker
    {
        public string speakerName;
        public Sprite speakerSprite;
        [TextArea] public string dialogue;
        public SpritePosition spritePosition;
    }

    public List<Speaker> Dialogue;

    public string GetSpeaker(int index) { return Dialogue[index].speakerName; }
    public Sprite GetSprite(int index) { return Dialogue[index].speakerSprite; }
    public SpritePosition GetSpritePosition(int index) { return Dialogue[index].spritePosition; }
}
