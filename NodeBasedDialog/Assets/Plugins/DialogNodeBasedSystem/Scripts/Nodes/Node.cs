using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace cherrydev
{
    public class Node : ScriptableObject
    {
        [HideInInspector] public DialogNodeGraph NodeGraph;
        [HideInInspector] public Rect Rect;

        [HideInInspector] public bool IsDragging;
        [HideInInspector] public bool IsSelected;

        protected float StandardHeight;

        /// <summary>
        /// Gets the table name from the node graph asset name
        /// </summary>
        /// <returns>The table name for this node's graph</returns>
        protected string GetTableNameFromNodeGraph()
        {
            if (NodeGraph == null)
                return string.Empty;
            
            return NodeGraph.LocalizationTableName;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Base initialisation method
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="nodeName"></param>
        /// <param name="nodeGraph"></param>
        public virtual void Initialize(Rect rect, string nodeName, DialogNodeGraph nodeGraph)
        {
            name = nodeName;
            StandardHeight = rect.height;
            Rect = rect; 
            NodeGraph = nodeGraph;
        }

        /// <summary>
        /// Base draw method
        /// </summary>
        /// <param name="nodeStyle"></param>
        /// <param name="labelStyle"></param>
        public virtual void Draw(GUIStyle nodeStyle, GUIStyle labelStyle) { }

        public virtual bool AddToParentConnectedNode(Node nodeToAdd) => true;

        public virtual bool AddToChildConnectedNode(Node nodeToAdd) => true;

        /// <summary>
        /// Process node events
        /// </summary>
        /// <param name="currentEvent"></param>
        public void ProcessNodeEvents(Event currentEvent)
        {
            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    ProcessMouseDownEvent(currentEvent);
                    break;
                case EventType.MouseUp:
                    ProcessMouseUpEvent(currentEvent);
                    break;
                case EventType.MouseDrag:
                    ProcessMouseDragEvent(currentEvent);
                    break;
            }
        }

        /// <summary>
        /// Called by the context menu "RemoveAllConnections",
        /// must be implemented to properly remove connections.
        /// </summary>
		public virtual void RemoveAllConnections() {}

        /// <summary>
        /// Process node mouse down event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessMouseDownEvent(Event currentEvent)
        {
            if (currentEvent.button == 0)
                ProcessLeftMouseDownEvent(currentEvent);
            else if (currentEvent.button == 1)
                ProcessRightMouseDownEvent(currentEvent);
        }

        /// <summary>
        /// Process node left click event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessLeftMouseDownEvent(Event currentEvent) => OnNodeLeftClick();

        /// <summary>
        /// Process node right click down event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessRightMouseDownEvent(Event currentEvent) => 
            NodeGraph.SetNodeToDrawLineFromAndLinePosition(this, currentEvent.mousePosition);

        /// <summary>
        /// Process node mouse up event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessMouseUpEvent(Event currentEvent)
        {
            if (currentEvent.button == 0)
                ProcessLeftMouseUpEvent(currentEvent);
        }

        /// <summary>
        /// Process node left click up event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessLeftMouseUpEvent(Event currentEvent) => IsDragging = false;

        /// <summary>
        /// Process node mouse drag event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessMouseDragEvent(Event currentEvent)
        {
            if (currentEvent.button == 0)
                ProcessLeftMouseDragEvent(currentEvent);
        }

        /// <summary>
        /// Process node left mouse drag event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessLeftMouseDragEvent(Event currentEvent)
        {
            IsDragging = true;
            DragNode(currentEvent.delta);
            GUI.changed = true;
        }

        /// <summary>
        /// Select and unselect node
        /// </summary>
        public void OnNodeLeftClick()
        {
            Selection.activeObject = this;
            IsSelected = !IsSelected;
        }

        /// <summary>
        /// Drag node
        /// </summary>
        /// <param name="delta"></param>
        public void DragNode(Vector2 delta)
        {
            Rect.position += delta;
            EditorUtility.SetDirty(this);
        }

        public virtual void RemoveChildConnection(Node childToRemove)
        {
            if (this is SentenceNode sentenceNode && sentenceNode.ChildNode == childToRemove)
                sentenceNode.ChildNode = null;
            else if (this is ExternalFunctionNode externalFunctionNode && externalFunctionNode.ChildNode == childToRemove)
                externalFunctionNode.ChildNode = null;
            else if (this is ModifyVariableNode modifyVariableNode && modifyVariableNode.ChildNode == childToRemove)
                modifyVariableNode.ChildNode = null;
            else if (this is VariableConditionNode conditionNode)
            {
                if (conditionNode.TrueChildNode == childToRemove)
                    conditionNode.TrueChildNode = null;
                if (conditionNode.FalseChildNode == childToRemove)
                    conditionNode.FalseChildNode = null;
            }
            else if (this is AnswerNode answerNode)
                answerNode.ChildNodes.Remove(childToRemove);
        }

        /// <summary>
        /// Remove a parent node connection
        /// </summary>
        /// <param name="nodeToRemove"></param>
        /// <returns></returns>
        public virtual bool RemoveFromParentConnectedNode(Node nodeToRemove) => false;

#endif
    }
}