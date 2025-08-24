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
        private Node _nodeToDragLineFrom;

        private GUIStyle _nodeStyle;
        private GUIStyle _selectedNodeStyle;

        private readonly Color _headerColor = new(0.235f, 0.235f, 0.235f);
        private readonly Color _backgroundColor = new(0.165f, 0.165f, 0.165f);
        private readonly Color _backgroundLinesColor = new(0.113f, 0.113f, 0.113f);

        private GUIStyle _toolbarButtonStyle;
        private GUIStyle _headerLabelStyle;
        private GUIStyle _dropdownStyle;
        private GUIStyle _searchFieldStyle;

        private GUIStyle _labelStyle;

        private Rect _selectionRect;
        private Vector2 _mouseScrollClickPosition;

        private Vector2 _graphOffset;
        private Vector2 _graphDrag;

        private GUIStyle _activeToolbarButtonStyle;

        private const float NodeWidth = 190f;
        private const float NodeHeight = 135f;

        private const float ToolbarHeight = 30f;

        private const float ConnectingLineWidth = 2f;
        private const float ConnectingLineArrowSize = 4f;

        private const int LabelFontSize = 12;

        private const int NodePadding = 20;
        private const int NodeBorder = 10;

        private const float GridLargeLineSpacing = 100f;
        private const float GridSmallLineSpacing = 25;

        private bool _isLeftMouseDragFromEmpty;
        private bool _isMiddleMouseClickedOnNode;

        private bool _showLocalizationKeys;
        private bool _showNodesDropdown;
        private bool _stylesInitialized;

        private bool _isConnectingVariableNode;
        private bool _isConnectingToTrue;

        private string _searchText = "";

        /// <summary>
        /// Define nodes and label style parameters on enable
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
            CleanUpUnusedAssets();
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
            if (!_stylesInitialized)
            {
                InitializeToolbarStyles();
                _stylesInitialized = true;
            }

            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), _backgroundColor);
            DrawToolbar();
            GUI.BeginGroup(new Rect(0, ToolbarHeight, position.width, position.height - ToolbarHeight));

            if (_currentNodeGraph != null)
            {
                Undo.RecordObject(_currentNodeGraph, "Changed Value");
                DrawDraggedLine();
                DrawNodeConnection();
                DrawGridBackground(GridSmallLineSpacing, 0.3f, _backgroundLinesColor);
                DrawGridBackground(GridLargeLineSpacing, 0.3f, _backgroundLinesColor);
                ProcessEvents(Event.current);
                DrawNodes(Event.current);
            }

            GUI.EndGroup();

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
        /// Initializes toolbar styles
        /// </summary>
        private void InitializeToolbarStyles()
        {
            _toolbarButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
            _toolbarButtonStyle.normal.textColor = Color.white;
            _toolbarButtonStyle.fontSize = 11;
            _toolbarButtonStyle.alignment = TextAnchor.MiddleCenter;
            _toolbarButtonStyle.fixedHeight = ToolbarHeight - 4;
            _toolbarButtonStyle.margin = new RectOffset(2, 2, 2, 2);
            _toolbarButtonStyle.padding = new RectOffset(6, 6, 2, 2);

            _activeToolbarButtonStyle = new GUIStyle(_toolbarButtonStyle);
            _activeToolbarButtonStyle.normal.background =
                EditorGUIUtility.Load("builtin skins/darkskin/images/btn act.png") as Texture2D;
            _activeToolbarButtonStyle.normal.textColor = Color.white;

            _headerLabelStyle = new GUIStyle(EditorStyles.label);
            _headerLabelStyle.normal.textColor = Color.white;
            _headerLabelStyle.fontStyle = FontStyle.Bold;
            _headerLabelStyle.fontSize = 12;
            _headerLabelStyle.alignment = TextAnchor.MiddleLeft;
            _headerLabelStyle.padding = new RectOffset(10, 10, 0, 0);

            _dropdownStyle = new GUIStyle(EditorStyles.popup);
            _dropdownStyle.normal.textColor = Color.white;
            _dropdownStyle.fontSize = 11;
            _dropdownStyle.alignment = TextAnchor.MiddleLeft;
            _dropdownStyle.fixedHeight = ToolbarHeight - 4;
            _dropdownStyle.margin = new RectOffset(2, 2, 2, 2);
            _dropdownStyle.padding = new RectOffset(6, 6, 2, 2);

            _searchFieldStyle = new GUIStyle(EditorStyles.toolbarTextField);
            _searchFieldStyle.normal.textColor = Color.white;
            _searchFieldStyle.fontSize = 11;
            _searchFieldStyle.alignment = TextAnchor.MiddleLeft;
            _searchFieldStyle.fixedHeight = ToolbarHeight - 4;
            _searchFieldStyle.margin = new RectOffset(2, 2, 2, 2);
            _searchFieldStyle.padding = new RectOffset(6, 6, 2, 2);
        }

        /// <summary>
        /// Clean up orphaned node assets that are sub-assets of the graph but not in the NodesList
        /// </summary>
        private void CleanUpUnusedAssets()
        {
            if (_currentNodeGraph == null)
                return;

            UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(
                AssetDatabase.GetAssetPath(_currentNodeGraph));

            foreach (UnityEngine.Object subAsset in subAssets)
            {
                Node nodeAsset = subAsset as Node;

                if (nodeAsset == null)
                    continue;

                if (!_currentNodeGraph.NodesList.Contains(nodeAsset))
                    DestroyImmediate(nodeAsset, true);
            }
        }

        /// <summary>
        /// Draws toolbar with helper buttons
        /// </summary>
        private void DrawToolbar()
        {
            EditorGUI.DrawRect(new Rect(0, 0, position.width, ToolbarHeight), _headerColor);
            GUILayout.BeginArea(new Rect(0, 0, position.width, ToolbarHeight));
            GUILayout.BeginHorizontal();

            if (_currentNodeGraph != null && _currentNodeGraph.NodesList != null &&
                _currentNodeGraph.NodesList.Count > 0)
            {
                Rect nodesButtonRect =
                    GUILayoutUtility.GetRect(new GUIContent("Nodes"), _toolbarButtonStyle, GUILayout.Width(100));
                if (GUI.Button(nodesButtonRect, "Nodes", _toolbarButtonStyle))
                    DrawNodesDropdown(nodesButtonRect);

                GUILayout.Space(10f);

                string newSearchText = EditorGUILayout.TextField(_searchText, _searchFieldStyle, GUILayout.Width(200));

                if (newSearchText != _searchText)
                {
                    _searchText = newSearchText;
                    if (!string.IsNullOrEmpty(_searchText))
                        SearchAndSelectNode(_searchText);
                }

                if (GUILayout.Button("Search", _toolbarButtonStyle, GUILayout.Width(60)))
                    SearchAndSelectNode(_searchText);

                if (!string.IsNullOrEmpty(_searchText))
                {
                    if (GUILayout.Button("×", _toolbarButtonStyle, GUILayout.Width(20)))
                        _searchText = "";
                }
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Find My Nodes", _toolbarButtonStyle, GUILayout.Width(100)))
                CenterWindowOnNodes();

            if (GUILayout.Button("Edit Table Keys",
                    _showLocalizationKeys ? _activeToolbarButtonStyle : _toolbarButtonStyle, GUILayout.Width(100)))
            {
                _showLocalizationKeys = !_showLocalizationKeys;
                DialogNodeGraph.ShowLocalizationKeys = _showLocalizationKeys;
                GUI.changed = true;
            }

            if (GUILayout.Button("Localization", _toolbarButtonStyle, GUILayout.Width(100)))
            {
                GenericMenu localizationMenu = new GenericMenu();
                localizationMenu.AddItem(new GUIContent("Set Up Localization Table"), false,
                    () => NodeGraphLocalizer.Instance.SetupLocalization(_currentNodeGraph));
                localizationMenu.AddItem(new GUIContent("Update Keys"), false,
                    () => NodeGraphLocalizer.Instance.SetupLocalization(_currentNodeGraph, false));
                localizationMenu.DropDown(new Rect(position.width - 100, ToolbarHeight, 150, 0));
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        /// <summary>
        /// Searches for a node containing the search text and selects it
        /// </summary>
        /// <param name="searchText">The text to search for</param>
        private void SearchAndSelectNode(string searchText)
        {
            if (string.IsNullOrEmpty(searchText) || _currentNodeGraph == null || _currentNodeGraph.NodesList == null)
                return;

            searchText = searchText.ToLower();

            foreach (Node node in _currentNodeGraph.NodesList)
            {
                if (node.GetType() == typeof(SentenceNode))
                {
                    SentenceNode sentenceNode = (SentenceNode)node;
                    string nodeText = sentenceNode.Sentence.Text?.ToLower() ?? "";

                    if (nodeText.Contains(searchText))
                    {
                        CenterAndSelectNode(node);
                        return;
                    }
                }
                else if (node.GetType() == typeof(AnswerNode))
                {
                    AnswerNode answerNode = (AnswerNode)node;
                    bool found = false;

                    if (answerNode.Answers != null)
                    {
                        foreach (string answer in answerNode.Answers)
                        {
                            if (!string.IsNullOrEmpty(answer) && answer.ToLower().Contains(searchText))
                            {
                                found = true;
                                break;
                            }
                        }
                    }

                    if (found)
                    {
                        CenterAndSelectNode(node);
                        return;
                    }
                }
                else if (node.GetType() == typeof(ExternalFunctionNode))
                {
                    ExternalFunctionNode externalFunctionNode = (ExternalFunctionNode)node;
                    if ((!string.IsNullOrEmpty(externalFunctionNode.FunctionName) &&
                         externalFunctionNode.FunctionName.ToLower().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(externalFunctionNode.Description) &&
                         externalFunctionNode.Description.ToLower().Contains(searchText)))
                    {
                        CenterAndSelectNode(node);
                        return;
                    }
                }
                else if (node.GetType() == typeof(ModifyVariableNode))
                {
                    ModifyVariableNode modifyVariableNode = (ModifyVariableNode)node;
                    if (!string.IsNullOrEmpty(modifyVariableNode.VariableName) &&
                        modifyVariableNode.VariableName.ToLower().Contains(searchText))
                    {
                        CenterAndSelectNode(node);
                        return;
                    }
                }
                else if (node.GetType() == typeof(VariableConditionNode))
                {
                    VariableConditionNode variableConditionNode = (VariableConditionNode)node;
                    if (!string.IsNullOrEmpty(variableConditionNode.VariableName) &&
                        variableConditionNode.VariableName.ToLower().Contains(searchText))
                    {
                        CenterAndSelectNode(node);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Draws the nodes dropdown in the toolbar
        /// </summary>
        /// <param name="buttonRect">The rectangle of the button that triggered the dropdown</param>
        private void DrawNodesDropdown(Rect buttonRect)
        {
            GenericMenu nodesMenu = new GenericMenu();

            foreach (Node node in _currentNodeGraph.NodesList)
            {
                string prefix;
                string nodeText;

                if (node.GetType() == typeof(SentenceNode))
                {
                    SentenceNode sentenceNode = (SentenceNode)node;
                    prefix = "S";
                    nodeText = !string.IsNullOrEmpty(sentenceNode.Sentence.Text) ? sentenceNode.Sentence.Text : "Empty";

                    if (nodeText.Length > 20)
                        nodeText = nodeText.Substring(0, 20) + "...";
                }
                else if (node.GetType() == typeof(AnswerNode))
                {
                    AnswerNode answerNode = (AnswerNode)node;
                    prefix = "A";

                    if (answerNode.Answers != null && answerNode.Answers.Count > 0 &&
                        !string.IsNullOrEmpty(answerNode.Answers[0]))
                        nodeText = answerNode.Answers[0];
                    else
                        nodeText = "Empty";

                    if (nodeText.Length > 20)
                        nodeText = nodeText.Substring(0, 20) + "...";
                }
                else if (node.GetType() == typeof(ExternalFunctionNode))
                {
                    ExternalFunctionNode externalFunctionNode = (ExternalFunctionNode)node;
                    prefix = "EF";
                    nodeText = !string.IsNullOrEmpty(externalFunctionNode.FunctionName)
                        ? $"Function {externalFunctionNode.FunctionName}"
                        : "Empty";

                    if (nodeText.Length > 20)
                        nodeText = nodeText.Substring(0, 20) + "...";
                }
                else if (node.GetType() == typeof(ModifyVariableNode))
                {
                    ModifyVariableNode modifyVariableNode = (ModifyVariableNode)node;
                    prefix = "MV";
                    nodeText = !string.IsNullOrEmpty(modifyVariableNode.VariableName)
                        ? $"Modify {modifyVariableNode.VariableName}"
                        : "Empty";

                    if (nodeText.Length > 20)
                        nodeText = nodeText.Substring(0, 20) + "...";
                }
                else if (node.GetType() == typeof(VariableConditionNode))
                {
                    VariableConditionNode variableConditionNode = (VariableConditionNode)node;
                    prefix = "VC";
                    nodeText = !string.IsNullOrEmpty(variableConditionNode.VariableName)
                        ? $"If {variableConditionNode.VariableName}"
                        : "Empty";

                    if (nodeText.Length > 20)
                        nodeText = nodeText.Substring(0, 20) + "...";
                }
                else
                {
                    prefix = "?";
                    nodeText = "Unknown";
                }

                string menuItemName = $"{prefix}: {nodeText}";
                nodesMenu.AddItem(new GUIContent(menuItemName), false, () => CenterAndSelectNode(node));
            }

            Rect dropDownRect = new Rect(buttonRect.x, buttonRect.y + buttonRect.height, 150, 0);
            nodesMenu.DropDown(dropDownRect);
        }

        /// <summary>
        /// Centers the view on a specific node and selects it
        /// </summary>
        /// <param name="nodeToCenter">The node to center on and select</param>
        private void CenterAndSelectNode(Node nodeToCenter)
        {
            if (nodeToCenter == null)
                return;

            Vector2 windowCenter = new Vector2(position.width / 2, (position.height - ToolbarHeight) / 2);
            Vector2 offset = windowCenter - nodeToCenter.Rect.center;

            foreach (Node node in _currentNodeGraph.NodesList)
                node.DragNode(offset);

            foreach (Node node in _currentNodeGraph.NodesList)
                node.IsSelected = false;

            nodeToCenter.IsSelected = true;
            _currentNode = nodeToCenter;

            GUI.changed = true;
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

                    for (int i = 0; i < answerNode.ChildNodes.Count; i++)
                    {
                        if (answerNode.ChildNodes[i] != null)
                        {
                            parentNode = node;
                            childNode = answerNode.ChildNodes[i];

                            Color connectionColor = Color.white;

                            if (childNode is ModifyVariableNode)
                                connectionColor = new Color(0.8f, 0.6f, 0.2f);
                            else if (childNode is VariableConditionNode)
                                connectionColor = new Color(0.6f, 0.2f, 0.8f);
                            else if (childNode is ExternalFunctionNode)
                                connectionColor = new Color(0.2f, 0.8f, 0.6f);

                            DrawConnectionLine(parentNode, childNode, connectionColor);
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

                        Color connectionColor = Color.white;
                        if (childNode is ExternalFunctionNode)
                            connectionColor = new Color(0.2f, 0.8f, 0.6f);

                        DrawConnectionLine(parentNode, childNode, connectionColor);
                    }
                }
                else if (node.GetType() == typeof(ExternalFunctionNode))
                {
                    ExternalFunctionNode externalFunctionNode = (ExternalFunctionNode)node;

                    if (externalFunctionNode.ChildNode != null)
                    {
                        parentNode = node;
                        childNode = externalFunctionNode.ChildNode;

                        DrawConnectionLine(parentNode, childNode);
                    }
                }
                else if (node.GetType() == typeof(ModifyVariableNode))
                {
                    ModifyVariableNode modifyVariableNode = (ModifyVariableNode)node;

                    if (modifyVariableNode.ChildNode != null)
                    {
                        parentNode = node;
                        childNode = modifyVariableNode.ChildNode;

                        DrawConnectionLine(parentNode, childNode);
                    }
                }
                else if (node.GetType() == typeof(VariableConditionNode))
                {
                    VariableConditionNode variableConditionNode = (VariableConditionNode)node;

                    if (variableConditionNode.TrueChildNode != null)
                    {
                        parentNode = node;
                        childNode = variableConditionNode.TrueChildNode;

                        DrawConnectionLine(parentNode, childNode, new Color(0.2f, 0.8f, 0.2f), "T");
                    }

                    if (variableConditionNode.FalseChildNode != null)
                    {
                        parentNode = node;
                        childNode = variableConditionNode.FalseChildNode;

                        DrawConnectionLine(parentNode, childNode, new Color(0.8f, 0.2f, 0.2f), "F");
                    }
                }
            }
        }

        /// <summary>
        /// Draw connection line from parent to child node
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="childNode"></param>
        /// <param name="lineColor">Optional line color</param>
        /// <param name="label">Optional label for the connection</param>
        private void DrawConnectionLine(Node parentNode, Node childNode, Color? lineColor = null, string label = null)
        {
            Color color = lineColor ?? Color.white;

            Vector2 startPosition = parentNode.Rect.center;
            Vector2 endPosition = childNode.Rect.center;

            float distance = Vector2.Distance(startPosition, endPosition);

            Vector2 startTangent = startPosition + Vector2.right * (distance * 0.5f);
            Vector2 endTangent = endPosition + Vector2.left * (distance * 0.5f);

            Handles.DrawBezier(
                startPosition,
                endPosition,
                startTangent,
                endTangent,
                color,
                null,
                ConnectingLineWidth
            );

            Vector3[] bezierPoints = Handles.MakeBezierPoints(
                startPosition,
                endPosition,
                startTangent,
                endTangent,
                20
            );

            Vector2 midPosition = bezierPoints[bezierPoints.Length / 2];
            Vector2 direction = (endPosition - startPosition).normalized;

            if (!string.IsNullOrEmpty(label))
            {
                Handles.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                Handles.DrawSolidDisc(midPosition, Vector3.forward, 12f);

                Handles.color = Color.white;

                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.white;
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = 12;
                style.fontStyle = FontStyle.Bold;

                Handles.BeginGUI();
                GUI.Label(new Rect(midPosition.x - 10, midPosition.y - 10, 20, 20), label, style);
                Handles.EndGUI();
            }
            else if (parentNode is AnswerNode answerNode)
            {
                int index = answerNode.ChildNodes.IndexOf(childNode);

                if (index >= 0)
                {
                    string indexText = (index + 1).ToString();

                    Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                    if (childNode is ModifyVariableNode)
                        backgroundColor = new Color(0.8f, 0.6f, 0.2f, 0.8f);
                    else if (childNode is VariableConditionNode)
                        backgroundColor = new Color(0.6f, 0.2f, 0.8f, 0.8f);
                    else if (childNode is SentenceNode)
                        backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

                    Handles.color = backgroundColor;
                    Handles.DrawSolidDisc(midPosition, Vector3.forward, 12f);

                    Handles.color = Color.white;

                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.white;
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontSize = 12;
                    style.fontStyle = FontStyle.Bold;

                    Handles.BeginGUI();
                    GUI.Label(new Rect(midPosition.x - 10, midPosition.y - 10, 20, 20), indexText, style);
                    Handles.EndGUI();
                }
                else
                    DrawArrowAtMidpoint(midPosition, direction);
            }
            else
                DrawArrowAtMidpoint(midPosition, direction);

            GUI.changed = true;
        }

        /// <summary>
        /// Draw arrow at the midpoint of a connection line
        /// </summary>
        /// <param name="midPosition">Midpoint of the line</param>
        /// <param name="direction">Direction of the line</param>
        private void DrawArrowAtMidpoint(Vector2 midPosition, Vector2 direction)
        {
            Vector2 arrowTail1 =
                midPosition - new Vector2(-direction.y, direction.x).normalized * ConnectingLineArrowSize;
            Vector2 arrowTail2 =
                midPosition + new Vector2(-direction.y, direction.x).normalized * ConnectingLineArrowSize;
            Vector2 arrowHead = midPosition + direction * ConnectingLineArrowSize;

            Handles.DrawBezier(arrowHead, arrowTail1, arrowHead, arrowTail1, Color.white, null, ConnectingLineWidth);
            Handles.DrawBezier(arrowHead, arrowTail2, arrowHead, arrowTail2, Color.white, null, ConnectingLineWidth);
        }

        /// <summary>
        /// Draw grid background lines for node editor window
        /// </summary>
        /// <param name="gridSize"></param>
        /// <param name="gridOpacity"></param>
        /// <param name="color"></param>
        /// <param name="lineWidth"></param>
        private void DrawGridBackground(float gridSize, float gridOpacity, Color color, float lineWidth = 4f)
        {
            int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
            int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

            Color finalColor = new Color(color.r, color.g, color.b, gridOpacity);
            Handles.color = finalColor;

            _graphOffset += _graphDrag * 0.5f;
            Vector3 gridOffset = new Vector3(_graphOffset.x % gridSize, _graphOffset.y % gridSize, 0);

            Handles.BeginGUI();

            for (int i = 0; i < verticalLineCount; i++)
            {
                Vector3 p1 = new Vector3(gridSize * i, -gridSize, 0) + gridOffset;
                Vector3 p2 = new Vector3(gridSize * i, position.height + gridSize, 0) + gridOffset;

                Handles.DrawAAPolyLine(lineWidth, p1, p2);
            }

            for (int j = 0; j < horizontalLineCount; j++)
            {
                Vector3 p1 = new Vector3(-gridSize, gridSize * j, 0) + gridOffset;
                Vector3 p2 = new Vector3(position.width + gridSize, gridSize * j, 0f) + gridOffset;

                Handles.DrawAAPolyLine(lineWidth, p1, p2);
            }

            Handles.EndGUI();
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
                node.Draw(!node.IsSelected ? _nodeStyle : _selectedNodeStyle, _labelStyle);

            if (_isLeftMouseDragFromEmpty)
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
                ProcessMouseUpEvent(currentEvent);

            if (_currentNode == null || _currentNodeGraph.NodeToDrawLineFrom != null || currentEvent.button == 2)
                ProcessNodeEditorEvents(currentEvent);
            else
                _currentNode.ProcessNodeEvents(currentEvent);
        }

        /// <summary>
        /// Process mouse up event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessMouseUpEvent(Event currentEvent)
        {
            if (currentEvent.button == 0)
            {
                _currentNode = null;
                _isLeftMouseDragFromEmpty = false;
                _selectionRect = new Rect(0, 0, 0, 0);
            }
            else if (currentEvent.button == 1)
            {
                ProcessRightMouseUpEvent(currentEvent);
            }
            else if (currentEvent.button == 2)
            {
                ProcessMiddleMouseUpEvent(currentEvent);
            }
        }

        private void ProcessMiddleMouseUpEvent(Event currentEvent)
        {
            if (_currentNodeGraph.NodeToDrawLineFrom != null)
            {
                CheckLineConnection(currentEvent);
                ClearDraggedLine();
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
            else if (currentEvent.button == 0)
                ProcessLeftMouseDownEvent(currentEvent);
            else if (currentEvent.button == 2)
                ProcessMiddleMouseDownEvent(currentEvent);
        }

        /// <summary>
        /// Process middle mouse button down event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessMiddleMouseDownEvent(Event currentEvent)
        {
            Node node = GetHighlightedNode(currentEvent.mousePosition);

            if (node != null)
            {
                _nodeToDragLineFrom = node;
                _currentNodeGraph.SetNodeToDrawLineFromAndLinePosition(_nodeToDragLineFrom,
                    currentEvent.mousePosition);
            }

            _isMiddleMouseClickedOnNode = node != null;
            _mouseScrollClickPosition = currentEvent.mousePosition;
        }

        /// <summary>
        /// Process right mouse click event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessRightMouseDownEvent(Event currentEvent)
        {
            Node clickedNode = GetHighlightedNode(currentEvent.mousePosition);

            if (clickedNode != null)
            {
                foreach (Node node in _currentNodeGraph.NodesList)
                    node.IsSelected = false;

                clickedNode.IsSelected = true;
            }
        }

        /// <summary>
        /// Process left mouse click event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessLeftMouseDownEvent(Event currentEvent)
        {
            if (_isLeftMouseDragFromEmpty)
                return;

            Node clickedNode = GetHighlightedNode(currentEvent.mousePosition);

            if (clickedNode == null)
            {
                _currentNode = null;
                _mouseScrollClickPosition = currentEvent.mousePosition;
                _isLeftMouseDragFromEmpty = true;
            }
            else
            {
                SelectOnlyHighlightedNode(currentEvent.mousePosition);
                ProcessNodeSelection(currentEvent.mousePosition);
            }
        }

        /// <summary>
        /// Process right mouse up event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessRightMouseUpEvent(Event currentEvent) => ShowContextMenu(currentEvent.mousePosition);

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
            else if (currentEvent.button == 2)
                ProcessMiddleMouseDragEvent(currentEvent);
        }

        /// <summary>
        /// Process middle mouse drag event (graph dragging)
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessMiddleMouseDragEvent(Event currentEvent)
        {
            if (!_isMiddleMouseClickedOnNode)
            {
                _graphDrag = currentEvent.delta;

                foreach (var node in _currentNodeGraph.NodesList)
                    node.DragNode(_graphDrag);

                GUI.changed = true;
            }
            else
            {
                if (_currentNodeGraph.NodeToDrawLineFrom != null)
                {
                    DragConnectionLine(currentEvent.delta);
                    GUI.changed = true;
                }
            }
        }

        /// <summary>
        /// Process left mouse drag event
        /// </summary>
        /// <param name="currentEvent"></param>
        private void ProcessLeftMouseDragEvent(Event currentEvent)
        {
            if (_isLeftMouseDragFromEmpty)
            {
                GUI.changed = true;
                return;
            }

            Node node = GetHighlightedNode(currentEvent.mousePosition);

            if (node != null)
                node.DragNode(currentEvent.delta);
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
                    NodeConnectionHelper.CreateConnection(_currentNodeGraph.NodeToDrawLineFrom, node);
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
            if (!_isLeftMouseDragFromEmpty)
                return;

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
            contextMenu.AddItem(new GUIContent("Create External Function Node"), false, CreateExternalFunctionNode,
                mousePosition);
            contextMenu.AddItem(new GUIContent("Create Modify Variable Node"), false, CreateModifyVariableNode,
                mousePosition);
            contextMenu.AddItem(new GUIContent("Create Variable Condition Node"), false, CreateVariableConditionNode,
                mousePosition);
            contextMenu.AddSeparator("");
            contextMenu.AddItem(new GUIContent("Select All Nodes"), false, SelectAllNodes, mousePosition);
            contextMenu.AddItem(new GUIContent("Remove Selected Nodes"), false, RemoveSelectedNodes, mousePosition);
            contextMenu.AddItem(new GUIContent("Remove Connections"), false, RemoveAllConnections, mousePosition);
            contextMenu.ShowAsContext();
        }

        /// <summary>
        /// Create External Function Node at mouse position and add it to Node Graph asset
        /// </summary>
        /// <param name="mousePositionObject"></param>
        private void CreateExternalFunctionNode(object mousePositionObject)
        {
            ExternalFunctionNode externalFunctionNode = CreateInstance<ExternalFunctionNode>();
            InitializeNode(mousePositionObject, externalFunctionNode, "External Function Node");
        }

        /// <summary>
        /// Create Variable Condition Node at mouse position and add it to Node Graph asset
        /// </summary>
        /// <param name="mousePositionObject"></param>
        private void CreateVariableConditionNode(object mousePositionObject)
        {
            if (_currentNodeGraph != null)
                _currentNodeGraph.EnsureVariablesConfig();

            VariableConditionNode variableConditionNode = CreateInstance<VariableConditionNode>();
            InitializeNode(mousePositionObject, variableConditionNode, "Variable Condition Node");
        }

        /// <summary>
        /// Create Modify Variable Node at mouse position and add it to Node Graph asset
        /// </summary>
        /// <param name="mousePositionObject"></param>
        private void CreateModifyVariableNode(object mousePositionObject)
        {
            if (_currentNodeGraph != null)
                _currentNodeGraph.EnsureVariablesConfig();

            ModifyVariableNode modifyVariableNode = CreateInstance<ModifyVariableNode>();
            InitializeNode(mousePositionObject, modifyVariableNode, "Modify Variable Node");
        }

        /// <summary>
        /// Create Sentence Node at mouse position and add it to Node Graph asset
        /// </summary>
        /// <param name="mousePositionObject"></param>
        private void CreateSentenceNode(object mousePositionObject)
        {
            SentenceNode sentenceNode = CreateInstance<SentenceNode>();
            InitializeNode(mousePositionObject, sentenceNode, "Sentence Node");
        }

        /// <summary>
        /// Create Answer Node at mouse position and add it to Node Graph asset
        /// </summary>
        /// <param name="mousePositionObject"></param>
        private void CreateAnswerNode(object mousePositionObject)
        {
            AnswerNode answerNode = CreateInstance<AnswerNode>();
            InitializeNode(mousePositionObject, answerNode, "Answer Node");
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

        private void SelectOnlyHighlightedNode(Vector2 position)
        {
            Node highlightedNode = GetHighlightedNode(position);

            foreach (Node node in _currentNodeGraph.NodesList)
                node.IsSelected = false;

            highlightedNode.IsSelected = true;
            _currentNode = highlightedNode;
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
        private void InitializeNode(object mousePositionObject, Node node, string nodeName)
        {
            Vector2 mousePosition = (Vector2)mousePositionObject;

            _currentNodeGraph.NodesList.Add(node);

            node.Initialize(new Rect(mousePosition, new Vector2(NodeWidth, NodeHeight)), nodeName, _currentNodeGraph);

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
                if (!node.IsSelected)
                    continue;

                NodeConnectionHelper.RemoveAllConnectionsForNode(node);
            }

            GUI.changed = true;
        }

        /// <summary>
        /// Center the node editor window on all nodes
        /// </summary>
        private void CenterWindowOnNodes()
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