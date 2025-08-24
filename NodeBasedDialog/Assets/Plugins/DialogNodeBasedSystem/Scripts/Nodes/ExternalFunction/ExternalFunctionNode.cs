using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace cherrydev
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Node Graph/Nodes/External Function Node", fileName = "New External Function Node")]
    public class ExternalFunctionNode : Node
    {
        [SerializeField] private string _functionName;
        [SerializeField] private string _description;

        [Space(10)] 
        public List<Node> ParentNodes = new();
        public Node ChildNode;

        public string FunctionName => _functionName;
        public string Description => _description;

        private const float NodeWidth = 190f;
        private const float NodeHeight = 80f;
        private const float LabelFieldSpace = 55f;
        private const float TextFieldWidth = 100f;

        /// <summary>
        /// Returns the external function name
        /// </summary>
        /// <returns>External function name</returns>
        public string GetExternalFunctionName() => _functionName;

#if UNITY_EDITOR

        /// <summary>
        /// Draw External Function Node method
        /// </summary>
        /// <param name="nodeStyle"></param>
        /// <param name="labelStyle"></param>
        public override void Draw(GUIStyle nodeStyle, GUIStyle labelStyle)
        {
            base.Draw(nodeStyle, labelStyle);

            Rect.size = new Vector2(NodeWidth, NodeHeight);
            ParentNodes.RemoveAll(item => item == null);
            
            GUILayout.BeginArea(Rect, nodeStyle);
            EditorGUILayout.LabelField("External Function", labelStyle);
            DrawFunctionNameField();
            GUILayout.EndArea();
        }

        /// <summary>
        /// Removes all connections in the external function node
        /// </summary>
        public override void RemoveAllConnections()
        {
            foreach (Node parent in ParentNodes.ToList())
                parent.RemoveChildConnection(this);
            
            ParentNodes.Clear();
    
            if (ChildNode != null)
            {
                ChildNode.RemoveFromParentConnectedNode(this);
                ChildNode = null;
            }
        }

        public override bool RemoveFromParentConnectedNode(Node nodeToRemove) => 
            ParentNodes.Remove(nodeToRemove);

        /// <summary>
        /// Draw label and text field for function name
        /// </summary>
        private void DrawFunctionNameField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Function", GUILayout.Width(LabelFieldSpace));
            _functionName = EditorGUILayout.TextField(_functionName, GUILayout.Width(TextFieldWidth));
            EditorGUILayout.EndHorizontal();
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

            if (nodeToAdd.GetType() == typeof(ExternalFunctionNode))
            {
                ExternalFunctionNode externalFunctionNodeToAdd = (ExternalFunctionNode)nodeToAdd;
                
                if (externalFunctionNodeToAdd.ChildNode == this)
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
                if (nodeToAdd == this)
                    return false;

                if (ParentNodes.Contains(nodeToAdd))
                    return false;
                    
                ParentNodes.Add(nodeToAdd);
                return true;
            }

            if (nodeToAdd.GetType() == typeof(ModifyVariableNode) || 
                nodeToAdd.GetType() == typeof(VariableConditionNode))
            {
                if (ParentNodes.Contains(nodeToAdd))
                    return false;
                    
                ParentNodes.Add(nodeToAdd);
                return true;
            }

            if (nodeToAdd.GetType() == typeof(ExternalFunctionNode))
            {
                if (nodeToAdd == this)
                    return false;

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