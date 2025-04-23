using UnityEngine;

namespace cherrydev
{
    [System.Serializable]
    public struct Sentence
    {
        public string CharacterName;
        public string Text;
        public Sprite CharacterSprite;

        public Sentence(string characterName, string text)
        {
            CharacterSprite = null;
            CharacterName = characterName;
            Text = text;
        }
    }
}