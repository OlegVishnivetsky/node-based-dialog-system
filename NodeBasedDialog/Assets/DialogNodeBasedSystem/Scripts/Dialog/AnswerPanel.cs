using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace cherrydev
{
    public class AnswerPanel : MonoBehaviour
    {
        [SerializeField] private Button answerButtonPrefab;
        [SerializeField] private Transform parentTransform;

        private List<Button> buttons = new List<Button>();
        private List<TextMeshProUGUI> buttonTexts = new List<TextMeshProUGUI>();

        /// <summary>
        /// Instantiate answer buttons based on max amount of answer buttons
        /// </summary>
        /// <param name="maxAmountOfAnswerButtons"></param>
        public void SetUpButtons(int maxAmountOfAnswerButtons)
        {
            for (int i = 0; i < maxAmountOfAnswerButtons; i++)
            {
                Button answerButton = Instantiate(answerButtonPrefab, parentTransform);

                buttons.Add(answerButton);
                buttonTexts.Add(answerButton.GetComponentInChildren<TextMeshProUGUI>());
            }
        }

        /// <summary>
        /// Returning button by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Button GetButtonByIndex(int index)
        {
            return buttons[index];
        }

        /// <summary>
        /// Returning button text bu index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public TextMeshProUGUI GetButtonTextByIndex(int index)
        {
            return buttonTexts[index];
        }

        /// <summary>
        /// Setting UnityAction to button onClick event by index 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="action"></param>
        public void AddButtonOnClickListener(int index, UnityAction action)
        {
            buttons[index].onClick.AddListener(action);
        }

        /// <summary>
        /// Enable certain amount of buttons
        /// </summary>
        /// <param name="amount"></param>
        public void EnableCertainAmountOfButtons(int amount)
        {
            if (buttons.Count == 0)
            {
                Debug.LogWarning("Please assign button list!");
                return;
            }

            for (int i = 0; i < amount; i++)
            {
                buttons[i].gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Disable all buttons
        /// </summary>
        public void DisalbleAllButtons()
        {
            foreach (Button button in buttons)
            {
                button.gameObject.SetActive(false);
            }
        }
    }
}