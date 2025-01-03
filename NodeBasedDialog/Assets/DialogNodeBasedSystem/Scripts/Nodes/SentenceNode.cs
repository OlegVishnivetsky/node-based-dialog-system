using UnityEditor;
using UnityEngine;

namespace cherrydev
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Nodes/Sentence Node", fileName = "New Sentence Node")]
    public class SentenceNode : Node
    {
        [SerializeField] private Sentence _sentence;

        [Space(10)]
        public Node ParentNode;
        public Node ChildNode;

        [Space(7)]
        [SerializeField] private bool _isExternalFunc;
        [SerializeField] private string _externalFunctionName;

        private string _externalButtonLabel;

        private const float LabelFieldSpace = 47f;
        private const float TextFieldWidth = 100f;
        
        private const float ExternalNodeHeight = 155f;

        /// <summary>
        /// Returning external function name
        /// </summary>
        /// <returns></returns>
        public string GetExternalFunctionName() => _externalFunctionName;

        /// <summary>
        /// Returning sentence character name
        /// </summary>
        /// <returns></returns>
        public string GetSentenceCharacterName() => _sentence.CharacterName;

        /// <summary>
        /// Setting sentence text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public void SetSentenceText(string text) => _sentence.Text = text;

        /// <summary>
        /// Returning sentence text
        /// </summary>
        /// <returns></returns>
        public string GetSentenceText() => _sentence.Text;

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
        /// <param name="lableStyle"></param>
        public override void Draw(GUIStyle nodeStyle, GUIStyle lableStyle)
        {
            base.Draw(nodeStyle, lableStyle);

            GUILayout.BeginArea(Rect, nodeStyle);

            EditorGUILayout.LabelField("Sentence Node", lableStyle);

            DrawCharacterNameFieldHorizontal();
            DrawSentenceTextFieldHorizontal();
            DrawCharacterSpriteHorizontal();
            DrawExternalFunctionTextField();

            if (GUILayout.Button(_externalButtonLabel))
            {
                _isExternalFunc = !_isExternalFunc;

            }

            GUILayout.EndArea();
        }

        /// <summary>
        /// Draw label and text fields for char name
        /// </summary>
        private void DrawCharacterNameFieldHorizontal()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Name ", GUILayout.Width(LabelFieldSpace));
            _sentence.CharacterName = EditorGUILayout.TextField(_sentence.CharacterName, GUILayout.Width(TextFieldWidth));
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
            SentenceNode sentenceNodeToAdd;

            if (nodeToAdd.GetType() == typeof(SentenceNode))
            {
                nodeToAdd = (SentenceNode)nodeToAdd;

                if (nodeToAdd == this)
                    return false;
            }

            if (nodeToAdd.GetType() == typeof(SentenceNode))
            {
                sentenceNodeToAdd = (SentenceNode)nodeToAdd;

                if (sentenceNodeToAdd != null && sentenceNodeToAdd.ChildNode == this)
                    return false;
            }

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
            SentenceNode sentenceNodeToAdd;

            if (nodeToAdd.GetType() == typeof(AnswerNode))
            {
                return false;
            }

            if (nodeToAdd.GetType() == typeof(SentenceNode))
            {
                nodeToAdd = (SentenceNode)nodeToAdd;

                if (nodeToAdd == this)
                {
                    return false;
                }
            }

            ParentNode = nodeToAdd;

            if (nodeToAdd.GetType() == typeof(SentenceNode))
            {
                sentenceNodeToAdd = (SentenceNode)nodeToAdd;

                if (sentenceNodeToAdd.ChildNode == this)
                {
                    return true;
                }
                else
                {
                    ParentNode = null;
                }
            }

            return true;
        }

#endif
    }
}