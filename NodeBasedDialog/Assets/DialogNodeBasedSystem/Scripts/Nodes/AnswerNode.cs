using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace cherrydev
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Nodes/Answer Node", fileName = "New Answer Node")]
    public class AnswerNode : Node
    {
        private int amountOfAnswers = 1;

        public List<string> answers = new List<string>();

        public SentenceNode parentSentenceNode;
        public List<SentenceNode> childSentenceNodes = new List<SentenceNode>();

        private const float lableFieldSpace = 18f;
        private const float textFieldWidth = 120f;

        private const float answerNodeWidth = 190f;
        private const float answerNodeHeight = 115f;

        private float currentAnswerNodeHeight = 115f;
        private float additionalAnswerNodeHeight = 20f;

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

            CalculateAmountOfAnswers();

            childSentenceNodes = new List<SentenceNode>(amountOfAnswers);
        }

        /// <summary>
        /// Draw Answer Node method
        /// </summary>
        /// <param name = "nodeStyle" ></ param >
        /// < param name="lableStyle"></param>
        public override void Draw(GUIStyle nodeStyle, GUIStyle lableStyle)
        {
            base.Draw(nodeStyle, lableStyle);

            childSentenceNodes.RemoveAll(item => item == null);

            rect.size = new Vector2(answerNodeWidth, currentAnswerNodeHeight);

            GUILayout.BeginArea(rect, nodeStyle);
            EditorGUILayout.LabelField("Answer Node", lableStyle);

            for (int i = 0; i < amountOfAnswers; i++)
            {
                DrawAnswerLine(i + 1, StringConstants.GreenDot);
            }

            DrawAnswerNodeButtons();

            GUILayout.EndArea();
        }

        /// <summary>
        /// Determines the number of answers depending on answers list count
        /// </summary>
        public void CalculateAmountOfAnswers()
        {
            if (answers.Count == 0)
            {
                amountOfAnswers = 1;

                answers = new List<string>() { string.Empty };
            }
            else
            {
                amountOfAnswers = answers.Count;
            }
        }

        /// <summary>
        /// Draw answer line
        /// </summary>
        /// <param name="answerNumber"></param>
        /// <param name="iconPathOrName"></param>
        private void DrawAnswerLine(int answerNumber, string iconPathOrName)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"{answerNumber}. ", 
                GUILayout.Width(lableFieldSpace));

            answers[answerNumber - 1] = EditorGUILayout.TextField(answers[answerNumber - 1], 
                GUILayout.Width(textFieldWidth));

            EditorGUILayout.LabelField(EditorGUIUtility.IconContent(iconPathOrName), 
                GUILayout.Width(lableFieldSpace));

            EditorGUILayout.EndHorizontal();
        }

        private void DrawAnswerNodeButtons()
        {
            if (GUILayout.Button("Add answer"))
            {
                IncreaseAmountOfAnswers();
            }

            if (GUILayout.Button("Remove answer"))
            {
                DecreaseAmountOfAnswers();
            }
        }

        /// <summary>
        /// Increase amount of answers and node height
        /// </summary>
        private void IncreaseAmountOfAnswers()
        {
            amountOfAnswers++;

            answers.Add(string.Empty);

            currentAnswerNodeHeight += additionalAnswerNodeHeight;
        }

        /// <summary>
        /// Decrease amount of answers and node height 
        /// </summary>
        private void DecreaseAmountOfAnswers()
        {
            if (answers.Count == 1)
            {
                return;
            }

            answers.RemoveAt(amountOfAnswers - 1);

            if (childSentenceNodes.Count == amountOfAnswers)
            {
                childSentenceNodes[amountOfAnswers - 1].parentNode = null;
                childSentenceNodes.RemoveAt(amountOfAnswers - 1);
            }

            amountOfAnswers--;

            currentAnswerNodeHeight -= additionalAnswerNodeHeight;
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

            if (IsCanAddToChildConnectedNode(sentenceNodeToAdd))
            {
                childSentenceNodes.Add(sentenceNodeToAdd);

                sentenceNodeToAdd.parentNode = this;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculate answer node height based on amount of answers
        /// </summary>
        public void CalculateAnswerNodeHeight()
        {
            currentAnswerNodeHeight = answerNodeHeight;

            for (int i = 0; i < amountOfAnswers - 1; i++)
            {
                currentAnswerNodeHeight += additionalAnswerNodeHeight;
            }
        }

        private bool IsCanAddToChildConnectedNode(SentenceNode sentenceNodeToAdd)
        {
            return sentenceNodeToAdd.parentNode == null 
                && childSentenceNodes.Count < amountOfAnswers 
                && sentenceNodeToAdd.childNode != this;
        }

#endif
    }
}