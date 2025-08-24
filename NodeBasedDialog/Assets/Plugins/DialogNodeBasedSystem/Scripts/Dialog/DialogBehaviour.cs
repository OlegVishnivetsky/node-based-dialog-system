using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private DialogVariablesHandler _variablesHandler;

        public AnswerNode CurrentAnswerNode { get; private set; }
        public SentenceNode CurrentSentenceNode { get; private set; }
        public ModifyVariableNode CurrentModifyVariableNode { get; private set; }
        public VariableConditionNode CurrentVariableConditionNode { get; private set; }
        public ExternalFunctionNode CurrentExternalFunctionNode { get; private set; }
        
        public UnityEvent OnDialogStarted => _onDialogStarted;
        public UnityEvent OnDialogFinished => _onDialogFinished;

#if UNITY_LOCALIZATION
        public event Action LanguageChanged;
#endif

        private int _maxAmountOfAnswerButtons;

        private bool _isDialogStarted;
        private bool _isCurrentSentenceSkipped;
        private bool _isCurrentSentenceTyping;

        private readonly List<string> _boundFunctionNames = new();

        public bool IsActive { get; set; } = true;

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
        public event Action DialogDisabled;

        public event Action<ModifyVariableNode> ModifyVariableNodeActivated;
        public event Action<string> VariableChanged;
        public event Action<string, object> VariableValueChanged;

        public event Action<VariableConditionNode> VariableConditionNodeActivated;
        public event Action<string, bool> VariableConditionEvaluated;
        
        private event Action<DialogVariablesHandler> _dialogFinished;


        public DialogExternalFunctionsHandler ExternalFunctionsHandler { get; private set; }
        public DialogVariablesHandler VariablesHandler => _variablesHandler;

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

                    if (_variablesHandler != null)
                    {
                        updatedText = DialogTextProcessor.ProcessText(updatedText, _variablesHandler);
                        updatedCharName = DialogTextProcessor.ProcessText(updatedCharName, _variablesHandler);
                    }

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

            if (_variablesHandler != null)
            {
                _variablesHandler.VariableChanged -= OnVariableChanged;
                _variablesHandler.VariableModified -= OnVariableModified;
            }
        }

        private void Update() => HandleSentenceSkipping();

        /// <summary>
        /// Disable dialog panel
        /// </summary>
        public void Disable() => DialogDisabled?.Invoke();

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
        /// <param name="onVariablesHandlerInitialized"></param>
        /// <param name="onDialogFinished"></param>
        public void StartDialog(
            DialogNodeGraph dialogNodeGraph, 
            Action<DialogVariablesHandler> onVariablesHandlerInitialized = null, 
            Action<DialogVariablesHandler> onDialogFinished = null)
        {
            _isDialogStarted = true;
            _boundFunctionNames.Clear();

            if (dialogNodeGraph.NodesList == null)
            {
                Debug.LogWarning("Dialog Graph's node list is empty");
                return;
            }

            _onDialogStarted?.Invoke();
            _currentNodeGraph = dialogNodeGraph;

            InitializeVariablesHandler(dialogNodeGraph);
            
            onVariablesHandlerInitialized?.Invoke(_variablesHandler);
            _dialogFinished = onDialogFinished;
            
            DefineFirstNode(dialogNodeGraph);
            CalculateMaxAmountOfAnswerButtons();
            HandleDialogGraphCurrentNode(_currentNode);
        }

        /// <summary>
        /// Initialize the variables handler for this dialog
        /// </summary>
        /// <param name="dialogNodeGraph"></param>
        private void InitializeVariablesHandler(DialogNodeGraph dialogNodeGraph)
        {
            if (_variablesHandler != null)
            {
                _variablesHandler.VariableChanged -= OnVariableChanged;
                _variablesHandler.VariableModified -= OnVariableModified;
            }

            if (dialogNodeGraph.VariablesConfig != null)
            {
                _variablesHandler = new DialogVariablesHandler(dialogNodeGraph.VariablesConfig);
                _variablesHandler.VariableChanged += OnVariableChanged;
                _variablesHandler.VariableModified += OnVariableModified;
            }
        }

        /// <summary>
        /// Called when a variable changes
        /// </summary>
        /// <param name="variableName"></param>
        private void OnVariableChanged(string variableName)
        {
            VariableChanged?.Invoke(variableName);

            if (_variablesHandler != null)
            {
                Variable variable = _variablesHandler.GetVariable(variableName);

                if (variable != null)
                    VariableValueChanged?.Invoke(variableName, variable.GetValue());
            }
        }

        /// <summary>
        /// Called when a modify variable node is executed
        /// </summary>
        /// <param name="modifyNode"></param>
        private void OnVariableModified(ModifyVariableNode modifyNode) =>
            ModifyVariableNodeActivated?.Invoke(modifyNode);

        /// <summary>
        /// Get variable value by name
        /// </summary>
        /// <typeparam name="T">Type of the variable</typeparam>
        /// <param name="variableName">Name of the variable</param>
        /// <returns>Variable value</returns>
        public T GetVariableValue<T>(string variableName)
        {
            if (_variablesHandler == null)
                return default!;

            return _variablesHandler.GetVariableValue<T>(variableName);
        }

        /// <summary>
        /// Set variable value by name
        /// </summary>
        /// <param name="variableName">Name of the variable</param>
        /// <param name="value">Value to set</param>
        public void SetVariableValue(string variableName, object value) =>
            _variablesHandler?.SetVariableValue(variableName, value);

        /// <summary>
        /// Set variable value directly
        /// </summary>
        public void SetVariableValue(string variableName, bool value) =>
            _variablesHandler?.SetVariableValueDirect(variableName, value);

        public void SetVariableValue(string variableName, int value) =>
            _variablesHandler?.SetVariableValueDirect(variableName, value);

        public void SetVariableValue(string variableName, float value) =>
            _variablesHandler?.SetVariableValueDirect(variableName, value);

        public void SetVariableValue(string variableName, string value) =>
            _variablesHandler?.SetVariableValueDirect(variableName, value);

        /// <summary>
        /// This method is designed for ease of use. Calls a method 
        /// BindExternalFunction of the class DialogExternalFunctionsHandler
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="function"></param>
        public void BindExternalFunction(string funcName, Action function)
        {
            ExternalFunctionsHandler.BindExternalFunction(funcName, function);

            if (!_boundFunctionNames.Contains(funcName))
                _boundFunctionNames.Add(funcName);
        }

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
            HandleDialogGraphCurrentNode(_currentNode);
        }

        /// <summary>
        /// Setting currentNode field to Node and call HandleDialogGraphCurrentNode method
        /// This method should be called when an answer button is clicked with the button index
        /// </summary>
        /// <param name="answerIndex">Index of the selected answer</param>
        public void SetCurrentNodeAndHandleDialogGraph(int answerIndex)
        {
            if (CurrentAnswerNode != null && answerIndex >= 0 && answerIndex < CurrentAnswerNode.ChildNodes.Count)
            {
                Node selectedNode = CurrentAnswerNode.ChildNodes[answerIndex];
                if (selectedNode != null)
                {
                    _currentNode = selectedNode;
                    HandleDialogGraphCurrentNode(_currentNode);
                }
                else
                {
                    Debug.LogWarning($"No child node found at index {answerIndex}");
                    EndDialog();
                }
            }
            else
            {
                Debug.LogWarning("Invalid answer index or no current answer node");
                EndDialog();
            }
        }

        public void PerformSentenceNode(SentenceNode sentenceNode, float progress)
        {
            if (sentenceNode == null)
                return;

            CurrentSentenceNode = sentenceNode;
            SentenceNodeActivated?.Invoke();

            string charName = sentenceNode.GetCharacterName();
            string fullText = sentenceNode.GetText();
            Sprite charSprite = sentenceNode.GetCharacterSprite();

            if (_variablesHandler != null)
            {
                charName = DialogTextProcessor.ProcessText(charName, _variablesHandler);
                fullText = DialogTextProcessor.ProcessText(fullText, _variablesHandler);
            }

            SentenceNodeActivatedWithParameter?.Invoke(charName, fullText, charSprite);

            if (!string.IsNullOrEmpty(fullText))
            {
                int charsToShow = Mathf.CeilToInt(fullText.Length * progress);
                charsToShow = Mathf.Clamp(charsToShow, 0, fullText.Length);
                string subText = fullText.Substring(0, charsToShow);
                DialogTextSkipped?.Invoke(subText);
            }
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
            else if (currentNode.GetType() == typeof(ModifyVariableNode))
                HandleModifyVariableNode(currentNode);
            else if (currentNode.GetType() == typeof(VariableConditionNode))
                HandleVariableConditionNode(currentNode);
            else if (currentNode.GetType() == typeof(ExternalFunctionNode))
                HandleExternalFunctionNode(currentNode);
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

            if (_variablesHandler != null)
            {
                localizedCharName = DialogTextProcessor.ProcessText(localizedCharName, _variablesHandler);
                localizedText = DialogTextProcessor.ProcessText(localizedText, _variablesHandler);
            }

            SentenceNodeActivatedWithParameter?.Invoke(localizedCharName, localizedText,
                sentenceNode.GetCharacterSprite());

            if (sentenceNode.IsExternalFunc())
                CallExternalFunction(sentenceNode.GetExternalFunctionName());

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

            for (int i = 0; i < answerNode.ChildNodes.Count; i++)
            {
                if (answerNode.ChildNodes[i] != null)
                {
                    string answerText = answerNode.Answers[i];

                    if (_variablesHandler != null)
                        answerText = DialogTextProcessor.ProcessText(answerText, _variablesHandler);

                    AnswerNodeSetUp?.Invoke(i, answerText);
                    AnswerButtonSetUp?.Invoke(i, answerNode);

                    amountOfActiveButtons++;
                }
                else
                    break;
            }

            if (amountOfActiveButtons == 0)
            {
                EndDialog();
                return;
            }

            AnswerNodeActivatedWithParameter?.Invoke(amountOfActiveButtons);
        }

        /// <summary>
        /// Processing modify variable node
        /// </summary>
        /// <param name="currentNode"></param>
        private void HandleModifyVariableNode(Node currentNode)
        {
            ModifyVariableNode modifyVariableNode = (ModifyVariableNode)currentNode;
            CurrentModifyVariableNode = modifyVariableNode;

            if (_variablesHandler != null)
                _variablesHandler.ExecuteModifyVariableNode(modifyVariableNode);
            else
                Debug.LogWarning("Variables handler is null, cannot execute ModifyVariableNode");

            if (modifyVariableNode.ChildNode != null)
            {
                _currentNode = modifyVariableNode.ChildNode;
                HandleDialogGraphCurrentNode(_currentNode);
            }
            else
                EndDialog();
        }

        /// <summary>
        /// Processing variable condition node
        /// </summary>
        /// <param name="currentNode"></param>
        private void HandleVariableConditionNode(Node currentNode)
        {
            VariableConditionNode variableConditionNode = (VariableConditionNode)currentNode;
            CurrentVariableConditionNode = variableConditionNode;

            if (_variablesHandler == null)
            {
                Debug.LogWarning("Variables handler is null, cannot evaluate VariableConditionNode");
                EndDialog();
                return;
            }

            bool conditionResult = variableConditionNode.EvaluateCondition(_variablesHandler);

            VariableConditionNodeActivated?.Invoke(variableConditionNode);
            VariableConditionEvaluated?.Invoke(variableConditionNode.VariableName, conditionResult);

            Node nextNode = null;

            if (conditionResult)
            {
                nextNode = variableConditionNode.TrueChildNode;
                Debug.Log($"Variable condition '{variableConditionNode.VariableName}' evaluated to TRUE");
            }
            else
            {
                nextNode = variableConditionNode.FalseChildNode;
                Debug.Log($"Variable condition '{variableConditionNode.VariableName}' evaluated to FALSE");
            }

            if (nextNode != null)
            {
                _currentNode = nextNode;
                HandleDialogGraphCurrentNode(_currentNode);
            }
            else
            {
                Debug.LogWarning(
                    $"No {(conditionResult ? "TRUE" : "FALSE")} path connected for variable condition node");
                EndDialog();
            }
        }

        /// <summary>
        /// Processing external function node
        /// </summary>
        /// <param name="currentNode"></param>
        private void HandleExternalFunctionNode(Node currentNode)
        {
            ExternalFunctionNode externalFunctionNode = (ExternalFunctionNode)currentNode;
            CurrentExternalFunctionNode = externalFunctionNode;

            ExternalFunctionsHandler.CallExternalFunction(externalFunctionNode.GetExternalFunctionName());

            if (externalFunctionNode.ChildNode != null)
            {
                _currentNode = externalFunctionNode.ChildNode;
                HandleDialogGraphCurrentNode(_currentNode);
            }
            else
                EndDialog();
        }

        /// <summary>
        /// Ends the dialog and unbinds all tracked external functions
        /// </summary>
        private void EndDialog()
        {
            _isDialogStarted = false;

            _dialogFinished?.Invoke(_variablesHandler);
            
            foreach (string funcName in _boundFunctionNames)
                ExternalFunctionsHandler.UnbindExternalFunction(funcName);

            _boundFunctionNames.Clear();
            _onDialogFinished?.Invoke();
            _dialogFinished = null;
        }

        /// <summary>
        /// Finds the first node that does not have any parent nodes but has child connections
        /// Updated to work with multiple parents system
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
                bool hasParents = false;
                bool hasChildren = false;

                if (node.GetType() == typeof(SentenceNode))
                {
                    SentenceNode sentenceNode = (SentenceNode)node;
                    hasParents = sentenceNode.ParentNodes.Count > 0;
                    hasChildren = sentenceNode.ChildNode != null;
                }
                else if (node.GetType() == typeof(AnswerNode))
                {
                    AnswerNode answerNode = (AnswerNode)node;
                    hasParents = answerNode.ParentNodes.Count > 0;
                    hasChildren = answerNode.ChildNodes.Count > 0 && answerNode.ChildNodes.Any(child => child != null);
                }
                else if (node.GetType() == typeof(ExternalFunctionNode))
                {
                    ExternalFunctionNode externalFunctionNode = (ExternalFunctionNode)node;
                    hasParents = externalFunctionNode.ParentNodes.Count > 0;
                    hasChildren = externalFunctionNode.ChildNode != null;
                }
                else if (node.GetType() == typeof(ModifyVariableNode))
                {
                    ModifyVariableNode modifyVariableNode = (ModifyVariableNode)node;
                    hasParents = modifyVariableNode.ParentNodes.Count > 0;
                    hasChildren = modifyVariableNode.ChildNode != null;
                }
                else if (node.GetType() == typeof(VariableConditionNode))
                {
                    VariableConditionNode variableConditionNode = (VariableConditionNode)node;
                    hasParents = variableConditionNode.ParentNodes.Count > 0;
                    hasChildren = variableConditionNode.TrueChildNode != null ||
                                  variableConditionNode.FalseChildNode != null;
                }

                if (!hasParents && hasChildren)
                {
                    _currentNode = node;
                    return;
                }
            }

            _currentNode = dialogNodeGraph.NodesList[0];
            Debug.LogWarning("No clear starting node found (node without parents). Using first node in list.");
        }

        public void CallExternalFunction(string getExternalFunctionName) =>
            ExternalFunctionsHandler.CallExternalFunction(getExternalFunctionName);

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

            yield return new WaitUntil(() => CheckNextSentenceKeyCodes() && IsActive);

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
                    EndDialog();
            }
            else if (_currentNode.GetType() == typeof(ExternalFunctionNode))
            {
                ExternalFunctionNode externalFunctionNode = (ExternalFunctionNode)_currentNode;

                if (externalFunctionNode.ChildNode != null)
                {
                    _currentNode = externalFunctionNode.ChildNode;
                    HandleDialogGraphCurrentNode(_currentNode);
                }
                else
                    EndDialog();
            }
            else if (_currentNode.GetType() == typeof(ModifyVariableNode))
            {
                ModifyVariableNode modifyVariableNode = (ModifyVariableNode)_currentNode;

                if (modifyVariableNode.ChildNode != null)
                {
                    _currentNode = modifyVariableNode.ChildNode;
                    HandleDialogGraphCurrentNode(_currentNode);
                }
                else
                    EndDialog();
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