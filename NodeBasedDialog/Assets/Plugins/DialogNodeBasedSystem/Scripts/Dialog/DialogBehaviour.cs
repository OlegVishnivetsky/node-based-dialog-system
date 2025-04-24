using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_LOCALIZATION
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
#endif

namespace cherrydev
{
    public class DialogBehaviour : MonoBehaviour
    {
        [SerializeField] private float _dialogCharDelay;
        [SerializeField] private List<KeyCode> _nextSentenceKeyCodes;
        [SerializeField] private bool _isCanSkippingText = true;
#if UNITY_LOCALIZATION
        [SerializeField] private bool _reloadTextOnLanguageChange = true;
#endif

        [Space(10)]
        [SerializeField] private UnityEvent _onDialogStarted;
        [SerializeField] private UnityEvent _onDialogFinished;

        private DialogNodeGraph _currentNodeGraph;
        private Node _currentNode;
        
        public AnswerNode CurrentAnswerNode { get; private set; }
        public SentenceNode CurrentSentenceNode { get; private set; }
        
#if UNITY_LOCALIZATION
        public event Action LanguageChanged;
#endif
        
        private int _maxAmountOfAnswerButtons;

        private bool _isDialogStarted;
        private bool _isCurrentSentenceSkipped;
        private bool _isCurrentSentenceTyping;

        public bool IsCanSkippingText
        {
            get => _isCanSkippingText;
            set => _isCanSkippingText = value;
        }

        public event Action SentenceStarted;
        public event Action SentenceEnded;
        public event Action SentenceNodeActivated;
        public event Action<string, string, Sprite> SentenceNodeActivatedWithParameter;
        public event Action AnswerNodeActivated;
        public event Action<int, AnswerNode> AnswerButtonSetUp;
        public event Action<int> MaxAmountOfAnswerButtonsCalculated;
        public event Action<int> AnswerNodeActivatedWithParameter;
        public event Action<int, string> AnswerNodeSetUp;
        public event Action DialogTextCharWrote;
        public event Action<string> DialogTextSkipped;

        public DialogExternalFunctionsHandler ExternalFunctionsHandler { get; private set; }

        private void Awake() => ExternalFunctionsHandler = new DialogExternalFunctionsHandler();

        private void OnEnable()
        {
#if UNITY_LOCALIZATION
            if (_reloadTextOnLanguageChange)
                LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
#endif
        }

#if UNITY_LOCALIZATION
        private void OnSelectedLocaleChanged(Locale obj)
        {
            if (_isDialogStarted && _currentNode != null)
            {
                LanguageChanged?.Invoke();

                if (_currentNode is SentenceNode sentenceNode)
                {
                    string updatedText = sentenceNode.GetText();
                    string updatedCharName = sentenceNode.GetCharacterName();

                    SentenceNodeActivatedWithParameter?.Invoke(updatedCharName, updatedText,
                        sentenceNode.GetCharacterSprite());

                    if (_isCurrentSentenceTyping)
                    {
                        StopAllCoroutines();
                        WriteDialogText(updatedText);
                    }
                    else
                        DialogTextSkipped?.Invoke(updatedText);
                }
                else if (_currentNode is AnswerNode)
                    HandleAnswerNode(_currentNode);
            }
        }
#endif

        private void OnDestroy()
        {
#if UNITY_LOCALIZATION
            if (_reloadTextOnLanguageChange)
                LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
#endif
        }
        
        private void Update() => HandleSentenceSkipping();

        /// <summary>
        /// Setting dialogCharDelay float parameter
        /// </summary>
        /// <param name="value"></param>
        public void SetCharDelay(float value) => _dialogCharDelay = value;

        /// <summary>
        /// Setting nextSentenceKeyCodes
        /// </summary>
        /// <param name="keyCodes"></param>
        public void SetNextSentenceKeyCodes(List<KeyCode> keyCodes) => _nextSentenceKeyCodes = keyCodes;

        /// <summary>
        /// Start a dialog
        /// </summary>
        /// <param name="dialogNodeGraph"></param>
        public void StartDialog(DialogNodeGraph dialogNodeGraph)
        {
            _isDialogStarted = true;

            if (dialogNodeGraph.NodesList == null)
            {
                Debug.LogWarning("Dialog Graph's node list is empty");
                return;
            }

            _onDialogStarted?.Invoke();
            _currentNodeGraph = dialogNodeGraph;

            DefineFirstNode(dialogNodeGraph);
            CalculateMaxAmountOfAnswerButtons();
            HandleDialogGraphCurrentNode(_currentNode);
        }

        /// <summary>
        /// This method is designed for ease of use. Calls a method 
        /// BindExternalFunction of the class DialogExternalFunctionsHandler
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="function"></param>
        public void BindExternalFunction(string funcName, Action function) => 
            ExternalFunctionsHandler.BindExternalFunction(funcName, function);

        /// <summary>
        /// Adding listener to OnDialogFinished UnityEvent
        /// </summary>
        /// <param name="action"></param>
        public void AddListenerToDialogFinishedEvent(UnityAction action) => 
            _onDialogFinished.AddListener(action);

        /// <summary>
        /// Setting currentNode field to Node and call HandleDialogGraphCurrentNode method
        /// </summary>
        /// <param name="node"></param>
        public void SetCurrentNodeAndHandleDialogGraph(Node node)
        {
            _currentNode = node;
            HandleDialogGraphCurrentNode(this._currentNode);
        }

        /// <summary>
        /// Processing dialog current node
        /// </summary>
        /// <param name="currentNode"></param>
        private void HandleDialogGraphCurrentNode(Node currentNode)
        {
            StopAllCoroutines();

            if (currentNode.GetType() == typeof(SentenceNode))
                HandleSentenceNode(currentNode);
            else if (currentNode.GetType() == typeof(AnswerNode))
                HandleAnswerNode(currentNode);
        }

        /// <summary>
        /// Processing sentence node
        /// </summary>
        /// <param name="currentNode"></param>
        private void HandleSentenceNode(Node currentNode)
        {
            SentenceNode sentenceNode = (SentenceNode)currentNode;
            CurrentSentenceNode = sentenceNode;

            _isCurrentSentenceSkipped = false;

            SentenceNodeActivated?.Invoke();
    
            string localizedCharName = sentenceNode.GetCharacterName();
            string localizedText = sentenceNode.GetText();
            
            SentenceNodeActivatedWithParameter?.Invoke(localizedCharName, localizedText,
                sentenceNode.GetCharacterSprite());

            if (sentenceNode.IsExternalFunc())
                ExternalFunctionsHandler.CallExternalFunction(sentenceNode.GetExternalFunctionName());
    
            WriteDialogText(localizedText);
        }

        /// <summary>
        /// Processing answer node
        /// </summary>
        /// <param name="currentNode"></param>
        private void HandleAnswerNode(Node currentNode)
        {
            AnswerNode answerNode = (AnswerNode)currentNode;
            CurrentAnswerNode = answerNode;
        
            int amountOfActiveButtons = 0;

            AnswerNodeActivated?.Invoke();

            for (int i = 0; i < answerNode.ChildSentenceNodes.Count; i++)
            {
                if (answerNode.ChildSentenceNodes[i])
                {
                    AnswerNodeSetUp?.Invoke(i, answerNode.Answers[i]);
                    AnswerButtonSetUp?.Invoke(i, answerNode);

                    amountOfActiveButtons++;
                }
                else
                    break;
            }

            if (amountOfActiveButtons == 0)
            {
                _isDialogStarted = false;

                _onDialogFinished?.Invoke();
                return;
            }

            AnswerNodeActivatedWithParameter?.Invoke(amountOfActiveButtons);
        }

        /// <summary>
        /// Finds the first node that does not have a parent node but has a child one
        /// </summary>
        /// <param name="dialogNodeGraph"></param>
        private void DefineFirstNode(DialogNodeGraph dialogNodeGraph)
        {
            if (dialogNodeGraph.NodesList.Count == 0)
            {
                Debug.LogWarning("The list of nodes in the DialogNodeGraph is empty");
                return;
            }

            foreach (Node node in dialogNodeGraph.NodesList)
            {
                _currentNode = node;

                if (node.GetType() == typeof(SentenceNode))
                {
                    SentenceNode sentenceNode = (SentenceNode)node;

                    if (sentenceNode.ParentNode == null && sentenceNode.ChildNode != null)
                    {
                        _currentNode = sentenceNode;
                        return;
                    }
                }
            }

            _currentNode = dialogNodeGraph.NodesList[0];
        }

        /// <summary>
        /// Writing dialog text
        /// </summary>
        /// <param name="text"></param>
        private void WriteDialogText(string text) => StartCoroutine(WriteDialogTextRoutine(text));

        /// <summary>
        /// Writing dialog text coroutine
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private IEnumerator WriteDialogTextRoutine(string text)
        {
            _isCurrentSentenceTyping = true;
            SentenceStarted?.Invoke();
            
            foreach (char textChar in text)
            {
                if (_isCurrentSentenceSkipped)
                {
                    DialogTextSkipped?.Invoke(text);
                    _isCurrentSentenceTyping = false;
                    break;
                }

                DialogTextCharWrote?.Invoke();

                yield return new WaitForSeconds(_dialogCharDelay);
            }

            _isCurrentSentenceTyping = false;
            SentenceEnded?.Invoke();
            
            yield return new WaitUntil(CheckNextSentenceKeyCodes);

            CheckForDialogNextNode();
        }

        /// <summary>
        /// Checking is next dialog node has a child node
        /// </summary>
        private void CheckForDialogNextNode()
        {
            if (_currentNode.GetType() == typeof(SentenceNode))
            {
                SentenceNode sentenceNode = (SentenceNode)_currentNode;

                if (sentenceNode.ChildNode != null)
                {
                    _currentNode = sentenceNode.ChildNode;
                    HandleDialogGraphCurrentNode(_currentNode);
                }
                else
                {
                    _isDialogStarted = false;
                    _onDialogFinished?.Invoke();
                }
            }
        }

        /// <summary>
        /// Calculate max amount of answer buttons
        /// </summary>
        private void CalculateMaxAmountOfAnswerButtons()
        {
            foreach (Node node in _currentNodeGraph.NodesList)
            {
                if (node.GetType() == typeof(AnswerNode))
                {
                    AnswerNode answerNode = (AnswerNode)node;

                    if (answerNode.Answers.Count > _maxAmountOfAnswerButtons)
                        _maxAmountOfAnswerButtons = answerNode.Answers.Count;
                }
            }

            MaxAmountOfAnswerButtonsCalculated?.Invoke(_maxAmountOfAnswerButtons);
        }

        /// <summary>
        /// Handles text skipping mechanics
        /// </summary>
        private void HandleSentenceSkipping()
        {
            if (!_isDialogStarted || !_isCanSkippingText)
                return;
            
            if (CheckNextSentenceKeyCodes() && !_isCurrentSentenceSkipped)
                _isCurrentSentenceSkipped = true;
        }

        /// <summary>
        /// Checking whether at least one key from the nextSentenceKeyCodes was pressed
        /// </summary>
        /// <returns></returns>
        private bool CheckNextSentenceKeyCodes()
        {
            for (int i = 0; i < _nextSentenceKeyCodes.Count; i++)
            { 
                if (Input.GetKeyDown(_nextSentenceKeyCodes[i]))
                    return true;
            }

            return false;
        }
    }
}