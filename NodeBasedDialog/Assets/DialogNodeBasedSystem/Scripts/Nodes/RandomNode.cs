using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace cherrydev
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Nodes/Random Node", fileName = "New RandomNode Node")]
    public class RandomNode : Node
    {
        [SerializeField] private List<Sentence> sentences;

        [Space(10)]
        public List<Node> childNodes;
        public Node parentNode;

        private const float lableFieldSpace = 47f;
        private const float textFieldWidth = 100f;

        private const float buttonsHeight = -60f;

        private int startSentence;

        public RandomNode()
        {
            sentences = new() { new Sentence() { characterName = "Test", text = "Hello" }};
        }

        int rndSentence = -1;
        public Sentence GetRndSentence()
        {
            rndSentence = Random.Range(0, sentences.Count);
            return sentences[rndSentence];
        }

        /// <summary>
        /// Returning sentence character name
        /// </summary>
        /// <returns></returns>
        public string GetSentenceCharacterName()
        {
            if (rndSentence == -1)
            {
                rndSentence = Random.Range(0, sentences.Count);
            }
            return sentences[rndSentence].characterName;
        }

        /// <summary>
        /// Setting sentence text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public void SetSentenceText(string text)
        {
            var s = sentences[0];
            s.text = text;
            sentences[0] = s;
        }

        /// <summary>
        /// Returning sentence text
        /// </summary>
        /// <returns></returns>
        public string GetRndSentenceText()
        {
            if (rndSentence == -1)
            {
                rndSentence = Random.Range(0, sentences.Count);
            }
            return sentences[rndSentence].text;
        }

        /// <summary>
        /// Returning sentence character sprite
        /// </summary>
        /// <returns></returns>
        public Sprite GetCharacterSprite()
        {
            if (rndSentence == -1)
            {
                rndSentence = Random.Range(0, sentences.Count);
            }
            return sentences[rndSentence].characterSprite;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Draw Random Node method
        /// </summary>
        /// <param name="nodeStyle"></param>
        /// <param name="lableStyle"></param>
        public override void Draw(GUIStyle nodeStyle, GUIStyle lableStyle)
        {
            base.Draw(nodeStyle, lableStyle);

            GUILayout.BeginArea(rect, nodeStyle);

            EditorGUILayout.LabelField("Random Node", lableStyle);

            for (int i = 0; i < sentences.Count; i++)
            {
                DrawSentenceLine(i);
            }

            DrawAnswerNodeButtons();
            rect.height = (standardHeight + buttonsHeight) * sentences.Count - buttonsHeight;

            GUILayout.EndArea();
        }

        private void DrawAnswerNodeButtons()
        {
            if (GUILayout.Button("Add sentence"))
            {
                sentences.Add(new());
            }

            if (sentences.Count > 0)
            {
                if (GUILayout.Button("Remove sentence"))
                {
                    sentences.RemoveAt(sentences.Count - 1);
                }
            }
        }

        /// <summary>
        /// Draw label and text fields for char name
        /// </summary>
        private void DrawSentenceLine(int i)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Name ", GUILayout.Width(lableFieldSpace));
            var s = sentences[i];
            s.characterName = EditorGUILayout.TextField(sentences[i].characterName, GUILayout.Width(textFieldWidth));
            sentences[i] = s;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Text ", GUILayout.Width(lableFieldSpace));
            s = sentences[i];
            s.text = EditorGUILayout.TextField(sentences[i].text, GUILayout.Width(textFieldWidth));
            sentences[i] = s;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Sprite ", GUILayout.Width(lableFieldSpace));
            s = sentences[i];
            s.characterSprite = (Sprite)EditorGUILayout.ObjectField(s.characterSprite,
                typeof(Sprite), false, GUILayout.Width(textFieldWidth));
            sentences[i] = s;

            //EditorGUILayout.LabelField(EditorGUIUtility.IconContent(StringConstants.GreenDot),
            //GUILayout.Width(lableFieldSpace));

            var style = new GUIStyle(GUI.skin.button);
            style.normal.textColor = Color.green;

            if (GUILayout.Button("-", style))
            {
                startSentence = i;
                Debug.Log($"startSentence = {startSentence}");
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Checking node size
        /// </summary>
        /// <param name="rect"></param>
        public void CheckNodeSize(float width, float height)
        {
            rect.width = width;
            
            if (standardHeight == 0)
            {
                standardHeight = height;
            }
        }

        /// <summary>
        /// Adding nodeToAdd Node to the childNode field
        /// </summary>
        /// <param name="nodeToAdd"></param>
        /// <returns></returns>
        public override bool AddToChildConnectedNode(Node nodeToAdd)
        {
            if (nodeToAdd is AnswerNode answer)
            {
                Debug.Log($"Linking {startSentence}");
                childNodes ??= new();
                while (childNodes.Count < startSentence + 1)
                {
                    childNodes.Add(null);
                }
                childNodes[startSentence] = answer;
                return true;
            }
            
            return false;
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

            parentNode = nodeToAdd;

            if (nodeToAdd.GetType() == typeof(SentenceNode))
            {
                sentenceNodeToAdd = (SentenceNode)nodeToAdd;

                if (sentenceNodeToAdd.childNode == this)
                {
                    return true;
                }
                else
                {
                    parentNode = null;
                }
            }

            return true;
        }

        public Node GetNextNode()
        {
            return childNodes[rndSentence];
        }

#endif
    }
}