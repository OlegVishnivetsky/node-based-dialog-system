using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace cherrydev
{
    public class NodeEditor : EditorWindow
    {
        private static DialogNodeGraph _currentNodeGraph;
        private Node _currentNode;

        private GUIStyle _nodeStyle;
        private GUIStyle _selectedNodeStyle;

        private GUIStyle _labelStyle;

        private Rect _selectionRect;
        private Vector2 _mouseScrollClickPosition;

        private Vector2 _graphOffset;
        private Vector2 _graphDrag;

        private const float NodeWidth = 190f;
        private const float NodeHeight = 135f;

        private const float ConnectingLineWidth = 2f;
        private const float ConnectingLineArrowSize = 4f;

        private const int LabelFontSize = 12;

        private const int NodePadding = 20;
        private const int NodeBorder = 10;

        private const float GridLargeLineSpacing = 100f;
        private const float GridSmallLineSpacing = 25;

        private bool _isScrollWheelDragging;

        /// <summary>
        /// Define nodes and lable style parameters on enable
        /// </summary>
        private void OnEnable()
        {
            Selection.selectionChanged += ChangeEditorWindowOnSelection;

            _nodeStyle = new GUIStyle();
            _nodeStyle.normal.background = EditorGUIUtility.Load(StringConstants.Node) as Texture2D;
            _nodeStyle.padding = new RectOffset(NodePadding, NodePadding, NodePadding, NodePadding);
            _nodeStyle.border = new RectOffset(NodeBorder, NodeBorder, NodeBorder, NodeBorder);

            _selectedNodeStyle = new GUIStyle();
            _selectedNodeStyle.normal.background = EditorGUIUtility.Load(StringConstants.SelectedNode) as Texture2D;
            _selectedNodeStyle.padding = new RectOffset(NodePadding, NodePadding, NodePadding, NodePadding);
            _selectedNodeStyle.border = new RectOffset(NodeBorder, NodeBorder, NodeBorder, NodeBorder);

            _labelStyle = new GUIStyle();
            _labelStyle.alignment = TextAnchor.MiddleLeft;
            _labelStyle.fontSize = LabelFontSize;
            _labelStyle.normal.textColor = Color.white;
        }

        /// <summary>
        /// Saving all changes and unsubscribing from events
        /// </summary>
        private void OnDisable()
        {
            Selection.selectionChanged -= ChangeEditorWindowOnSelection;
            AssetDatabase.SaveAssets();
            SaveChanges();
        }

        /// <summary>
        /// Open Node Editor Window when Node Graph asset is double clicked in the inspector
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        [OnOpenAsset(0)]
        public static bool OnDoubleClickAsset(int instanceID, int line)
        {
            DialogNodeGraph nodeGraph = EditorUtility.InstanceIDToObject(instanceID) as DialogNodeGraph;

            if (_currentNodeGraph != null)
                SetUpNodes();

            if (nodeGraph != null)
            {
                OpenWindow();
                _currentNodeGraph = nodeGraph;
                SetUpNodes();

                return true;
            }

            return false;
        }

        public static void SetCurrentNodeGraph(DialogNodeGraph nodeGraph) => 
            _currentNodeGraph = nodeGraph;

        /// <summary>
        /// Open Node Editor window
        /// </summary>
        [MenuItem("Dialog Node Based Editor", menuItem = "Window/Dialog Node Based Editor")]
        public static void OpenWindow()
        {
            NodeEditor window = (NodeEditor)GetWindow(typeof(NodeEditor));
            window.titleContent = new GUIContent("Dialog Graph Editor");
            window.Show();

        }

        /// <summary>
        /// Rendering and handling GUI events
        /// </summary>
        private void OnGUI()
        {
            if (_currentNodeGraph != null)
            {
                DrawDraggedLine();
                DrawNodeConnection();
                DrawGridBackground(GridSmallLineSpacing, 0.2f, Color.gray);
                DrawGridBackground(GridLargeLineSpacing, 0.2f, Color.gray);
                ProcessEvents(Event.current);
                DrawNodes(Event.current);
            }

            if (GUI.changed)
                Repaint();
        }

        /// <summary>
        /// Setting up nodes when opening the editor
        /// </summary>
        private static void SetUpNodes()
        {
            foreach (Node node in _currentNodeGraph.NodesList)
            {
                if (node.GetType() == typeof(AnswerNode))
                {
                    AnswerNode answerNode = (AnswerNode)node;
                    answerNode.CalculateAmountOfAnswers();
                    answerNode.CalculateAnswerNodeHeight();
                }
                if (node.GetType() == typeof(SentenceNode))
                {
                    SentenceNode sentenceNode = (SentenceNode)node;
                    sentenceNode.CheckNodeSize(NodeWidth, NodeHeight);
                }
            }
        }

        /// <summary>
        /// Draw connection line when we drag it
        /// </summary>
        private void DrawDraggedLine()
        {
            if (_currentNodeGraph.LinePosition != Vector2.zero)
            {
                Handles.DrawBezier(_currentNodeGraph.NodeToDrawLineFrom.Rect.center, _currentNodeGraph.LinePosition,
                   _currentNodeGraph.NodeToDrawLineFrom.Rect.center, _currentNodeGraph.LinePosition,
                   Color.white, null, ConnectingLineWidth);
            }
        }

        /// <summary>
        /// Draw connections in the editor window between nodes
        /// </summary>
        private void DrawNodeConnection()
        {
            if (_currentNodeGraph.NodesList == null)
                return;
            

            foreach (Node node in _currentNodeGraph.NodesList)
            {
                Node parentNode;
                Node childNode;

                if (node.GetType() == typeof(AnswerNode))
                {
                    AnswerNode answerNode = (AnswerNode)node;

                    for (int i = 0; i < answerNode.ChildSentenceNodes.Count; i++)
                    {
                        if (answerNode.ChildSentenceNodes[i] != null)
                        {
                            parentNode = node;
                            childNode = answerNode.ChildSentenceNodes[i];

                            DrawConnectionLine(parentNode, childNode);
                        }
                    }
                }
                else if (node.GetType() == typeof(SentenceNode))
                {
                    SentenceNode sentenceNode = (SentenceNode)node;

                    if (sentenceNode.ChildNode != null)
                    {
                        parentNode = node;
                        childNode = sentenceNode.ChildNode;

                        DrawConnectionLine(parentNode, childNode);
                    }
                }
            }
        }

        /// <summary>
        /// Draw connection line from parent to child node
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="childNode"></param>
        private void DrawConnectionLine(Node parentNode, Node childNode)
        {
            Vector2 startPosition = parentNode.Rect.center;
            Vector2 endPosition = childNode.Rect.center;

            Vector2 midPosition = (startPosition + endPosition) / 2;
            Vector2 direction = endPosition - startPosition;

            Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * ConnectingLineArrowSize;
            Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * ConnectingLineArrowSize;

            Vector2 arrowHeadPoint = midPosition + direction.normalized * ConnectingLineArrowSize;

            Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1,
                Color.white, null, ConnectingLineWidth);
            Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2,
                Color.white, null, ConnectingLineWidth);

            Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition,
                Color.white, null, ConnectingLineWidth);

            GUI.changed = true;
        }

        /// <summary>
        /// Draw grid background lines for node editor window
        /// </summary>
        /// <param name="gridSize"></param>
        /// <param name="gridOpacity"></param>
        /// <param name="color"></param>
        private void DrawGridBackground(float gridSize, float gridOpacity, Color color)
        {
            int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
            int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

            Handles.color = new Color(color.r, color.g, color.b, gridOpacity);

            _graphOffset += _graphDrag * 0.5f;

            Vector3 gridOffset = new Vector3(_graphOffset.x % gridSize, _graphOffset.y % gridSize, 0);

            for (int i = 0; i < verticalLineCount; i++)
                Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset, 
                    new Vector3(gridSize * i, position.height + gridSize, 0f) + gridOffset);
            

            for (int j = 0; j < horizontalLineCount; j++)
                Handles.DrawLine(new Vector3(-gridSize, gridSize * j, 0) + gridOffset, 
                    new Vector3(position.width + gridSize, gridSize * j, 0f) + gridOffset);
            
            Handles.color = Color.white;
        }

        /// <summary>
        /// Call Draw method from all existing nodes in nodes list
        /// </summary>
        private void DrawNodes(Event currentEvent)
        {
            if (_currentNodeGraph.NodesList == null) 
                return;
            
            foreach (Node node in _currentNodeGraph.NodesList)
            {
                if (!node.IsSelected)
                    node.Draw(_nodeStyle, _labelStyle);
                else
                    node.Draw(_selectedNodeStyle, _labelStyle);
            }
            
            if (_isScrollWheelDragging)
                SelectNodesBySelectionRect(currentEvent.mousePosition);

            GUI.changed = true;
        }

        /// <summary>
        /// Process events
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessEvents(Event currentEvent)
        {
            _graphDrag = Vector2.zero;

            if (currentEvent.type == EventType.MouseUp)
            {
                ProcessScrollWheelUpEvent(currentEvent);
                ProcessRightMouseUpEvent(currentEvent);
            }

            if (_currentNode == null || _currentNode.IsDragging == false)
                _currentNode = GetHighlightedNode(currentEvent.mousePosition);
            
            if (_currentNode == null || _currentNodeGraph.NodeToDrawLineFrom != null)
                ProcessNodeEditorEvents(currentEvent);
            else
                _currentNode.ProcessNodeEvents(currentEvent);
        }

        /// <summary>
        /// Process all events
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessNodeEditorEvents(Event currentEvent)
        {
            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    ProcessMouseDownEvent(currentEvent);
                    break;
                case EventType.MouseDrag:
                    ProcessMouseDragEvent(currentEvent);
                    break;
            }
        }

        /// <summary>
        /// Process mouse down event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessMouseDownEvent(Event currentEvent)
        {
            if (currentEvent.button == 1)
                ProcessRightMouseDownEvent(currentEvent);
            else if (currentEvent.button == 0 && _currentNodeGraph.NodesList != null)
                ProcessLeftMouseDownEvent(currentEvent);
            else if (currentEvent.button == 2)
                ProcessScrollWheelDownEvent(currentEvent);
        }

        /// <summary>
        /// Process right mouse click event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessRightMouseDownEvent(Event currentEvent)
        {
            if (GetHighlightedNode(currentEvent.mousePosition) == null)
                ShowContextMenu(currentEvent.mousePosition);
        }

        /// <summary>
        /// Process left mouse click event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessLeftMouseDownEvent(Event currentEvent) => 
            ProcessNodeSelection(currentEvent.mousePosition);

        /// <summary>
        /// Process scroll wheel down event
        /// </summary>
        /// <param name="currentEvent"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ProcessScrollWheelDownEvent(Event currentEvent)
        {
            _mouseScrollClickPosition = currentEvent.mousePosition;
            _isScrollWheelDragging = true;
        }

        /// <summary>
        /// Process right mouse up event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessRightMouseUpEvent(Event currentEvent)
        {
            if (_currentNodeGraph.NodeToDrawLineFrom != null)
            {
                CheckLineConnection(currentEvent);
                ClearDraggedLine();
            }
        }

        /// <summary>
        /// Process scroll wheel up event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessScrollWheelUpEvent(Event currentEvent)
        {
            _selectionRect = new Rect(0, 0, 0, 0);
            _isScrollWheelDragging = false;
        }

        /// <summary>
        /// Process mouse drag event
        /// </summary>
        /// <param name="currentEvent"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ProcessMouseDragEvent(Event currentEvent)
        {
            if (currentEvent.button == 0)
                ProcessLeftMouseDragEvent(currentEvent);
            else if (currentEvent.button == 1)
                ProcessRightMouseDragEvent(currentEvent);
        }

        /// <summary>
        /// Process left mouse drag event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessLeftMouseDragEvent(Event currentEvent)
        {
            _graphDrag = currentEvent.delta;

            foreach (var node in _currentNodeGraph.NodesList)
                node.DragNode(_graphDrag);

            GUI.changed = true;
        }

        /// <summary>
        /// Process right mouse drag event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessRightMouseDragEvent(Event currentEvent)
        {
            if (_currentNodeGraph.NodeToDrawLineFrom != null)
            {
                DragConnectionLine(currentEvent.delta);
                GUI.changed = true;
            }
        }

        /// <summary>
        /// Drag connecting line from the node
        /// </summary>
        /// <param name="delta"></param>
        private void DragConnectionLine(Vector2 delta) => 
            _currentNodeGraph.LinePosition += delta;
        
        /// <summary>
        /// Check line connect when right mouse up
        /// </summary>
        /// <param name="currentEvent"></param>
        private void CheckLineConnection(Event currentEvent)
        {
            if (_currentNodeGraph.NodeToDrawLineFrom != null)
            {
                Node node = GetHighlightedNode(currentEvent.mousePosition);

                if (node != null)
                {
                    _currentNodeGraph.NodeToDrawLineFrom.AddToChildConnectedNode(node);
                    node.AddToParentConnectedNode(_currentNodeGraph.NodeToDrawLineFrom);
                }
            }
        }

        /// <summary>
        /// Clear dragged line
        /// </summary>
        private void ClearDraggedLine()
        {
            _currentNodeGraph.NodeToDrawLineFrom = null;
            _currentNodeGraph.LinePosition = Vector2.zero;
            GUI.changed = true;
        }

        /// <summary>
        /// Process node selection, add to selected node list if node is selected
        /// </summary>
        /// <param name="mouseClickPosition"></param>
        private void ProcessNodeSelection(Vector2 mouseClickPosition)
        {
            Node clickedNode = GetHighlightedNode(mouseClickPosition);

            //unselect all nodes when clicking outside a node
            if (clickedNode == null)
            {
                foreach (Node node in _currentNodeGraph.NodesList)
                {
                    if (node.IsSelected) 
                        node.IsSelected = false;
                }
            }
        }

        /// <summary>
        /// Draw selection rect and select all node in it
        /// </summary>
        /// <param name="mousePosition"></param>
        private void SelectNodesBySelectionRect(Vector2 mousePosition)
        {
            if (!_isScrollWheelDragging)
                return;

            // Normalize the rectangle to handle any drag direction
            _selectionRect = new Rect(
                Mathf.Min(_mouseScrollClickPosition.x, mousePosition.x),
                Mathf.Min(_mouseScrollClickPosition.y, mousePosition.y),
                Mathf.Abs(mousePosition.x - _mouseScrollClickPosition.x),
                Mathf.Abs(mousePosition.y - _mouseScrollClickPosition.y)
            );

            foreach (Node node in _currentNodeGraph.NodesList)
            {
                if (_selectionRect.Overlaps(node.Rect, true))
                    node.IsSelected = true;
                else
                    node.IsSelected = false;
            }
            
            GUI.color = new Color(0, 0, 0, 0.5f);
            GUI.DrawTexture(_selectionRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        /// <summary>
        /// Return the node that is at the mouse position
        /// </summary>
        /// <param name="mousePosition"></param>
        /// <returns></returns>
        private Node GetHighlightedNode(Vector2 mousePosition)
        {
            if (_currentNodeGraph.NodesList.Count == 0)
                return null;
            
            foreach (Node node in _currentNodeGraph.NodesList)
            {
                if (node.Rect.Contains(mousePosition))
                    return node;
            }

            return null;
        }

        /// <summary>
        /// Show the context menu
        /// </summary>
        /// <param name="mousePosition"></param>
        private void ShowContextMenu(Vector2 mousePosition)
        {
            GenericMenu contextMenu = new GenericMenu();

            contextMenu.AddItem(new GUIContent("Create Sentence Node"), false, CreateSentenceNode, mousePosition);
            contextMenu.AddItem(new GUIContent("Create Answer Node"), false, CreateAnswerNode, mousePosition);
            contextMenu.AddSeparator("");
            contextMenu.AddItem(new GUIContent("Select All Nodes"), false, SelectAllNodes, mousePosition);
            contextMenu.AddItem(new GUIContent("Remove Selected Nodes"), false, RemoveSelectedNodes, mousePosition);
            contextMenu.AddItem(new GUIContent("Remove Connections"), false, RemoveAllConnections, mousePosition);
            contextMenu.AddSeparator("");
            contextMenu.AddItem(new GUIContent("Find My Nodes"), false, CenterWindowOnNodes, mousePosition);
            contextMenu.ShowAsContext();
        }

        /// <summary>
        /// Create Sentence Node at mouse position and add it to Node Graph asset
        /// </summary>
        /// <param name="mousePositionObject"></param>
        private void CreateSentenceNode(object mousePositionObject)
        {
            SentenceNode sentenceNode = CreateInstance<SentenceNode>();
            InitialiseNode(mousePositionObject, sentenceNode, "Sentence Node");
        }

        /// <summary>
        /// Create Answer Node at mouse position and add it to Node Graph asset
        /// </summary>
        /// <param name="mousePositionObject"></param>
        private void CreateAnswerNode(object mousePositionObject)
        {
            AnswerNode answerNode = CreateInstance<AnswerNode>();
            InitialiseNode(mousePositionObject, answerNode, "Answer Node");
        }

        /// <summary>
        /// Select all nodes in node editor
        /// </summary>
        /// <param name="userData"></param>
        private void SelectAllNodes(object userData)
        {
            foreach (Node node in _currentNodeGraph.NodesList)
                node.IsSelected = true;
            
            GUI.changed = true;
        }

        /// <summary>
        /// Remove all selected nodes
        /// </summary>
        /// <param name="userData"></param>
        private void RemoveSelectedNodes(object userData)
        {
            Queue<Node> nodeDeletionQueue = new Queue<Node>();

            foreach (Node node in _currentNodeGraph.NodesList)
            {
                if (node.IsSelected)
                    nodeDeletionQueue.Enqueue(node);
            }

            while (nodeDeletionQueue.Count > 0)
            {
                Node nodeToDelete = nodeDeletionQueue.Dequeue();

                _currentNodeGraph.NodesList.Remove(nodeToDelete);

                DestroyImmediate(nodeToDelete, true);
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Create Node at mouse position and add it to Node Graph asset
        /// </summary>
        /// <param name="mousePositionObject"></param>
        /// <param name="node"></param>
        /// <param name="nodeName"></param>
        private void InitialiseNode(object mousePositionObject, Node node, string nodeName)
        {
            Vector2 mousePosition = (Vector2)mousePositionObject;

            _currentNodeGraph.NodesList.Add(node);

            node.Initialise(new Rect(mousePosition, new Vector2(NodeWidth, NodeHeight)), nodeName, _currentNodeGraph);

            AssetDatabase.AddObjectToAsset(node, _currentNodeGraph);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Chance current node graph and draw the new one
        /// </summary>
        private void ChangeEditorWindowOnSelection()
        {
            DialogNodeGraph nodeGraph = Selection.activeObject as DialogNodeGraph;

            if (nodeGraph != null)
            {
                _currentNodeGraph = nodeGraph;
                GUI.changed = true;
            }
        }

        /// <summary>
        /// Clears all connections in the selected nodes
        /// </summary>
        /// <param name="userData"></param>
        private void RemoveAllConnections(object userData)
        {
            foreach (Node node in _currentNodeGraph.NodesList)
            {
                if (node.GetType() == typeof(AnswerNode))
                {
                    AnswerNode answerNode = (AnswerNode)node;
                    answerNode.ParentSentenceNode = null;
                    answerNode.ChildSentenceNodes.Clear();
                }
                else if (node.GetType() == typeof(SentenceNode))
                {
                    SentenceNode sentenceNode = (SentenceNode)node;
                    sentenceNode.ParentNode = null;
                    sentenceNode.ChildNode = null;
                }
            }
        }
        
        /// <summary>
        /// Center the node editor window on all nodes
        /// </summary>
        private void CenterWindowOnNodes(object userData)
        {
            Vector2 nodesCenter = CalculateNodesCenter();
            Vector2 canvasCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

            Vector2 offset = canvasCenter - nodesCenter;

            foreach (var node in _currentNodeGraph.NodesList)
                node.DragNode(offset);
            
            GUI.changed = true;
        }
        
        /// <summary>
        /// Calculate the center of all nodes
        /// </summary>
        /// <returns>The center position of all nodes</returns>
        private Vector2 CalculateNodesCenter()
        {
            if (_currentNodeGraph.NodesList == null || _currentNodeGraph.NodesList.Count == 0)
                return Vector2.zero;

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (var node in _currentNodeGraph.NodesList)
            {
                Rect nodeRect = node.Rect;
                minX = Mathf.Min(minX, nodeRect.xMin);
                maxX = Mathf.Max(maxX, nodeRect.xMax);
                minY = Mathf.Min(minY, nodeRect.yMin);
                maxY = Mathf.Max(maxY, nodeRect.yMax);
            }

            return new Vector2((minX + maxX) / 2, (minY + maxY) / 2);
        }
    }
}