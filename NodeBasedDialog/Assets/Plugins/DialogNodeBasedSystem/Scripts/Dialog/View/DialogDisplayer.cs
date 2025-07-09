using UnityEngine;

namespace cherrydev
{
    public class DialogDisplayer : MonoBehaviour
    {
        [Header("MAIN COMPONENT")]
        [SerializeField] private DialogBehaviour _dialogBehaviour;

        [Header("NODE PANELS")]
        [SerializeField] private SentencePanel _dialogSentencePanel;
        [SerializeField] private AnswerPanel _dialogAnswerPanel;

        private void OnEnable()
        {
            _dialogBehaviour.AddListenerToDialogFinishedEvent(DisableDialogPanel);

            _dialogBehaviour.DialogDisabled += DisableDialogPanel;
            _dialogBehaviour.AnswerButtonSetUp += SetUpAnswerButtonsClickEvent;

            _dialogBehaviour.DialogTextCharWrote += _dialogSentencePanel.IncreaseMaxVisibleCharacters;
            _dialogBehaviour.DialogTextSkipped += _dialogSentencePanel.ShowFullDialogText;

            _dialogBehaviour.SentenceNodeActivated += EnableDialogSentencePanel;
            _dialogBehaviour.SentenceNodeActivated += DisableDialogAnswerPanel;
            _dialogBehaviour.SentenceNodeActivated += _dialogSentencePanel.ResetDialogText;
            _dialogBehaviour.SentenceNodeActivatedWithParameter += _dialogSentencePanel.Setup;

            _dialogBehaviour.AnswerNodeActivated += EnableDialogAnswerPanel;
            _dialogBehaviour.AnswerNodeActivated += DisableDialogSentencePanel;

            _dialogBehaviour.AnswerNodeActivatedWithParameter += _dialogAnswerPanel.EnableCertainAmountOfButtons;
            _dialogBehaviour.MaxAmountOfAnswerButtonsCalculated += _dialogAnswerPanel.SetUpButtons;

            _dialogBehaviour.AnswerNodeSetUp += SetUpAnswerDialogPanel;
#if UNITY_LOCALIZATION
            _dialogBehaviour.LanguageChanged += HandleLanguageChanged;
#endif
        }

        private void OnDisable()
        {
            _dialogBehaviour.DialogDisabled -= DisableDialogPanel;
            _dialogBehaviour.AnswerButtonSetUp -= SetUpAnswerButtonsClickEvent;

            _dialogBehaviour.DialogTextCharWrote -= _dialogSentencePanel.IncreaseMaxVisibleCharacters;
            _dialogBehaviour.DialogTextSkipped -= _dialogSentencePanel.ShowFullDialogText;

            _dialogBehaviour.SentenceNodeActivated -= EnableDialogSentencePanel;
            _dialogBehaviour.SentenceNodeActivated -= DisableDialogAnswerPanel;
            _dialogBehaviour.SentenceNodeActivated += _dialogSentencePanel.ResetDialogText;
            _dialogBehaviour.SentenceNodeActivatedWithParameter -= _dialogSentencePanel.Setup;

            _dialogBehaviour.AnswerNodeActivated -= EnableDialogAnswerPanel;
            _dialogBehaviour.AnswerNodeActivated -= DisableDialogSentencePanel;

            _dialogBehaviour.AnswerNodeActivatedWithParameter -= _dialogAnswerPanel.EnableCertainAmountOfButtons;
            _dialogBehaviour.MaxAmountOfAnswerButtonsCalculated -= _dialogAnswerPanel.SetUpButtons;

            _dialogBehaviour.AnswerNodeSetUp -= SetUpAnswerDialogPanel;
#if UNITY_LOCALIZATION
            _dialogBehaviour.LanguageChanged -= HandleLanguageChanged;
#endif
        }

        /// <summary>
        /// Disable dialog answer and sentence panel
        /// </summary>
        public void DisableDialogPanel()
        {
            DisableDialogAnswerPanel();
            DisableDialogSentencePanel();
        }

        /// <summary>
        /// Enable dialog answer panel
        /// </summary>
        public void EnableDialogAnswerPanel()
        {
            ActiveGameObject(_dialogAnswerPanel.gameObject, true);
            _dialogAnswerPanel.DisableAllButtons();
        }

        /// <summary>
        /// Disable dialog answer panel
        /// </summary>
        public void DisableDialogAnswerPanel() => ActiveGameObject(_dialogAnswerPanel.gameObject, false);

        /// <summary>
        /// Enable dialog sentence panel
        /// </summary>
        public void EnableDialogSentencePanel()
        {
            _dialogSentencePanel.ResetDialogText();
            ActiveGameObject(_dialogSentencePanel.gameObject, true);
        }

        /// <summary>
        /// Disable dialog sentence panel
        /// </summary>
        public void DisableDialogSentencePanel() => ActiveGameObject(_dialogSentencePanel.gameObject, false);

        /// <summary>
        /// Enable or disable game object depends on isActive bool flag
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="isActive"></param>
        public void ActiveGameObject(GameObject gameObject, bool isActive)
        {
            if (gameObject == null)
            {
                Debug.LogWarning("Game object is null");
                return;
            }

            gameObject.SetActive(isActive);
        }
        
        /// <summary>
        /// Removing all listeners and Setting up answer button onClick event
        /// UPDATED: Now uses the new ChildNodes system and passes the button index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="answerNode"></param>
        public void SetUpAnswerButtonsClickEvent(int index, AnswerNode answerNode)
        {
            _dialogAnswerPanel.GetButtonByIndex(index).onClick.RemoveAllListeners();
            _dialogAnswerPanel.AddButtonOnClickListener(index, 
                () => _dialogBehaviour.SetCurrentNodeAndHandleDialogGraph(index));
        }

        /// <summary>
        /// Setting up answer dialog panel
        /// </summary>
        /// <param name="index"></param>
        /// <param name="answerText"></param>
        public void SetUpAnswerDialogPanel(int index, string answerText)
        {
            AnswerNode currentAnswerNode = _dialogBehaviour.CurrentAnswerNode;
            
            if (currentAnswerNode != null)
                _dialogAnswerPanel.GetButtonTextByIndex(index).text = currentAnswerNode.GetAnswerText(index);
            else
                _dialogAnswerPanel.GetButtonTextByIndex(index).text = answerText;
        }

        private void HandleLanguageChanged()
        {
            if (_dialogBehaviour.CurrentAnswerNode != null)
                RefreshAnswerButtons();
        }
        
        /// <summary>
        /// Refresh all answer buttons with updated localized text
        /// </summary>
        private void RefreshAnswerButtons()
        {
            AnswerNode currentAnswerNode = _dialogBehaviour.CurrentAnswerNode;
            
            if (currentAnswerNode != null)
            {
                for (int i = 0; i < currentAnswerNode.Answers.Count; i++)
                {
                    if (i < _dialogAnswerPanel.GetButtonCount() &&
                        _dialogAnswerPanel.GetButtonByIndex(i).gameObject.activeSelf)
                        _dialogAnswerPanel.GetButtonTextByIndex(i).text = currentAnswerNode.GetAnswerText(i);
                }
            }
        }
    }
}