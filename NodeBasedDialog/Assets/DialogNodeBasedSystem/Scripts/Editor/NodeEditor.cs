using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace cherrydev
{
    public class NodeEditor : EditorWindow
    {
        private static DialogNodeGraph currentNodeGraph;
        private Node currentNode;

        private GUIStyle nodeStyle;
        private GUIStyle selectedNodeStyle;

        private GUIStyle lableStyle;

        private Rect selectionRect;
        private Vector2 mouseScrollClickPosition;

        private Vector2 graphOffset;
        private Vector2 graphDrag;

        private const float nodeWidth = 190f;
        private const float nodeHeight = 135f;

        private const float connectingLineWidth = 2f;
        private const float connectingLineArrowSize = 4f;

        private const int lableFontSize = 12;

        private const int nodePadding = 20;
        private const int nodeBorder = 10;

        private const float gridLargeLineSpacing = 100f;
        private const float gridSmallLineSpacing = 25;

        private bool isScrollWheelDragging = false;

        /// <summary>
        /// Define nodes and lable style parameters on enable
        /// </summary>
        private void OnEnable()
        {
            Selection.selectionChanged += ChangeEditorWindowOnSelection;

            nodeStyle = new GUIStyle();
            nodeStyle.normal.background = EditorGUIUtility.Load(StringConstants.Node) as Texture2D;
            nodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
            nodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

            selectedNodeStyle = new GUIStyle();
            selectedNodeStyle.normal.background = EditorGUIUtility.Load(StringConstants.SelectedNode) as Texture2D;
            selectedNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
            selectedNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

            lableStyle = new GUIStyle();
            lableStyle.alignment = TextAnchor.MiddleLeft;
            lableStyle.fontSize = lableFontSize;
            lableStyle.normal.textColor = Color.white;
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

            if (currentNodeGraph != null)
            {
                SetUpNodes();
            }

            if (nodeGraph != null)
            {
                OpenWindow();

                currentNodeGraph = nodeGraph;

                SetUpNodes();

                return true;
            }

            return false;
        }

        public static void SetCurrentNodeGraph(DialogNodeGraph nodeGraph)
        {
            currentNodeGraph = nodeGraph;
        }

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
            if (currentNodeGraph != null)
            {
                DrawDraggedLine();
                DrawNodeConnection();
                DrawGridBackground(gridSmallLineSpacing, 0.2f, Color.gray);
                DrawGridBackground(gridLargeLineSpacing, 0.2f, Color.gray);
                ProcessEvents(Event.current);
                DrawNodes();
            }

            if (GUI.changed)
                Repaint();
        }

        /// <summary>
        /// Setting up nodes when opening the editor
        /// </summary>
        private static void SetUpNodes()
        {
            foreach (Node node in currentNodeGraph.nodesList)
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
                    sentenceNode.CheckNodeSize(nodeWidth, nodeHeight);
                }
            }
        }

        /// <summary>
        /// Draw connection line when we drag it
        /// </summary>
        private void DrawDraggedLine()
        {
            if (currentNodeGraph.linePosition != Vector2.zero)
            {
                Handles.DrawBezier(currentNodeGraph.nodeToDrawLineFrom.rect.center, currentNodeGraph.linePosition,
                   currentNodeGraph.nodeToDrawLineFrom.rect.center, currentNodeGraph.linePosition,
                   Color.white, null, connectingLineWidth);
            }
        }

        /// <summary>
        /// Draw connections in the editor window between nodes
        /// </summary>
        private void DrawNodeConnection()
        {
            if (currentNodeGraph.nodesList == null)
            {
                return;
            }

            foreach (Node node in currentNodeGraph.nodesList)
            {
                Node parentNode = null;
                Node childNode = null;

                if (node.GetType() == typeof(AnswerNode))
                {
                    AnswerNode answerNode = (AnswerNode)node;

                    for (int i = 0; i < answerNode.childSentenceNodes.Count; i++)
                    {
                        if (answerNode.childSentenceNodes[i] != null)
                        {
                            parentNode = node;
                            childNode = answerNode.childSentenceNodes[i];

                            DrawConnectionLine(parentNode, childNode);
                        }
                    }
                }
                else if (node.GetType() == typeof(SentenceNode))
                {
                    SentenceNode sentenceNode = (SentenceNode)node;

                    if (sentenceNode.childNode != null)
                    {
                        parentNode = node;
                        childNode = sentenceNode.childNode;

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
            Vector2 startPosition = parentNode.rect.center;
            Vector2 endPosition = childNode.rect.center;

            Vector2 midPosition = (startPosition + endPosition) / 2;
            Vector2 direction = endPosition - startPosition;

            Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
            Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;

            Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

            Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1,
                Color.white, null, connectingLineWidth);
            Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2,
                Color.white, null, connectingLineWidth);

            Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition,
                Color.white, null, connectingLineWidth);

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

            graphOffset += graphDrag * 0.5f;

            Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);

            for (int i = 0; i < verticalLineCount; i++)
            {
                Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset, new Vector3(gridSize * i, position.height + gridSize, 0f) + gridOffset);
            }

            for (int j = 0; j < horizontalLineCount; j++)
            {
                Handles.DrawLine(new Vector3(-gridSize, gridSize * j, 0) + gridOffset, new Vector3(position.width + gridSize, gridSize * j, 0f) + gridOffset);
            }

            Handles.color = Color.white;
        }

        /// <summary>
        /// Call Draw method from all existing nodes in nodes list
        /// </summary>
        private void DrawNodes()
        {
            if (currentNodeGraph.nodesList == null)
            {
                return;
            }

            foreach (Node node in currentNodeGraph.nodesList)
            {
                if (!node.isSelected)
                {
                    node.Draw(nodeStyle, lableStyle);
                }
                else
                {
                    node.Draw(selectedNodeStyle, lableStyle);
                }
            }

            GUI.changed = true;
        }

        /// <summary>
        /// Process events
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessEvents(Event currentEvent)
        {
            graphDrag = Vector2.zero;

            if (currentNode == null || currentNode.isDragging == false)
            {
                currentNode = GetHighlightedNode(currentEvent.mousePosition);
            }

            if (currentNode == null || currentNodeGraph.nodeToDrawLineFrom != null)
            {
                ProcessNodeEditorEvents(currentEvent);
            }
            else
            {
                currentNode.ProcessNodeEvents(currentEvent);
            }
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

                case EventType.MouseUp:
                    ProcessMouseUpEvent(currentEvent);
                    break;

                case EventType.MouseDrag:
                    ProcessMouseDragEvent(currentEvent);
                    break;

                case EventType.Repaint:
                    SelectNodesBySelectionRect(currentEvent.mousePosition);
                    break;

                default:
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
            {
                ProcessRightMouseDownEvent(currentEvent);
            }
            else if (currentEvent.button == 0 && currentNodeGraph.nodesList != null)
            {
                ProcessLeftMouseDownEvent(currentEvent);
            }
            else if (currentEvent.button == 2)
            {
                ProcessScrollWheelDownEvent(currentEvent);
            }
        }

        /// <summary>
        /// Process right mouse click event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessRightMouseDownEvent(Event currentEvent)
        {
            if (GetHighlightedNode(currentEvent.mousePosition) == null)
            {
                ShowContextMenu(currentEvent.mousePosition);
            }
        }

        /// <summary>
        /// Process left mouse click event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessLeftMouseDownEvent(Event currentEvent)
        {
            ProcessNodeSelection(currentEvent.mousePosition);
        }

        /// <summary>
        /// Process scroll wheel down event
        /// </summary>
        /// <param name="currentEvent"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ProcessScrollWheelDownEvent(Event currentEvent)
        {
            mouseScrollClickPosition = currentEvent.mousePosition;
            isScrollWheelDragging = true;
        }

        /// <summary>
        /// Process mouse up event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessMouseUpEvent(Event currentEvent)
        {
            if (currentEvent.button == 1)
            {
                ProcessRightMouseUpEvent(currentEvent);
            }
            else if (currentEvent.button == 2)
            {
                ProcessScrollWheelUpEvent(currentEvent);
            }
        }

        /// <summary>
        /// Process right mouse up event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessRightMouseUpEvent(Event currentEvent)
        {
            if (currentNodeGraph.nodeToDrawLineFrom != null)
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
            selectionRect = new Rect(0, 0, 0, 0);
            isScrollWheelDragging = false;
        }

        /// <summary>
        /// Process mouse drag event
        /// </summary>
        /// <param name="currentEvent"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ProcessMouseDragEvent(Event currentEvent)
        {
            if (currentEvent.button == 0)
            {
                ProcessLeftMouseDragEvent(currentEvent);
            }
            else if (currentEvent.button == 1)
            {
                ProcessRightMouseDragEvent(currentEvent);
            }
        }

        /// <summary>
        /// Process left mouse drag event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessLeftMouseDragEvent(Event currentEvent)
        {
            SelectNodesBySelectionRect(currentEvent.mousePosition);

            graphDrag = currentEvent.delta;

            foreach (var node in currentNodeGraph.nodesList)
            {
                node.DragNode(graphDrag);
            }

            GUI.changed = true;
        }

        /// <summary>
        /// Process right mouse drag event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessRightMouseDragEvent(Event currentEvent)
        {
            if (currentNodeGraph.nodeToDrawLineFrom != null)
            {
                DragConnectiongLine(currentEvent.delta);
                GUI.changed = true;
            }
        }

        /// <summary>
        /// Drag connecting line from the node
        /// </summary>
        /// <param name="delta"></param>
        private void DragConnectiongLine(Vector2 delta)
        {
            currentNodeGraph.linePosition += delta;
        }

        /// <summary>
        /// Check line connect when right mouse up
        /// </summary>
        /// <param name="currentEvent"></param>
        private void CheckLineConnection(Event currentEvent)
        {
            if (currentNodeGraph.nodeToDrawLineFrom != null)
            {
                Node node = GetHighlightedNode(currentEvent.mousePosition);

                if (node != null)
                {
                    currentNodeGraph.nodeToDrawLineFrom.AddToChildConnectedNode(node);
                    node.AddToParentConnectedNode(currentNodeGraph.nodeToDrawLineFrom);
                }
            }
        }

        /// <summary>
        /// Clear dragged line
        /// </summary>
        private void ClearDraggedLine()
        {
            currentNodeGraph.nodeToDrawLineFrom = null;
            currentNodeGraph.linePosition = Vector2.zero;
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
                foreach (Node node in currentNodeGraph.nodesList)
                {
                    if (node.isSelected)
                    {
                        node.isSelected = false;
                    }
                }

                return;
            }
        }

        /// <summary>
        /// Draw selection rect and select all node in it
        /// </summary>
        /// <param name="mousePosition"></param>
        private void SelectNodesBySelectionRect(Vector2 mousePosition)
        {
            if (!isScrollWheelDragging)
            {
                return;
            }

            selectionRect = new Rect(mouseScrollClickPosition.x, mouseScrollClickPosition.y,
                mousePosition.x - mouseScrollClickPosition.x, mousePosition.y - mouseScrollClickPosition.y);

            EditorGUI.DrawRect(selectionRect, new Color(0, 0, 0, 0.5f));


            foreach (Node node in currentNodeGraph.nodesList)
            {
                if (selectionRect.Contains(node.rect.position))
                {
                    node.isSelected = true;
                }
            }
        }

        /// <summary>
        /// Return the node that is at the mouse position
        /// </summary>
        /// <param name="mousePosition"></param>
        /// <returns></returns>
        private Node GetHighlightedNode(Vector2 mousePosition)
        {
            if (currentNodeGraph.nodesList.Count == 0)
            {
                return null;
            }

            foreach (Node node in currentNodeGraph.nodesList)
            {
                if (node.rect.Contains(mousePosition))
                {
                    return node;
                }
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
            contextMenu.ShowAsContext();
        }

        /// <summary>
        /// Create Sentence Node at mouse position and add it to Node Graph asset
        /// </summary>
        /// <param name="mousePositionObject"></param>
        private void CreateSentenceNode(object mousePositionObject)
        {
            SentenceNode sentenceNode = ScriptableObject.CreateInstance<SentenceNode>();
            InitialiseNode(mousePositionObject, sentenceNode, "Sentence Node");
        }

        /// <summary>
        /// Create Answer Node at mouse position and add it to Node Graph asset
        /// </summary>
        /// <param name="mousePositionObject"></param>
        private void CreateAnswerNode(object mousePositionObject)
        {
            AnswerNode answerNode = ScriptableObject.CreateInstance<AnswerNode>();
            InitialiseNode(mousePositionObject, answerNode, "Answer Node");
        }

        /// <summary>
        /// Select all nodes in node editor
        /// </summary>
        /// <param name="userData"></param>
        private void SelectAllNodes(object userData)
        {
            foreach (Node node in currentNodeGraph.nodesList)
            {
                node.isSelected = true;
            }

            GUI.changed = true;
        }

        /// <summary>
        /// Remove all selected nodes
        /// </summary>
        /// <param name="userData"></param>
        private void RemoveSelectedNodes(object userData)
        {
            Queue<Node> nodeDeletionQueue = new Queue<Node>();

            foreach (Node node in currentNodeGraph.nodesList)
            {
                if (node.isSelected)
                {
                    nodeDeletionQueue.Enqueue(node);
                }
            }

            while (nodeDeletionQueue.Count > 0)
            {
                Node nodeTodelete = nodeDeletionQueue.Dequeue();

                currentNodeGraph.nodesList.Remove(nodeTodelete);

                DestroyImmediate(nodeTodelete, true);
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

            currentNodeGraph.nodesList.Add(node);

            node.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), nodeName, currentNodeGraph);

            AssetDatabase.AddObjectToAsset(node, currentNodeGraph);
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
                currentNodeGraph = nodeGraph;
                GUI.changed = true;
            }
        }
    }
}