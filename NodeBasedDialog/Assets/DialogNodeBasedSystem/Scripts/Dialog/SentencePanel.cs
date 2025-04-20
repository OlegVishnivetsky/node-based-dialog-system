using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cherrydev
{
    public class SentencePanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _dialogNameText;
        [SerializeField] private TextMeshProUGUI _dialogText;
        [SerializeField] private Image _dialogCharacterImage;

        private string _currentFullText;
        
        /// <summary>
        /// Setting dialogText max visible characters to zero
        /// </summary>
        public void ResetDialogText()
        {
            _dialogText.maxVisibleCharacters = 0;
            _currentFullText = string.Empty;
        }

        /// <summary>
        /// Set dialog text max visible characters to dialog text length
        /// </summary>
        /// <param name="text"></param>
        public void ShowFullDialogText(string text)
        {
            _currentFullText = text;
            _dialogText.text = text;
            _dialogText.maxVisibleCharacters = text.Length;
        }

        /// <summary>
        /// Increasing max visible characters
        /// </summary>
        public void IncreaseMaxVisibleCharacters() => _dialogText.maxVisibleCharacters++;
        
        /// <summary>
        /// Assigning dialog name text, character image sprite and dialog text
        /// </summary>
        public void Setup(string characterName, string text, Sprite sprite)
        {
            _dialogNameText.text = characterName;
            _dialogText.text = text;
            _currentFullText = text;

            if (sprite == null)
            {
                _dialogCharacterImage.color = new Color(_dialogCharacterImage.color.r,
                    _dialogCharacterImage.color.g, _dialogCharacterImage.color.b, 0);
                return;
            }

            _dialogCharacterImage.color = new Color(_dialogCharacterImage.color.r,
                _dialogCharacterImage.color.g, _dialogCharacterImage.color.b, 255);
            _dialogCharacterImage.sprite = sprite;
        }
    }
}