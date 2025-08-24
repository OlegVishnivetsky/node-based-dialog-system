using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_LOCALIZATION
using UnityEngine.Localization.Settings;
#endif

namespace cherrydev
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Node Graph/Nodes/Answer Node", 
        fileName = "New Answer Node")]
    public class AnswerNode : Node
    {
        private int _amountOfAnswers = 1;

        public List<string> Answers = new();
        public List<string> AnswerKeys = new();

        public List<Node> ParentNodes = new();
        public List<Node> ChildNodes = new();

        private const float LabelFieldSpace = 18f;
        private const float TextFieldWidth = 120f;

        private const float AnswerNodeWidth = 190f;
        private const float AnswerNodeHeight = 115f;

        private float _currentAnswerNodeHeight = 115f;
        private const float AdditionalAnswerNodeHeight = 20f;

        public string GetAnswerText(int index)
        {
            if (index < 0 || index >= Answers.Count)
                return string.Empty;

#if UNITY_LOCALIZATION
            if (index < AnswerKeys.Count && !string.IsNullOrEmpty(AnswerKeys[index]))
            {
                try
                {
                    string tableName = GetTableNameFromNodeGraph();
                    if (string.IsNullOrEmpty(tableName))
                        return Answers[index];
                
                    string localizedValue = LocalizationSettings.StringDatabase.GetLocalizedString(
                        tableName, AnswerKeys[index]);

                    if (!string.IsNullOrEmpty(localizedValue))
                        return localizedValue;
                    else
                        Debug.LogWarning($"Localized answer was empty for key: {AnswerKeys[index]}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to get localized answer: {ex.Message}");
                }
            }
#endif

            return Answers[index];
        }

#if UNITY_EDITOR

        /// <summary>
        /// Answer node initialisation method
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="nodeName"></param>
        /// <param name="nodeGraph"></param>
        public override void Initialize(Rect rect, string nodeName, DialogNodeGraph nodeGraph)
        {
            base.Initialize(rect, nodeName, nodeGraph);

            CalculateAmountOfAnswers();
            ChildNodes = new List<Node>(_amountOfAnswers);
        }

        /// <summary>
        /// Draw Answer Node method
        /// </summary>
        /// <param name = "nodeStyle" ></param>
        /// < param name="labelStyle"></param>
        public override void Draw(GUIStyle nodeStyle, GUIStyle labelStyle)
        {
            base.Draw(nodeStyle, labelStyle);

            ChildNodes.RemoveAll(item => item == null);
            ParentNodes.RemoveAll(item => item == null);

            float additionalHeight = DialogNodeGraph.ShowLocalizationKeys ? _amountOfAnswers * 20f : 0;
            Rect.size = new Vector2(AnswerNodeWidth, _currentAnswerNodeHeight + additionalHeight);

            GUILayout.BeginArea(Rect, nodeStyle);
            EditorGUILayout.LabelField("Answer Node", labelStyle);

            for (int i = 0; i < _amountOfAnswers; i++)
            {
                DrawAnswerLine(i + 1, StringConstants.GreenDot);
        
                if (DialogNodeGraph.ShowLocalizationKeys)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Key: ", GUILayout.Width(25));
                    
                    while (AnswerKeys.Count <= i)
                        AnswerKeys.Add(string.Empty);
                
                    AnswerKeys[i] = EditorGUILayout.TextField(AnswerKeys[i], GUILayout.Width(TextFieldWidth + 13));
                    EditorGUILayout.EndHorizontal();
                }
            }

            DrawAnswerNodeButtons();

            GUILayout.EndArea();
        }

        /// <summary>
        /// Removes all connections in a answer node
        /// </summary>
        public override void RemoveAllConnections()
        {
            ParentNodes.Clear();
            ChildNodes.Clear();
        }
        
        /// <summary>
        /// Determines the number of answers depending on answers list count
        /// </summary>
        public void CalculateAmountOfAnswers()
        {
            if (Answers.Count == 0)
            {
                _amountOfAnswers = 1;
                Answers = new List<string> { string.Empty };
            }
            else
                _amountOfAnswers = Answers.Count;
        }

        /// <summary>
        /// Draw answer line
        /// </summary>
        /// <param name="answerNumber"></param>
        /// <param name="iconPathOrName"></param>
        private void DrawAnswerLine(int answerNumber, string iconPathOrName)
        {
            GUIContent iconContent = EditorGUIUtility.IconContent(iconPathOrName);
            Texture2D fallbackTexture = Resources.Load<Texture2D>("Dot");
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{answerNumber}. ", GUILayout.Width(LabelFieldSpace));

            Answers[answerNumber - 1] = EditorGUILayout.TextField(Answers[answerNumber - 1],
                GUILayout.Width(TextFieldWidth));

            if (fallbackTexture == null)
                EditorGUILayout.LabelField(iconContent, GUILayout.Width(LabelFieldSpace));
            else
                GUILayout.Label(fallbackTexture, GUILayout.Width(LabelFieldSpace), GUILayout.Height(LabelFieldSpace));
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAnswerNodeButtons()
        {
            if (GUILayout.Button("Add answer"))
                IncreaseAmountOfAnswers();

            if (GUILayout.Button("Remove answer"))
                DecreaseAmountOfAnswers();
        }

        /// <summary>
        /// Increase amount of answers and node height
        /// </summary>
        private void IncreaseAmountOfAnswers()
        {
            _amountOfAnswers++;
            Answers.Add(string.Empty);
            _currentAnswerNodeHeight += AdditionalAnswerNodeHeight;
        }

        /// <summary>
        /// Decrease amount of answers and node height 
        /// </summary>
        private void DecreaseAmountOfAnswers()
        {
            if (Answers.Count == 1)
                return;

            Answers.RemoveAt(_amountOfAnswers - 1);

            if (ChildNodes.Count == _amountOfAnswers)
            {
                Node nodeToRemove = ChildNodes[_amountOfAnswers - 1];
        
                if (nodeToRemove != null)
                    nodeToRemove.RemoveFromParentConnectedNode(this);
        
                ChildNodes.RemoveAt(_amountOfAnswers - 1);
            }

            _amountOfAnswers--;
            _currentAnswerNodeHeight -= AdditionalAnswerNodeHeight;
        }

        /// <summary>
        /// Adding nodeToAdd Node to the parent connected nodes list
        /// </summary>
        /// <param name="nodeToAdd"></param>
        /// <returns></returns>
        public override bool AddToParentConnectedNode(Node nodeToAdd)
        {
            if (nodeToAdd == this)
                return false;

            if (ParentNodes.Contains(nodeToAdd))
                return false;

            if (nodeToAdd.GetType() == typeof(SentenceNode) 
                || nodeToAdd.GetType() == typeof(ModifyVariableNode)
                || nodeToAdd.GetType() == typeof(VariableConditionNode)
                || nodeToAdd.GetType() == typeof(ExternalFunctionNode))
            {
                ParentNodes.Add(nodeToAdd);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove a parent node connection
        /// </summary>
        /// <param name="nodeToRemove"></param>
        /// <returns></returns>
        public override bool RemoveFromParentConnectedNode(Node nodeToRemove) => ParentNodes.Remove(nodeToRemove);

        /// <summary>
        /// Adding nodeToAdd Node to the child nodes list (supports all node types)
        /// </summary>
        /// <param name="nodeToAdd"></param>
        /// <returns></returns>
        public override bool AddToChildConnectedNode(Node nodeToAdd)
        {
            if (nodeToAdd.GetType() == typeof(AnswerNode))
                return false;

            if (IsCanAddToChildConnectedNode(nodeToAdd))
            {
                ChildNodes.Add(nodeToAdd);
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Calculate answer node height based on amount of answers
        /// </summary>
        public void CalculateAnswerNodeHeight()
        {
            _currentAnswerNodeHeight = AnswerNodeHeight;

            for (int i = 0; i < _amountOfAnswers - 1; i++)
                _currentAnswerNodeHeight += AdditionalAnswerNodeHeight;
        }

        /// <summary>
        /// Checks if node can be added as child of answer node
        /// </summary>
        /// <param name="nodeToAdd"></param>
        /// <returns></returns>
        private bool IsCanAddToChildConnectedNode(Node nodeToAdd)
        {
            if (ChildNodes.Count >= _amountOfAnswers)
            {
                Debug.LogWarning("Maximum amount of answers reached");
                return false;
            }

            if (nodeToAdd == this)
                return false;

            if (nodeToAdd is AnswerNode)
                return false;

            return true;
        }
#endif
    }
}