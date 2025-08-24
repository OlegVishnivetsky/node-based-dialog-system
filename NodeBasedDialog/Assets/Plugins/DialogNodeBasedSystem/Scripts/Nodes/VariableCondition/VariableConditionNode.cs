using System.Linq;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace cherrydev
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Node Graph/Nodes/Variable Condition Node", fileName = "New Variable Condition Node")]
    public class VariableConditionNode : Node
    {
        [SerializeField] private string _variableName = "";
        [SerializeField] private ConditionType _conditionType = ConditionType.Equal;
        
        [SerializeField] private bool _boolTargetValue;
        [SerializeField] private int _intTargetValue;
        [SerializeField] private float _floatTargetValue;
        [SerializeField] private string _stringTargetValue = "";

        [Space(10)]
        public List<Node> ParentNodes = new(); // Changed from single Node to List<Node>
        public Node TrueChildNode;
        public Node FalseChildNode;

        public string VariableName => _variableName;
        public ConditionType Condition => _conditionType;

        /// <summary>
        /// Evaluate the condition using the variables handler
        /// </summary>
        /// <param name="variablesHandler">The variables handler to check values</param>
        /// <returns>True if condition is met, false otherwise</returns>
        public bool EvaluateCondition(DialogVariablesHandler variablesHandler)
        {
            if (string.IsNullOrEmpty(_variableName))
            {
                Debug.LogWarning("Variable name is empty in VariableConditionNode");
                return false;
            }

            if (variablesHandler == null)
            {
                Debug.LogWarning("Variables handler is null");
                return false;
            }

            Variable variable = variablesHandler.GetVariable(_variableName);
            if (variable == null)
            {
                Debug.LogWarning($"Variable '{_variableName}' not found");
                return false;
            }

            return EvaluateConditionForType(variable);
        }

        private bool EvaluateConditionForType(Variable variable)
        {
            switch (variable.Type)
            {
                case VariableType.Bool:
                    return EvaluateBoolCondition(variable.GetBoolValue());
                    
                case VariableType.Int:
                    return EvaluateIntCondition(variable.GetIntValue());
                    
                case VariableType.Float:
                    return EvaluateFloatCondition(variable.GetFloatValue());
                    
                case VariableType.String:
                    return EvaluateStringCondition(variable.GetStringValue());
                    
                default:
                    Debug.LogWarning($"Unsupported variable type: {variable.Type}");
                    return false;
            }
        }

        private bool EvaluateBoolCondition(bool variableValue)
        {
            return _conditionType switch
            {
                ConditionType.Equal => variableValue == _boolTargetValue,
                ConditionType.NotEqual => variableValue != _boolTargetValue,
                _ => false
            };
        }

        private bool EvaluateIntCondition(int variableValue)
        {
            return _conditionType switch
            {
                ConditionType.Equal => variableValue == _intTargetValue,
                ConditionType.NotEqual => variableValue != _intTargetValue,
                ConditionType.Greater => variableValue > _intTargetValue,
                ConditionType.GreaterOrEqual => variableValue >= _intTargetValue,
                ConditionType.Less => variableValue < _intTargetValue,
                ConditionType.LessOrEqual => variableValue <= _intTargetValue,
                _ => false
            };
        }

        private bool EvaluateFloatCondition(float variableValue)
        {
            return _conditionType switch
            {
                ConditionType.Equal => Mathf.Approximately(variableValue, _floatTargetValue),
                ConditionType.NotEqual => !Mathf.Approximately(variableValue, _floatTargetValue),
                ConditionType.Greater => variableValue > _floatTargetValue,
                ConditionType.GreaterOrEqual => variableValue >= _floatTargetValue,
                ConditionType.Less => variableValue < _floatTargetValue,
                ConditionType.LessOrEqual => variableValue <= _floatTargetValue,
                _ => false
            };
        }

        private bool EvaluateStringCondition(string variableValue)
        {
            return _conditionType switch
            {
                ConditionType.Equal => variableValue.Equals(_stringTargetValue, System.StringComparison.Ordinal),
                ConditionType.NotEqual => !variableValue.Equals(_stringTargetValue, System.StringComparison.Ordinal),
                ConditionType.Greater => string.Compare(variableValue, _stringTargetValue, System.StringComparison.Ordinal) > 0,
                ConditionType.GreaterOrEqual => string.Compare(variableValue, _stringTargetValue, System.StringComparison.Ordinal) >= 0,
                ConditionType.Less => string.Compare(variableValue, _stringTargetValue, System.StringComparison.Ordinal) < 0,
                ConditionType.LessOrEqual => string.Compare(variableValue, _stringTargetValue, System.StringComparison.Ordinal) <= 0,
                _ => false
            };
        }

        private string GetConditionDescription()
        {
            if (string.IsNullOrEmpty(_variableName))
                return "No variable selected";

            if (NodeGraph?.VariablesConfig == null)
                return "No variables config";

            Variable variable = NodeGraph.VariablesConfig.GetVariable(_variableName);
            if (variable == null)
                return $"Variable '{_variableName}' not found";

            string conditionSymbol = _conditionType switch
            {
                ConditionType.Equal => "==",
                ConditionType.NotEqual => "!=",
                ConditionType.Greater => ">",
                ConditionType.GreaterOrEqual => ">=",
                ConditionType.Less => "<",
                ConditionType.LessOrEqual => "<=",
                _ => "?"
            };

            string targetValue = variable.Type switch
            {
                VariableType.Bool => _boolTargetValue.ToString(),
                VariableType.Int => _intTargetValue.ToString(),
                VariableType.Float => _floatTargetValue.ToString("F2"),
                VariableType.String => $"\"{_stringTargetValue}\"",
                _ => "?"
            };

            return $"{_variableName} {conditionSymbol} {targetValue}";
        }

#if UNITY_EDITOR
        private const float LabelWidth = 80f;
        private const float FieldWidth = 90f;
        private const float NodeWidth = 240f;
        private const float NodeHeight = 160f;

        public override void Draw(GUIStyle nodeStyle, GUIStyle labelStyle)
        {
            base.Draw(nodeStyle, labelStyle);

            // Clean up null parent references
            ParentNodes.RemoveAll(item => item == null);

            Rect.size = new Vector2(NodeWidth, NodeHeight);

            GUILayout.BeginArea(Rect, nodeStyle);
            
            EditorGUILayout.LabelField("Variable Condition", labelStyle);
            EditorGUILayout.Space(3);
            
            if (NodeGraph != null)
                NodeGraph.EnsureVariablesConfig();
            
            DrawVariableSelection();
            DrawConditionSelection();
            DrawTargetValueField();
            
            EditorGUILayout.Space(3);
            DrawConditionPreview();

            GUILayout.EndArea();
        }

        /// <summary>
        /// Removes all connection in a variable condition node
        /// </summary>
        public override void RemoveAllConnections()
        {
            ParentNodes.Clear(); // Clear all parent connections
            TrueChildNode = null;
            FalseChildNode = null;
        }

        private void DrawVariableSelection()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Variable:", GUILayout.Width(LabelWidth));
            
            if (NodeGraph?.VariablesConfig != null && NodeGraph.VariablesConfig.Variables.Count > 0)
            {
                string[] variableNames = NodeGraph.VariablesConfig.Variables
                    .Where(v => v != null)
                    .Select(v => v.Name)
                    .ToArray();
                
                int currentIndex = System.Array.IndexOf(variableNames, _variableName);
                
                if (currentIndex == -1) 
                    currentIndex = 0;
                
                int newIndex = EditorGUILayout.Popup(currentIndex, variableNames, GUILayout.Width(FieldWidth + 20));
                
                if (newIndex >= 0 && newIndex < variableNames.Length)
                    _variableName = variableNames[newIndex];
            }
            else
            {
                _variableName = EditorGUILayout.TextField(_variableName, GUILayout.Width(FieldWidth + 20));
                
                if (NodeGraph?.VariablesConfig != null && NodeGraph.VariablesConfig.Variables.Count == 0)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.HelpBox("No variables found. Create variables in the VariablesConfig.", MessageType.Info);
                    return;
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawConditionSelection()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Condition:", GUILayout.Width(LabelWidth));
            
            Variable selectedVariable = NodeGraph?.VariablesConfig?.GetVariable(_variableName);
            
            if (selectedVariable != null)
            {
                ConditionType[] availableConditions = GetAvailableConditions(selectedVariable.Type);
                string[] conditionNames = availableConditions.Select(GetConditionDisplayName).ToArray();
                
                int currentIndex = System.Array.IndexOf(availableConditions, _conditionType);
                if (currentIndex == -1) currentIndex = 0;
                
                int newIndex = EditorGUILayout.Popup(currentIndex, conditionNames, GUILayout.Width(FieldWidth + 20));
                if (newIndex >= 0 && newIndex < availableConditions.Length)
                {
                    _conditionType = availableConditions[newIndex];
                }
            }
            else
                _conditionType = (ConditionType)EditorGUILayout.EnumPopup(_conditionType, GUILayout.Width(FieldWidth + 20));
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTargetValueField()
        {
            Variable selectedVariable = NodeGraph?.VariablesConfig?.GetVariable(_variableName);
            if (selectedVariable == null)
                return;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Target Value:", GUILayout.Width(LabelWidth));
            
            switch (selectedVariable.Type)
            {
                case VariableType.Bool:
                    _boolTargetValue = EditorGUILayout.Toggle(_boolTargetValue, GUILayout.Width(FieldWidth + 20));
                    break;
                case VariableType.Int:
                    _intTargetValue = EditorGUILayout.IntField(_intTargetValue, GUILayout.Width(FieldWidth + 20));
                    break;
                case VariableType.Float:
                    _floatTargetValue = EditorGUILayout.FloatField(_floatTargetValue, GUILayout.Width(FieldWidth + 20));
                    break;
                case VariableType.String:
                    _stringTargetValue = EditorGUILayout.TextField(_stringTargetValue, GUILayout.Width(FieldWidth + 20));
                    break;
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawConditionPreview()
        {
            string conditionText = GetConditionDescription();
            EditorGUILayout.LabelField(conditionText, EditorStyles.helpBox);
        }

        private ConditionType[] GetAvailableConditions(VariableType variableType)
        {
            return variableType switch
            {
                VariableType.Bool => new[] { ConditionType.Equal, ConditionType.NotEqual },
                VariableType.Int or VariableType.Float or VariableType.String => System.Enum.GetValues(typeof(ConditionType)).Cast<ConditionType>().ToArray(),
                _ => new[] { ConditionType.Equal }
            };
        }

        private string GetConditionDisplayName(ConditionType conditionType)
        {
            return conditionType switch
            {
                ConditionType.Equal => "Equal (==)",
                ConditionType.NotEqual => "Not Equal (!=)",
                ConditionType.Greater => "Greater (>)",
                ConditionType.GreaterOrEqual => "Greater or Equal (>=)",
                ConditionType.Less => "Less (<)",
                ConditionType.LessOrEqual => "Less or Equal (<=)",
                _ => conditionType.ToString()
            };
        }

        public override bool AddToChildConnectedNode(Node nodeToAdd)
        {
            if (nodeToAdd == this) 
                return false;

            if (TrueChildNode == null)
            {
                TrueChildNode = nodeToAdd;
                return true;
            }

            if (FalseChildNode == null)
            {
                FalseChildNode = nodeToAdd;
                return true;
            }

            return false;
        }

        public override bool AddToParentConnectedNode(Node nodeToAdd)
        {
            if (nodeToAdd == this) 
                return false;
            
            if (ParentNodes.Contains(nodeToAdd))
                return false;
            
            ParentNodes.Add(nodeToAdd);
            return true;
        }

        /// <summary>
        /// Remove a parent node connection
        /// </summary>
        /// <param name="nodeToRemove"></param>
        /// <returns></returns>
        public override bool RemoveFromParentConnectedNode(Node nodeToRemove) => ParentNodes.Remove(nodeToRemove);
#endif
    }
}