using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace cherrydev
{
    public class DialogBehaviour : MonoBehaviour
    {
        [SerializeField] private float dialogCharDelay;
        [SerializeField] private KeyCode nextSentenceKeyCode;

        [Space(7)]
        [SerializeField] private UnityEvent onDialogStart;
        [SerializeField] private UnityEvent onDialogFinished;

        private DialogNodeGraph currentNodeGraph;
        private Node currentNode;

        private int maxAmountOfAnswerButtons;

        public static event Action OnSentenceNodeActive;

        public static event Action OnDialogSentenceEnd;

        public static event Action<string, Sprite> OnSentenceNodeActiveWithParameter;

        public static event Action OnAnswerNodeActive;

        public static event Action<int, AnswerNode> OnAnswerButtonSetUp;

        public static event Action<int> OnMaxAmountOfAnswerButtonsCalculated;

        public static event Action<int> OnAnswerNodeActiveWithParameter;

        public static event Action<int, string> OnAnswerNodeSetUp;

        public static event Action<char> OnDialogTextCharWrote;

        /// <summary>
        /// Start a dialog
        /// </summary>
        /// <param name="dialogNodeGraph"></param>
        public void StartDialog(DialogNodeGraph dialogNodeGraph)
        {
            if (dialogNodeGraph.nodesList == null)
            {
                Debug.LogWarning("Dialog Graph's node list is empty");
                return;
            }

            onDialogStart?.Invoke();

            currentNodeGraph = dialogNodeGraph;

            DefineFirstNode(dialogNodeGraph);
            CalculateMaxAmountOfAnswerButtons();
            HandleDialogGraphCurrentNode(currentNode);
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
                SentenceNode sentenceNode = (SentenceNode)currentNode;

                OnSentenceNodeActive?.Invoke();
                OnSentenceNodeActiveWithParameter?.Invoke(sentenceNode.GetSentenceCharacterName(),
                    sentenceNode.GetCharacterSprite());

                WriteDialogText(sentenceNode.GetSentenceText());
            }
            else if (currentNode.GetType() == typeof(AnswerNode))
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
                    onDialogFinished?.Invoke();
                    return;
                }

                OnAnswerNodeActiveWithParameter?.Invoke(amountOfActiveButtons);
            }
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
                yield return new WaitForSeconds(dialogCharDelay);
                OnDialogTextCharWrote?.Invoke(textChar);
            }

            yield return new WaitUntil(() => Input.GetKeyDown(nextSentenceKeyCode));

            OnDialogSentenceEnd?.Invoke();
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
    }
}