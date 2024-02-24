using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace cherrydev
{
    public class DialogBehaviour : MonoBehaviour
    {
        [SerializeField] private float dialogCharDelay;
        [SerializeField] private List<KeyCode> nextSentenceKeyCodes;
        [SerializeField] private bool isCanSkippingText = true;

        [Space(10)]
        [SerializeField] private UnityEvent onDialogStarted;
        [SerializeField] private UnityEvent onDialogFinished;

        private DialogNodeGraph currentNodeGraph;
        private Node currentNode;

        private int maxAmountOfAnswerButtons;

        private bool isDialogStarted;
        private bool isCurrentSentenceSkipped;

        public bool IsCanSkippingText
        {
            get
            {
                return isCanSkippingText;
            }
            set
            {
                isCanSkippingText = value;
            }
        }

        public event Action OnSentenceNodeActive;

        public event Action<string, string, Sprite> OnSentenceNodeActiveWithParameter;

        public event Action OnAnswerNodeActive;

        public event Action<int, AnswerNode> OnAnswerButtonSetUp;

        public event Action<int> OnMaxAmountOfAnswerButtonsCalculated;

        public event Action<int> OnAnswerNodeActiveWithParameter;

        public event Action<int, string> OnAnswerNodeSetUp;

        public event Action OnDialogTextCharWrote;

        public event Action<string> OnDialogTextSkipped;

        public DialogExternalFunctionsHandler ExternalFunctionsHandler { get; private set; }

        private void Awake()
        {
            ExternalFunctionsHandler = new DialogExternalFunctionsHandler();
        }

        private void Update()
        {
            HandleSentenceSkipping();
        }

        /// <summary>
        /// Setting dialogCharDelay float parameter
        /// </summary>
        /// <param name="value"></param>
        public void SetCharDelay(float value)
        {
            dialogCharDelay = value;
        }

        /// <summary>
        /// Setting nextSentenceKeyCodes
        /// </summary>
        /// <param name="keyCodes"></param>
        public void SetNextSentenceKeyCodes(List<KeyCode> keyCodes)
        {
            nextSentenceKeyCodes = keyCodes;
        }

        /// <summary>
        /// Start a dialog
        /// </summary>
        /// <param name="dialogNodeGraph"></param>
        public void StartDialog(DialogNodeGraph dialogNodeGraph)
        {
            isDialogStarted = true;

            if (dialogNodeGraph.nodesList == null)
            {
                Debug.LogWarning("Dialog Graph's node list is empty");
                return;
            }

            onDialogStarted?.Invoke();

            currentNodeGraph = dialogNodeGraph;

            DefineFirstNode(dialogNodeGraph);
            CalculateMaxAmountOfAnswerButtons();
            HandleDialogGraphCurrentNode(currentNode);
        }

        /// <summary>
        /// This method is designed for ease of use. Calls a method 
        /// BindExternalFunction of the class DialogExternalFunctionsHandler
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="function"></param>
        public void BindExternalFunction(string funcName, Action function)
        {
            ExternalFunctionsHandler.BindExternalFunction(funcName, function);
        }

        /// <summary>
        /// Adding listener to OnDialogFinished UnityEvent
        /// </summary>
        /// <param name="action"></param>
        public void AddListenerToDialogFinishedEvent(UnityAction action)
        {
            onDialogFinished.AddListener(action);
        }

        /// <summary>
        /// Setting currentNode field to Node and call HandleDialogGraphCurrentNode method
        /// </summary>
        /// <param name="node"></param>
        public void SetCurrentNodeAndHandleDialogGraph(Node node)
        {
            currentNode = node;
            HandleDialogGraphCurrentNode(this.currentNode);
        }

        /// <summary>
        /// Processing dialog current node
        /// </summary>
        /// <param name="currentNode"></param>
        private void HandleDialogGraphCurrentNode(Node currentNode)
        {
            StopAllCoroutines();

            if (currentNode.GetType() == typeof(SentenceNode))
            {
                HandleSentenceNode(currentNode);
            }
            else if (currentNode.GetType() == typeof(AnswerNode))
            {
                HandleAnswerNode(currentNode);
            }
        }

        /// <summary>
        /// Processing sentence node
        /// </summary>
        /// <param name="currentNode"></param>
        private void HandleSentenceNode(Node currentNode)
        {
            SentenceNode sentenceNode = (SentenceNode)currentNode;

            isCurrentSentenceSkipped = false;

            OnSentenceNodeActive?.Invoke();
            OnSentenceNodeActiveWithParameter?.Invoke(sentenceNode.GetSentenceCharacterName(), sentenceNode.GetSentenceText(),
                sentenceNode.GetCharacterSprite());

            if (sentenceNode.IsExternalFunc())
            {
                ExternalFunctionsHandler.CallExternalFunction(sentenceNode.GetExternalFunctionName());
            }

            WriteDialogText(sentenceNode.GetSentenceText());
        }

        /// <summary>
        /// Processing answer node
        /// </summary>
        /// <param name="currentNode"></param>
        private void HandleAnswerNode(Node currentNode)
        {
            AnswerNode answerNode = (AnswerNode)currentNode;

            int amountOfActiveButtons = 0;

            OnAnswerNodeActive?.Invoke();

            for (int i = 0; i < answerNode.childSentenceNodes.Count; i++)
            {
                if (answerNode.childSentenceNodes[i] != null)
                {
                    OnAnswerNodeSetUp?.Invoke(i, answerNode.answers[i]);
                    OnAnswerButtonSetUp?.Invoke(i, answerNode);

                    amountOfActiveButtons++;
                }
                else
                {
                    break;
                }
            }

            if (amountOfActiveButtons == 0)
            {
                isDialogStarted = false;

                onDialogFinished?.Invoke();
                return;
            }

            OnAnswerNodeActiveWithParameter?.Invoke(amountOfActiveButtons);
        }

        /// <summary>
        /// Finds the first node that does not have a parent node but has a child one
        /// </summary>
        /// <param name="dialogNodeGraph"></param>
        private void DefineFirstNode(DialogNodeGraph dialogNodeGraph)
        {
            if (dialogNodeGraph.nodesList.Count == 0)
            {
                Debug.LogWarning("The list of nodes in the DialogNodeGraph is empty");

                return;
            }

            foreach (Node node in dialogNodeGraph.nodesList)
            {
                currentNode = node;

                if (node.GetType() == typeof(SentenceNode))
                {
                    SentenceNode sentenceNode = (SentenceNode)node;

                    if (sentenceNode.parentNode == null && sentenceNode.childNode != null)
                    {
                        currentNode = sentenceNode;

                        return;
                    }
                }
            }

            currentNode = dialogNodeGraph.nodesList[0];
        }

        /// <summary>
        /// Writing dialog text
        /// </summary>
        /// <param name="text"></param>
        private void WriteDialogText(string text)
        {
            StartCoroutine(WriteDialogTextRoutine(text));
        }

        /// <summary>
        /// Writing dialog text coroutine
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private IEnumerator WriteDialogTextRoutine(string text)
        {
            foreach (char textChar in text)
            {
                if (isCurrentSentenceSkipped)
                {
                    OnDialogTextSkipped?.Invoke(text);
                    break;
                }

                OnDialogTextCharWrote?.Invoke();

                yield return new WaitForSeconds(dialogCharDelay);
            }

            yield return new WaitUntil(CheckNextSentenceKeyCodes);

            CheckForDialogNextNode();
        }

        /// <summary>
        /// Checking is next dialog node has a child node
        /// </summary>
        private void CheckForDialogNextNode()
        {
            if (currentNode.GetType() == typeof(SentenceNode))
            {
                SentenceNode sentenceNode = (SentenceNode)currentNode;

                if (sentenceNode.childNode != null)
                {
                    currentNode = sentenceNode.childNode;
                    HandleDialogGraphCurrentNode(currentNode);
                }
                else
                {
                    isDialogStarted = false;

                    onDialogFinished?.Invoke();
                }
            }
        }

        /// <summary>
        /// Calculate max amount of answer buttons
        /// </summary>
        private void CalculateMaxAmountOfAnswerButtons()
        {
            foreach (Node node in currentNodeGraph.nodesList)
            {
                if (node.GetType() == typeof(AnswerNode))
                {
                    AnswerNode answerNode = (AnswerNode)node;

                    if (answerNode.answers.Count > maxAmountOfAnswerButtons)
                    {
                        maxAmountOfAnswerButtons = answerNode.answers.Count;
                    }
                }
            }

            OnMaxAmountOfAnswerButtonsCalculated?.Invoke(maxAmountOfAnswerButtons);
        }

        /// <summary>
        /// Handles text skipping mechanics
        /// </summary>
        private void HandleSentenceSkipping()
        {
            if (!isDialogStarted || !isCanSkippingText)
            {
                return;
            }

            if (CheckNextSentenceKeyCodes() && !isCurrentSentenceSkipped)
            {
                isCurrentSentenceSkipped = true;
            }
        }

        /// <summary>
        /// Checking whether at least one key from the nextSentenceKeyCodes was pressed
        /// </summary>
        /// <returns></returns>
        private bool CheckNextSentenceKeyCodes()
        {
            for (int i = 0; i < nextSentenceKeyCodes.Count; i++)
            { 
                if (Input.GetKeyDown(nextSentenceKeyCodes[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}