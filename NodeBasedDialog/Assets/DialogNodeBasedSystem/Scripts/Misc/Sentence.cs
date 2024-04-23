using UnityEngine;

namespace cherrydev
{
    [System.Serializable]
    public struct Sentence
    {
        public string characterName;
        public string text;
        public Sprite characterSprite;
        public Sprite emotionSprite;

        public Sentence(string characterName, string text)
        {
            characterSprite = null;
            emotionSprite = null;
            this.characterName = characterName;
            this.text = text;
        }
    }
}