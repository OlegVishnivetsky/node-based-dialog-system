#if UNITY_EDITOR
using System.Collections.Generic;

namespace cherrydev
{
    /// <summary>
    /// Helper class to manage node connections centrally
    /// </summary>
    public static class NodeConnectionHelper
    {
        /// <summary>
        /// Creates a connection between parent and child, removing any conflicting connections
        /// </summary>
        public static bool CreateConnection(Node parent, Node child)
        {
            if (parent == null || child == null || parent == child)
                return false;

            // First, check if this connection is valid
            if (!IsValidConnection(parent, child))
                return false;

            // Remove any existing connections that would conflict
            PrepareForNewConnection(parent, child);

            // Create the new connection
            bool parentAccepted = parent.AddToChildConnectedNode(child);
            bool childAccepted = child.AddToParentConnectedNode(parent);

            return parentAccepted && childAccepted;
        }

        /// <summary>
        /// Checks if a connection between two nodes is valid
        /// </summary>
        private static bool IsValidConnection(Node parent, Node child)
        {
            // Answer nodes cannot be parents of Answer nodes
            if (parent is AnswerNode && child is AnswerNode)
                return false;

            // Add more validation rules as needed
            return true;
        }

        /// <summary>
        /// Prepares nodes for a new connection by removing conflicting connections
        /// </summary>
        private static void PrepareForNewConnection(Node parent, Node child)
        {
            switch (parent)
            {
                case SentenceNode sentenceParent:
                    if (sentenceParent.ChildNode != null && sentenceParent.ChildNode != child)
                        RemoveConnection(sentenceParent, sentenceParent.ChildNode);
                    break;

                case ModifyVariableNode modifyParent:
                    if (modifyParent.ChildNode != null && modifyParent.ChildNode != child)
                        RemoveConnection(modifyParent, modifyParent.ChildNode);
                    break;

                case VariableConditionNode conditionParent:
                    break;

                case AnswerNode answerParent:
                    if (answerParent.ChildNodes.Contains(child))
                        answerParent.ChildNodes.Remove(child);
                    break;
            }

            Node existingParent = GetParentNode(child);
            
            if (existingParent != null && existingParent != parent)
                RemoveConnection(existingParent, child);
        }

        /// <summary>
        /// Removes a specific connection between two nodes
        /// </summary>
        public static void RemoveConnection(Node parent, Node child)
        {
            if (parent == null || child == null)
                return;

            switch (parent)
            {
                case SentenceNode sentenceParent:
                    if (sentenceParent.ChildNode == child)
                        sentenceParent.ChildNode = null;
                    break;

                case AnswerNode answerParent:
                    answerParent.ChildNodes.Remove(child);
                    break;

                case ModifyVariableNode modifyParent:
                    if (modifyParent.ChildNode == child)
                        modifyParent.ChildNode = null;
                    break;

                case VariableConditionNode conditionParent:
                    if (conditionParent.TrueChildNode == child)
                        conditionParent.TrueChildNode = null;
                    if (conditionParent.FalseChildNode == child)
                        conditionParent.FalseChildNode = null;
                    break;
            }

            switch (child)
            {
                case SentenceNode sentenceChild:
                    if (sentenceChild.ParentNode == parent)
                        sentenceChild.ParentNode = null;
                    break;

                case AnswerNode answerChild:
                    if (answerChild.ParentNode == parent)
                        answerChild.ParentNode = null;
                    break;

                case ModifyVariableNode modifyChild:
                    if (modifyChild.ParentNode == parent)
                        modifyChild.ParentNode = null;
                    break;

                case VariableConditionNode conditionChild:
                    if (conditionChild.ParentNode == parent)
                        conditionChild.ParentNode = null;
                    break;
            }
        }
        /// <summary>
        /// Removes all connections for a node, including references from other nodes
        /// </summary>
        public static void RemoveAllConnectionsForNode(Node nodeToDisconnect)
        {
            if (nodeToDisconnect == null)
                return;

            RemoveFromParent(nodeToDisconnect);
            RemoveFromChildren(nodeToDisconnect);
            nodeToDisconnect.RemoveAllConnections();
        }

        /// <summary>
        /// Removes a node from its parent's child references
        /// </summary>
        private static void RemoveFromParent(Node node)
        {
            Node parentNode = GetParentNode(node);
            
            if (parentNode == null)
                return;

            switch (parentNode)
            {
                case SentenceNode sentenceNode:
                    if (sentenceNode.ChildNode == node)
                        sentenceNode.ChildNode = null;
                    break;
                    
                case AnswerNode answerNode:
                    answerNode.ChildNodes.Remove(node);
                    break;
                    
                case ModifyVariableNode modifyVariableNode:
                    if (modifyVariableNode.ChildNode == node)
                        modifyVariableNode.ChildNode = null;
                    break;
                    
                case VariableConditionNode variableConditionNode:
                    if (variableConditionNode.TrueChildNode == node)
                        variableConditionNode.TrueChildNode = null;
                    if (variableConditionNode.FalseChildNode == node)
                        variableConditionNode.FalseChildNode = null;
                    break;
            }
        }

        /// <summary>
        /// Removes a node from its children's parent references
        /// </summary>
        private static void RemoveFromChildren(Node node)
        {
            List<Node> childNodes = GetChildNodes(node);
            
            foreach (Node childNode in childNodes)
            {
                switch (childNode)
                {
                    case SentenceNode sentenceNode:
                        if (sentenceNode.ParentNode == node)
                            sentenceNode.ParentNode = null;
                        break;
                        
                    case AnswerNode answerNode:
                        if (answerNode.ParentNode == node)
                            answerNode.ParentNode = null;
                        break;
                        
                    case ModifyVariableNode modifyVariableNode:
                        if (modifyVariableNode.ParentNode == node)
                            modifyVariableNode.ParentNode = null;
                        break;
                        
                    case VariableConditionNode variableConditionNode:
                        if (variableConditionNode.ParentNode == node)
                            variableConditionNode.ParentNode = null;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the parent node of a given node
        /// </summary>
        private static Node GetParentNode(Node node)
        {
            return node switch
            {
                SentenceNode sentenceNode => sentenceNode.ParentNode,
                AnswerNode answerNode => answerNode.ParentNode,
                ModifyVariableNode modifyVariableNode => modifyVariableNode.ParentNode,
                VariableConditionNode variableConditionNode => variableConditionNode.ParentNode,
                _ => null
            };
        }

        /// <summary>
        /// Gets all child nodes of a given node
        /// </summary>
        private static List<Node> GetChildNodes(Node node)
        {
            List<Node> children = new List<Node>();
            
            switch (node)
            {
                case SentenceNode sentenceNode:
                    if (sentenceNode.ChildNode != null)
                        children.Add(sentenceNode.ChildNode);
                    break;
                    
                case AnswerNode answerNode:
                    children.AddRange(answerNode.ChildNodes);
                    break;
                    
                case ModifyVariableNode modifyVariableNode:
                    if (modifyVariableNode.ChildNode != null)
                        children.Add(modifyVariableNode.ChildNode);
                    break;
                    
                case VariableConditionNode variableConditionNode:
                    if (variableConditionNode.TrueChildNode != null)
                        children.Add(variableConditionNode.TrueChildNode);
                    if (variableConditionNode.FalseChildNode != null)
                        children.Add(variableConditionNode.FalseChildNode);
                    break;
            }
            
            return children;
        }
    }
}
#endif