using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace cherrydev
{
    public class AnswerPanel : MonoBehaviour
    {
        [SerializeField] private Button _answerButtonPrefab;
        [SerializeField] private Transform _parentTransform;

        private readonly List<Button> _buttons = new();
        private readonly List<TextMeshProUGUI> _buttonTexts = new();

        /// <summary>
        /// Returns the total number of buttons
        /// </summary>
        /// <returns>The number of buttons</returns>
        public int GetButtonCount() => _buttons.Count;
        
        /// <summary>
        /// Instantiate answer buttons based on max amount of answer buttons
        /// </summary>
        /// <param name="maxAmountOfAnswerButtons"></param>
        public void SetUpButtons(int maxAmountOfAnswerButtons)
        {
            DeleteAllExistingButtons();
            
            for (int i = 0; i < maxAmountOfAnswerButtons; i++)
            {
                Button answerButton = Instantiate(_answerButtonPrefab, _parentTransform);

                _buttons.Add(answerButton);
                _buttonTexts.Add(answerButton.GetComponentInChildren<TextMeshProUGUI>());
            }
        }

        /// <summary>
        /// Returning button by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Button GetButtonByIndex(int index) => _buttons[index];

        /// <summary>
        /// Returning button text bu index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public TextMeshProUGUI GetButtonTextByIndex(int index) => _buttonTexts[index];

        /// <summary>
        /// Setting UnityAction to button onClick event by index 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="action"></param>
        public void AddButtonOnClickListener(int index, UnityAction action) => _buttons[index].onClick.AddListener(action);

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Enable certain amount of buttons
        /// </summary>
        /// <param name="amount"></param>
        public void EnableCertainAmountOfButtons(int amount)
        {
            if (_buttons.Count == 0)
            {
                Debug.LogWarning("Please assign button list!");
                return;
            }

            for (int i = 0; i < amount; i++)
                _buttons[i].gameObject.SetActive(true);
        }

        /// <summary>
        /// Disable all buttons
        /// </summary>
        public void DisableAllButtons()
        {
            foreach (Button button in _buttons)
                button.gameObject.SetActive(false);
        }

        /// <summary>
        /// Removes all existing buttons, used before setup
        /// </summary>
        private void DeleteAllExistingButtons()
        {
            if (_buttons.Count > 0)
            {
                foreach (var button in _buttons) 
                    Destroy(button.gameObject);
                            
                _buttons.Clear();
                _buttonTexts.Clear();
            }
        }
    }
}