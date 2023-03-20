using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace cherrydev
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Nodes/Answer Node", fileName = "New Answer Node")]
    public class AnswerNode : Node
    {
        private const int amountOfAnswers = 4;

        public List<string> answers = new List<string>();

        public SentenceNode parentSentenceNode;
        public SentenceNode[] childSentenceNodes;

        private const float lableFieldSpace = 15f;
        private const float textFieldWidth = 120f;

        private const float answerNodeWidth = 190f;
        private const float answerNodeHeight = 145f;

#if UNITY_EDITOR

        /// <summary>
        /// Answer node initialisation method
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="nodeName"></param>
        /// <param name="nodeGraph"></param>
        public override void Initialise(Rect rect, string nodeName, DialogNodeGraph nodeGraph)
        {
            base.Initialise(rect, nodeName, nodeGraph);

            childSentenceNodes = new SentenceNode[amountOfAnswers];

            for (int i = 0; i < amountOfAnswers; i++)
            {
                answers.Add(string.Empty);
            }
        }

        /// <summary>
        /// Draw Answer Node method
        /// </summary>
        /// <param name = "nodeStyle" ></ param >
        /// < param name="lableStyle"></param>
        public override void Draw(GUIStyle nodeStyle, GUIStyle lableStyle)
        {
            base.Draw(nodeStyle, lableStyle);

            rect.size = new Vector2(answerNodeWidth, answerNodeHeight);

            GUILayout.BeginArea(rect, nodeStyle);
            EditorGUILayout.LabelField("Answer Node", lableStyle);

            DrawAnswerLine(1, EditorIcons.GreenDot);
            DrawAnswerLine(2, EditorIcons.GreenDot);
            DrawAnswerLine(3, EditorIcons.GreenDot);
            DrawAnswerLine(4, EditorIcons.GreenDot);

            GUILayout.EndArea();
        }

        private void DrawAnswerLine(int answerNumber, string iconPathOrName)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{answerNumber}. ", GUILayout.Width(lableFieldSpace));
            answers[answerNumber - 1] = EditorGUILayout.TextField(answers[answerNumber - 1], GUILayout.Width(textFieldWidth));
            EditorGUILayout.LabelField(EditorGUIUtility.IconContent(iconPathOrName), GUILayout.Width(lableFieldSpace));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Adding nodeToAdd Node to the parentSentenceNode field
        /// </summary>
        /// <param name="nodeToAdd"></param>
        /// <returns></returns>
        public override bool AddToParentConnectedNode(Node nodeToAdd)
        {
            if (nodeToAdd.GetType() == typeof(SentenceNode))
            {
                parentSentenceNode = (SentenceNode)nodeToAdd;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Adding nodeToAdd Node to the childSentenceNodes array
        /// </summary>
        /// <param name="nodeToAdd"></param>
        /// <returns></returns>
        public override bool AddToChildConnectedNode(Node nodeToAdd)
        {
            SentenceNode sentenceNodeToAdd;

            if (nodeToAdd.GetType() != typeof(AnswerNode))
            {
                sentenceNodeToAdd = (SentenceNode)nodeToAdd;
            }
            else
            {
                return false;
            }

            for (int i = 0; i < amountOfAnswers; i++)
            {
                if (childSentenceNodes[i] == null && sentenceNodeToAdd.parentNode == null)
                {
                    childSentenceNodes[i] = (SentenceNode)nodeToAdd;

                    return true;
                }
            }

            return false;
        }

#endif
    }
}