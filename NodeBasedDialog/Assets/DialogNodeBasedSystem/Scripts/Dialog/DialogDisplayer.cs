using UnityEngine;

namespace cherrydev
{
    public class DialogDisplayer : MonoBehaviour
    {
        [Header("MAIN COMPONENT")]
        [SerializeField] private DialogBehaviour dialogBehaviour;

        [Header("NODE PANELS")]
        [SerializeField] private SentencePanel dialogSentensePanel;
        [SerializeField] private AnswerPanel dialogAnswerPanel;

        private void OnEnable()
        {
            dialogBehaviour.AddListenerToDialogFinishedEvent(DisableDialogPanel);

            DialogBehaviour.OnAnswerButtonSetUp += SetUpAnswerButtonsClickEvent;

            DialogBehaviour.OnDialogSentenceEnd += dialogSentensePanel.ResetDialogText;

            DialogBehaviour.OnDialogTextCharWrote += dialogSentensePanel.AddCharToDialogText;

            DialogBehaviour.OnSentenceNodeActive += EnableDialogSentencePanel;
            DialogBehaviour.OnSentenceNodeActive += DisableDialogAnswerPanel;
            DialogBehaviour.OnSentenceNodeActiveWithParameter += dialogSentensePanel.AssignDialogNameTextAndSprite;

            DialogBehaviour.OnAnswerNodeActive += EnableDialogAnswerPanel;
            DialogBehaviour.OnAnswerNodeActive += DisableDialogSentencePanel;

            DialogBehaviour.OnAnswerNodeActiveWithParameter += dialogAnswerPanel.EnableCertainAmountOfButtons;
            DialogBehaviour.OnMaxAmountOfAnswerButtonsCalculated += dialogAnswerPanel.SetUpButtons;

            DialogBehaviour.OnAnswerNodeSetUp += SetUpAnswerDialogPanel;
        }

        private void OnDisable()
        {
            DialogBehaviour.OnAnswerButtonSetUp -= SetUpAnswerButtonsClickEvent;

            DialogBehaviour.OnDialogSentenceEnd -= dialogSentensePanel.ResetDialogText;

            DialogBehaviour.OnDialogTextCharWrote -= dialogSentensePanel.AddCharToDialogText;

            DialogBehaviour.OnSentenceNodeActive -= EnableDialogSentencePanel;
            DialogBehaviour.OnSentenceNodeActive -= DisableDialogAnswerPanel;

            DialogBehaviour.OnSentenceNodeActiveWithParameter -= dialogSentensePanel.AssignDialogNameTextAndSprite;

            DialogBehaviour.OnAnswerNodeActive -= EnableDialogAnswerPanel;
            DialogBehaviour.OnAnswerNodeActive -= DisableDialogSentencePanel;

            DialogBehaviour.OnAnswerNodeActiveWithParameter -= dialogAnswerPanel.EnableCertainAmountOfButtons;
            DialogBehaviour.OnMaxAmountOfAnswerButtonsCalculated -= dialogAnswerPanel.SetUpButtons;

            DialogBehaviour.OnAnswerNodeSetUp -= SetUpAnswerDialogPanel;
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
            ActiveGameObject(dialogAnswerPanel.gameObject, true);
            dialogAnswerPanel.DisalbleAllButtons();
        }

        /// <summary>
        /// Disable dialog answer panel
        /// </summary>
        public void DisableDialogAnswerPanel()
        {
            ActiveGameObject(dialogAnswerPanel.gameObject, false);
        }

        /// <summary>
        /// Enable dialog sentence panel
        /// </summary>
        public void EnableDialogSentencePanel()
        {
            dialogSentensePanel.ResetDialogText();

            ActiveGameObject(dialogSentensePanel.gameObject, true);
        }

        /// <summary>
        /// Disable dialog sentence panel
        /// </summary>
        public void DisableDialogSentencePanel()
        {
            ActiveGameObject(dialogSentensePanel.gameObject, false);
        }

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
        /// Setting up answer button onClick event
        /// </summary>
        /// <param name="index"></param>
        /// <param name="answerNode"></param>
        public void SetUpAnswerButtonsClickEvent(int index, AnswerNode answerNode)
        {
            dialogAnswerPanel.GetButtonByIndex(index).onClick.AddListener(() =>
            {
                dialogBehaviour.SetCurrentNodeAndHandleDialogGraph(answerNode.childSentenceNodes[index]);
            });
        }

        /// <summary>
        /// Setting up answer dialog panel
        /// </summary>
        /// <param name="index"></param>
        /// <param name="answerText"></param>
        public void SetUpAnswerDialogPanel(int index, string answerText)
        {
            dialogAnswerPanel.GetButtonTextByIndex(index).text = answerText;
        }
    }
}