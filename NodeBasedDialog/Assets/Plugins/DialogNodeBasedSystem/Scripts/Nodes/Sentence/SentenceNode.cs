using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_LOCALIZATION
using UnityEngine.Localization.Settings;
#endif

namespace cherrydev
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Node Graph/Nodes/Sentence Node", fileName = "New Sentence Node")]
    public class SentenceNode : Node
    {
        [SerializeField] private Sentence _sentence;

        [Space(10)] 
        public List<Node> ParentNodes = new();
        public Node ChildNode;

        [Space(7)] 
        [SerializeField] private bool _isExternalFunc;
        [SerializeField] private string _externalFunctionName;

        [Space(7)]
        [HideInInspector] public string CharacterNameKey;
        [HideInInspector] public string SentenceTextKey;

        public Sentence Sentence => _sentence;

        private string _externalButtonLabel;

        private const float LabelFieldSpace = 47f;
        private const float TextFieldWidth = 100f;
        private const float ExternalNodeHeight = 155f;

        /// <summary>
        /// Returns character name, using localization if available
        /// </summary>
        /// <returns>Character name (localized if possible)</returns>
        public string GetCharacterName()
        {
            if (NodeGraph == null)
                return _sentence.CharacterName;
            
            string localizedName = TryGetLocalizedString(NodeGraph.CharacterNamesLocalizationName, "Localized name");
            return !string.IsNullOrEmpty(localizedName) ? localizedName : _sentence.CharacterName;
        }

        /// <summary>
        /// Returns sentence text, using localization if available
        /// </summary>
        /// <returns>Sentence text (localized if possible)</returns>
        public string GetText()
        {
            if (NodeGraph == null)
                return _sentence.Text;
            
            string localizedText = TryGetLocalizedString(SentenceTextKey, "Localized string");
            return !string.IsNullOrEmpty(localizedText) ? localizedText : _sentence.Text;
        }
        
        /// <summary>
        /// Try to get localized string for a given key, returns empty if failed
        /// </summary>
        /// <param name="key">Localization key</param>
        /// <param name="errorPrefix">Prefix for error message</param>
        /// <returns>Localized string or empty</returns>
        private string TryGetLocalizedString(string key, string errorPrefix)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

#if UNITY_LOCALIZATION
            try
            {
                string tableName = GetTableNameFromNodeGraph();
                
                if (string.IsNullOrEmpty(tableName))
                    return string.Empty;
                
                string localizedValue = LocalizationSettings.StringDatabase.GetLocalizedString(
                    tableName, key);
        
                if (string.IsNullOrEmpty(localizedValue))
                    Debug.LogWarning($"{errorPrefix} was empty for key: {key}");
            
                return localizedValue;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"{errorPrefix} error: {ex.Message}");
                return string.Empty;
            }
#else
            return string.Empty;
#endif
        }

        /// <summary>
        /// Returning external function name
        /// </summary>
        /// <returns></returns>
        public string GetExternalFunctionName() => _externalFunctionName;

        /// <summary>
        /// Returning sentence character sprite
        /// </summary>
        /// <returns></returns>
        public Sprite GetCharacterSprite() => _sentence.CharacterSprite;

        /// <summary>
        /// Returns the value of a isExternalFunc boolean field
        /// </summary>
        /// <returns></returns>
        public bool IsExternalFunc() => _isExternalFunc;

#if UNITY_EDITOR

        /// <summary>
        /// Draw Sentence Node method
        /// </summary>
        /// <param name="nodeStyle"></param>
        /// <param name="labelStyle"></param>
        public override void Draw(GUIStyle nodeStyle, GUIStyle labelStyle)
        {
            base.Draw(nodeStyle, labelStyle);

            ParentNodes.RemoveAll(item => item == null);
            
            GUILayout.BeginArea(Rect, nodeStyle);

            EditorGUILayout.LabelField("Sentence Node", labelStyle);

            if (DialogNodeGraph.ShowLocalizationKeys)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Localization Keys", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Name Key", GUILayout.Width(LabelFieldSpace));
                CharacterNameKey = EditorGUILayout.TextField(CharacterNameKey, GUILayout.Width(TextFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Text Key", GUILayout.Width(LabelFieldSpace));
                SentenceTextKey = EditorGUILayout.TextField(SentenceTextKey, GUILayout.Width(TextFieldWidth));
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                DrawCharacterNameFieldHorizontal();
                DrawSentenceTextFieldHorizontal();
                DrawCharacterSpriteHorizontal();

                DrawExternalFunctionTextField();

                if (GUILayout.Button(_externalButtonLabel))
                    _isExternalFunc = !_isExternalFunc;
            }

            GUILayout.EndArea();
        }

        /// <summary>
        /// Removes all connections in a sentence node
        /// </summary>
        public override void RemoveAllConnections()
        {
            ParentNodes.Clear();
            ChildNode = null;
        }

        public override bool RemoveFromParentConnectedNode(Node nodeToRemove) => 
            ParentNodes.Remove(nodeToRemove);
        
        /// <summary>
        /// Draw label and text fields for char name
        /// </summary>
        private void DrawCharacterNameFieldHorizontal()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Name ", GUILayout.Width(LabelFieldSpace));
            _sentence.CharacterName =
                EditorGUILayout.TextField(_sentence.CharacterName, GUILayout.Width(TextFieldWidth));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw label and text fields for sentence text
        /// </summary>
        private void DrawSentenceTextFieldHorizontal()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Text ", GUILayout.Width(LabelFieldSpace));
            _sentence.Text = EditorGUILayout.TextField(_sentence.Text, GUILayout.Width(TextFieldWidth));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw label and text fields for char sprite
        /// </summary>
        private void DrawCharacterSpriteHorizontal()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Sprite ", GUILayout.Width(LabelFieldSpace));
            _sentence.CharacterSprite = (Sprite)EditorGUILayout.ObjectField(_sentence.CharacterSprite,
                typeof(Sprite), false, GUILayout.Width(TextFieldWidth));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw label and text fields for external function, 
        /// depends on IsExternalFunc boolean field
        /// </summary>
        private void DrawExternalFunctionTextField()
        {
            if (_isExternalFunc)
            {
                _externalButtonLabel = "Remove external func";

                EditorGUILayout.BeginHorizontal();
                Rect.height = ExternalNodeHeight;
                EditorGUILayout.LabelField($"Func Name ", GUILayout.Width(LabelFieldSpace));
                _externalFunctionName = EditorGUILayout.TextField(_externalFunctionName,
                    GUILayout.Width(TextFieldWidth));
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                _externalButtonLabel = "Add external func";
                Rect.height = StandardHeight;
            }
        }

        /// <summary>
        /// Checking node size
        /// </summary>
        public void CheckNodeSize(float width, float height)
        {
            Rect.width = width;

            if (StandardHeight == 0)
            {
                StandardHeight = height;
            }

            if (_isExternalFunc)
                Rect.height = ExternalNodeHeight;
            else
                Rect.height = StandardHeight;
        }

        /// <summary>
        /// Adding nodeToAdd Node to the childNode field
        /// </summary>
        /// <param name="nodeToAdd"></param>
        /// <returns></returns>
        public override bool AddToChildConnectedNode(Node nodeToAdd)
        {
            if (nodeToAdd == this)
                return false;

            if (nodeToAdd.GetType() == typeof(SentenceNode))
            {
                SentenceNode sentenceNodeToAdd = (SentenceNode)nodeToAdd;
                if (sentenceNodeToAdd != null && sentenceNodeToAdd.ChildNode == this)
                {
                    Debug.LogWarning("Circular parenting not allowed");
                    return false;
                }
            }
    
            if (nodeToAdd.GetType() == typeof(ExternalFunctionNode))
            {
                ExternalFunctionNode externalFunctionNodeToAdd = (ExternalFunctionNode)nodeToAdd;
                
                if (externalFunctionNodeToAdd != null && externalFunctionNodeToAdd.ChildNode == this)
                {
                    Debug.LogWarning("Circular parenting not allowed");
                    return false;
                }
            }

            if (ChildNode != null && ChildNode != nodeToAdd)
                ChildNode.RemoveFromParentConnectedNode(this);

            ChildNode = nodeToAdd;
            return true;
        }

        /// <summary>
        /// Adding nodeToAdd Node to the parentNode field
        /// </summary>
        /// <param name="nodeToAdd"></param>
        /// <returns></returns>
        public override bool AddToParentConnectedNode(Node nodeToAdd)
        {
            
            if (nodeToAdd.GetType() == typeof(AnswerNode))
            {
                if (ParentNodes.Contains(nodeToAdd))
                    return false;
                    
                ParentNodes.Add(nodeToAdd);
                return true;
            }

            if (nodeToAdd.GetType() == typeof(SentenceNode))
            {
                nodeToAdd = (SentenceNode)nodeToAdd;

                if (nodeToAdd == this)
                    return false;

                if (ParentNodes.Contains(nodeToAdd))
                    return false;

                ParentNodes.Add(nodeToAdd);
                return true;
            }

            if (nodeToAdd.GetType() == typeof(ModifyVariableNode) || 
                nodeToAdd.GetType() == typeof(VariableConditionNode) ||
                nodeToAdd.GetType() == typeof(ExternalFunctionNode))
            {
                if (ParentNodes.Contains(nodeToAdd))
                    return false;
                    
                ParentNodes.Add(nodeToAdd);
                return true;
            }

            return false;
        }
#endif
    }
}