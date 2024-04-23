using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace cherrydev
{
    public class MemberNode : Node
    {
        public List<MemberInfo> members = new List<MemberInfo>();
        public SentenceNode childSentenceNode;
        public int amountOfMembers = 1;


        private float currentMemberNodeHeight = 115f;
        private float additionalMemberNodeHeight = 20f;

        private const float numberLableFieldSpace = 10f;
        private const float nameLableFieldSpace = 40f;

        private const float textFieldWidth = 90f;
        private const float memberNodeWidth = 190f;

        private const int minAmountOfMembers = 1;
        private const int maxAmountOfMembers = 4;

        public int GetAmountOfMembers()
        {
            return amountOfMembers;
        }

#if UNITY_EDITOR
        public override void Initialise(Rect rect, string nodeName, DialogNodeGraph nodeGraph)
        {
            base.Initialise(rect, nodeName, nodeGraph);

            members.Add(new MemberInfo(amountOfMembers));
        }

        public override void Draw(GUIStyle nodeStyle, GUIStyle lableStyle)
        {
            base.Draw(nodeStyle, lableStyle);

            rect.size = new Vector2(memberNodeWidth, currentMemberNodeHeight);

            GUILayout.BeginArea(rect, nodeStyle);

            EditorGUILayout.LabelField("Member Node", lableStyle);

            HandleMebmerFieldsDrawing();

            if (GUILayout.Button("Add member"))
            {
                if (members.Count >= maxAmountOfMembers)
                {
                    return;
                }

                AddMember();
            }

            if (GUILayout.Button("Remove member"))
            {
                if (members.Count <= minAmountOfMembers)
                {
                    return;
                }

                RemoveMember();
            }

            GUILayout.EndArea();
        }

        public void CheckNodeSize()
        {
            for (int i = 0; i < members.Count - 1; i++)
            {
                currentMemberNodeHeight += additionalMemberNodeHeight;
            }
        }

        public void HandleMebmerFieldsDrawing()
        {
            for (int i = 0; i < amountOfMembers; i++)
            {
                DrawMemverField(i);
            }
        }

        public override bool AddToChildConnectedNode(Node nodeToAdd)
        {
            if (childSentenceNode == null)
            {
                if (nodeToAdd.GetType() == typeof(SentenceNode))
                {
                    childSentenceNode = (SentenceNode)nodeToAdd;

                    return true;
                }
            }

            return false;
        }

        public void AddChildSentenceNode(SentenceNode childSentenceNode)
        {
            this.childSentenceNode = childSentenceNode;
        }

        private void DrawMemverField(int number)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"{number + 1}. ",
                GUILayout.Width(numberLableFieldSpace));
            EditorGUILayout.LabelField($"Name ",
                GUILayout.Width(nameLableFieldSpace));

            members[number].memberName = EditorGUILayout.TextField(members[number].memberName,
                GUILayout.Width(textFieldWidth));

            EditorGUILayout.EndHorizontal();
        }

        private void AddMember()
        {
            amountOfMembers++;
            members.Add(new MemberInfo(amountOfMembers));

            currentMemberNodeHeight += additionalMemberNodeHeight;
        }

        private void RemoveMember()
        {
            amountOfMembers--;
            members.RemoveAt(members.Count - 1);

            currentMemberNodeHeight -= additionalMemberNodeHeight;
        }
#endif
    }

    [System.Serializable]
    public class MemberInfo
    {
        public string memberName;

        public int Number { get; set; }

        public MemberInfo(int number) 
        { 
            Number = number;
        }
    }
}