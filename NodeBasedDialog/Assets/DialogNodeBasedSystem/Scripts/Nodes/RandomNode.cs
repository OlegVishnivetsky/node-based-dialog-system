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

        private const float labelFieldSpace = 65f;
        private const float textFieldWidth = 100f;

        private const float NODE_WIDTH = 240f;
        private const float buttonsHeight = -60f;

        private int startSentence;

        public RandomNode()
        {
            sentences = new() { new Sentence() { characterName = "Test", text = "Hello" }};
        }

        int rndSentence = -1;
        public Sentence GetRndSentence()
        {
            int total = 0;
            foreach (Sentence sentence in sentences)
            {
                total += sentence.probability;
            }

            if (total == 0)
            {
                rndSentence = Random.Range(0, sentences.Count);
                return sentences[rndSentence];
            }

            int rnd = Random.Range(0, total);
            for (int i = 0; i < sentences.Count; i++)
            {
                if (rnd < sentences[i].probability)
                {
                    rndSentence = i;
                    return sentences[rndSentence];
                }
                rnd -= sentences[i].probability;
            }

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
        private const float SENTENCE_HEIGHT = 89.5f;
        private const int BUTTON_HEIGHT = 90;
        public override void Draw(GUIStyle nodeStyle, GUIStyle labelStyle)
        {
            base.Draw(nodeStyle, labelStyle);

            GUILayout.BeginArea(rect, nodeStyle);

            EditorGUILayout.LabelField("Random Node", labelStyle);

            for (int i = 0; i < sentences.Count; i++)
            {
                DrawSentenceLine(i);
            }

            DrawAnswerNodeButtons();
            rect.height = SENTENCE_HEIGHT * sentences.Count + BUTTON_HEIGHT;

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
            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{i + 1}. Name ", GUILayout.Width(labelFieldSpace));
            var s = sentences[i];
            s.characterName = EditorGUILayout.TextField(sentences[i].characterName, GUILayout.Width(textFieldWidth));
            sentences[i] = s;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Text ", GUILayout.Width(labelFieldSpace));
            s = sentences[i];
            s.text = EditorGUILayout.TextField(sentences[i].text, GUILayout.Width(textFieldWidth));
            sentences[i] = s;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Sprite ", GUILayout.Width(labelFieldSpace));
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
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Probability ", GUILayout.Width(labelFieldSpace));
            s = sentences[i];
            s.probability = EditorGUILayout.IntField(sentences[i].probability, GUILayout.Width(textFieldWidth));
            sentences[i] = s;
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
                childNodes ??= new();
                while (childNodes.Count < startSentence + 1)
                {
                    childNodes.Add(null);
                }
                childNodes[startSentence] = nodeToAdd;
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

        public void Redraw()
        {
            rect.height = SENTENCE_HEIGHT * sentences.Count + BUTTON_HEIGHT;
            rect.width = NODE_WIDTH;
        }

#endif
    }
}