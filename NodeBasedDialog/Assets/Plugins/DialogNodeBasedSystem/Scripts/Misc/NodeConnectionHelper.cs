#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace cherrydev
{
    /// <summary>
    /// Helper class to manage node connections centrally - Updated for multiple parents support and ExternalFunctionNode
    /// </summary>
    public static class NodeConnectionHelper
    {
        /// <summary>
        /// Creates a connection between parent and child, handling multiple parents
        /// </summary>
        public static bool CreateConnection(Node parent, Node child)
        {
            if (parent == null || child == null || parent == child)
                return false;

            if (!IsValidConnection(parent, child))
                return false;

            if (ConnectionExists(parent, child))
                return false;

            if (WouldCreateCycle(parent, child))
                return false;

            PrepareForNewConnection(parent, child);

            bool parentAccepted = parent.AddToChildConnectedNode(child);
            
            if (parentAccepted)
            {
                bool childAccepted = child.AddToParentConnectedNode(parent);
                
                if (!childAccepted)
                {
                    RemoveConnection(parent, child);
                    return false;
                }
        
                return true;
            }
    
            return false;
        }

        /// <summary>
        /// Checks if a connection between two nodes is valid
        /// </summary>
        private static bool IsValidConnection(Node parent, Node child)
        {
            if (parent is AnswerNode && child is AnswerNode)
                return false;

            return true;
        }

        /// <summary>
        /// Checks if a connection already exists between parent and child
        /// </summary>
        private static bool ConnectionExists(Node parent, Node child)
        {
            List<Node> childNodes = GetChildNodes(parent);
            return childNodes.Contains(child);
        }

        /// <summary>
        /// Checks if creating this connection would create a cycle
        /// </summary>
        private static bool WouldCreateCycle(Node parent, Node child) => CanReachNode(child, parent);

        /// <summary>
        /// Recursively checks if we can reach targetNode from sourceNode
        /// </summary>
        private static bool CanReachNode(Node sourceNode, Node targetNode)
        {
            if (sourceNode == null || targetNode == null)
                return false;

            if (sourceNode == targetNode)
                return true;

            List<Node> children = GetChildNodes(sourceNode);
            
            foreach (Node child in children)
            {
                if (CanReachNode(child, targetNode))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Prepares nodes for a new connection by removing conflicting connections for single-child nodes
        /// </summary>
        private static void PrepareForNewConnection(Node parent, Node child)
        {
            switch (parent)
            {
                case SentenceNode sentenceParent:
                    if (sentenceParent.ChildNode != null && sentenceParent.ChildNode != child)
                        RemoveConnection(sentenceParent, sentenceParent.ChildNode);
                    break;

                case ExternalFunctionNode externalFunctionParent:
                    if (externalFunctionParent.ChildNode != null && externalFunctionParent.ChildNode != child)
                        RemoveConnection(externalFunctionParent, externalFunctionParent.ChildNode);
                    break;

                case ModifyVariableNode modifyParent:
                    if (modifyParent.ChildNode != null && modifyParent.ChildNode != child)
                        RemoveConnection(modifyParent, modifyParent.ChildNode);
                    break;

                case VariableConditionNode conditionParent:
                    break;

                case AnswerNode answerParent:
                    break;
            }
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

                case ExternalFunctionNode externalFunctionParent:
                    if (externalFunctionParent.ChildNode == child)
                        externalFunctionParent.ChildNode = null;
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
                    sentenceChild.RemoveFromParentConnectedNode(parent);
                    break;

                case ExternalFunctionNode externalFunctionChild:
                    externalFunctionChild.RemoveFromParentConnectedNode(parent);
                    break;

                case AnswerNode answerChild:
                    answerChild.RemoveFromParentConnectedNode(parent);
                    break;

                case ModifyVariableNode modifyChild:
                    modifyChild.RemoveFromParentConnectedNode(parent);
                    break;

                case VariableConditionNode conditionChild:
                    conditionChild.RemoveFromParentConnectedNode(parent);
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

            RemoveFromAllParents(nodeToDisconnect);
            RemoveFromAllChildren(nodeToDisconnect);
            nodeToDisconnect.RemoveAllConnections();
        }

        /// <summary>
        /// Removes a node from all its parents' child references
        /// </summary>
        private static void RemoveFromAllParents(Node node)
        {
            List<Node> parentNodes = GetParentNodes(node);
            
            foreach (Node parentNode in parentNodes.ToArray())
            {
                switch (parentNode)
                {
                    case SentenceNode sentenceNode:
                        if (sentenceNode.ChildNode == node)
                            sentenceNode.ChildNode = null;
                        break;

                    case ExternalFunctionNode externalFunctionNode:
                        if (externalFunctionNode.ChildNode == node)
                            externalFunctionNode.ChildNode = null;
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
        }

        /// <summary>
        /// Removes a node from all its children's parent references
        /// </summary>
        private static void RemoveFromAllChildren(Node node)
        {
            List<Node> childNodes = GetChildNodes(node);
            
            foreach (Node childNode in childNodes)
            {
                switch (childNode)
                {
                    case SentenceNode sentenceNode:
                        sentenceNode.RemoveFromParentConnectedNode(node);
                        break;

                    case ExternalFunctionNode externalFunctionNode:
                        externalFunctionNode.RemoveFromParentConnectedNode(node);
                        break;
                        
                    case AnswerNode answerNode:
                        answerNode.RemoveFromParentConnectedNode(node);
                        break;
                        
                    case ModifyVariableNode modifyVariableNode:
                        modifyVariableNode.RemoveFromParentConnectedNode(node);
                        break;
                        
                    case VariableConditionNode variableConditionNode:
                        variableConditionNode.RemoveFromParentConnectedNode(node);
                        break;
                }
            }
        }

        /// <summary>
        /// Gets all parent nodes of a given node - Updated for multiple parents and ExternalFunctionNode
        /// </summary>
        private static List<Node> GetParentNodes(Node node)
        {
            return node switch
            {
                SentenceNode sentenceNode => new List<Node>(sentenceNode.ParentNodes),
                ExternalFunctionNode externalFunctionNode => new List<Node>(externalFunctionNode.ParentNodes),
                AnswerNode answerNode => new List<Node>(answerNode.ParentNodes),
                ModifyVariableNode modifyVariableNode => new List<Node>(modifyVariableNode.ParentNodes),
                VariableConditionNode variableConditionNode => new List<Node>(variableConditionNode.ParentNodes),
                _ => new List<Node>()
            };
        }

        /// <summary>
        /// Gets all child nodes of a given node - Updated to include ExternalFunctionNode
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

                case ExternalFunctionNode externalFunctionNode:
                    if (externalFunctionNode.ChildNode != null)
                        children.Add(externalFunctionNode.ChildNode);
                    break;
                    
                case AnswerNode answerNode:
                    children.AddRange(answerNode.ChildNodes.Where(child => child != null));
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