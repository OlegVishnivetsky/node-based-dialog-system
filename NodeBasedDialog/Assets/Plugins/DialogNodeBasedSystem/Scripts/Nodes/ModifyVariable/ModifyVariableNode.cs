using System.Linq;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace cherrydev
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Node Graph/Nodes/Modify Variable Node",
        fileName = "New Modify Variable Node")]
    public class ModifyVariableNode : Node
    {
        [SerializeField] private string _variableName = "";
        [SerializeField] private ModificationType _modifyType = ModificationType.Set;
        
        [SerializeField] private bool _boolValue;
        [SerializeField] private int _intValue;
        [SerializeField] private float _floatValue;
        [SerializeField] private string _stringValue = "";

        [Space(10)]
        public List<Node> ParentNodes = new();
        public Node ChildNode;

        public string VariableName => _variableName;
        public ModificationType Modification => _modifyType;

        /// <summary>
        /// Execute the variable modification
        /// </summary>
        public void ExecuteModification()
        {
            if (string.IsNullOrEmpty(_variableName))
            {
                Debug.LogWarning("Variable name is empty in ModifyVariableNode");
                return;
            }

            if (NodeGraph?.VariablesConfig == null)
            {
                Debug.LogWarning("No VariablesConfig found in DialogNodeGraph");
                return;
            }

            Variable variable = NodeGraph.VariablesConfig.GetVariable(_variableName);
            if (variable == null)
            {
                Debug.LogWarning($"Variable '{_variableName}' not found in VariablesConfig");
                return;
            }

            switch (variable.Type)
            {
                case VariableType.Bool:
                    ModifyBoolVariable(variable);
                    break;
                case VariableType.Int:
                    ModifyIntVariable(variable);
                    break;
                case VariableType.Float:
                    ModifyFloatVariable(variable);
                    break;
                case VariableType.String:
                    ModifyStringVariable(variable);
                    break;
            }
        }

        private void ModifyBoolVariable(Variable variable)
        {
            bool newValue = _modifyType == ModificationType.Toggle ? !variable.GetBoolValue() : _boolValue;
            variable.SetValue(newValue);
        }

        private void ModifyIntVariable(Variable variable)
        {
            int currentValue = variable.GetIntValue();
            int newValue = _modifyType switch
            {
                ModificationType.Set => _intValue,
                ModificationType.Increase => currentValue + _intValue,
                ModificationType.Decrease => currentValue - _intValue,
                _ => _intValue
            };
            variable.SetValue(newValue);
        }

        private void ModifyFloatVariable(Variable variable)
        {
            float currentValue = variable.GetFloatValue();
            float newValue = _modifyType switch
            {
                ModificationType.Set => _floatValue,
                ModificationType.Increase => currentValue + _floatValue,
                ModificationType.Decrease => currentValue - _floatValue,
                _ => _floatValue
            };
            variable.SetValue(newValue);
        }

        private void ModifyStringVariable(Variable variable)
        {
            string currentValue = variable.GetStringValue();
            string newValue = _modifyType switch
            {
                ModificationType.Set => _stringValue,
                ModificationType.Increase => currentValue + _stringValue,
                ModificationType.Decrease => currentValue.Replace(_stringValue, ""),
                _ => _stringValue
            };
            variable.SetValue(newValue);
        }

        private string GetModificationDescription()
        {
            return _modifyType switch
            {
                ModificationType.Set => $"Set to {GetCurrentModifyValueAsString()}",
                ModificationType.Increase => $"Increased by {GetCurrentModifyValueAsString()}",
                ModificationType.Decrease => $"Decreased by {GetCurrentModifyValueAsString()}",
                ModificationType.Toggle => "Toggled",
                _ => "Modified"
            };
        }

        private string GetCurrentModifyValueAsString()
        {
            if (NodeGraph?.VariablesConfig == null) 
                return "null";
            
            Variable variable = NodeGraph.VariablesConfig.GetVariable(_variableName);
            
            if (variable == null) 
                return "null";
            
            return variable.Type switch
            {
                VariableType.Bool => _boolValue.ToString(),
                VariableType.Int => _intValue.ToString(),
                VariableType.Float => _floatValue.ToString(),
                VariableType.String => $"\"{_stringValue}\"",
                _ => "unknown"
            };
        }

#if UNITY_EDITOR
        private const float LabelWidth = 80f;
        private const float FieldWidth = 90f;
        private const float NodeWidth = 210f;
        private const float NodeHeight = 155f;
        private const float ToggleModificationHeight = 120f;

        public override void Draw(GUIStyle nodeStyle, GUIStyle labelStyle)
        {
            base.Draw(nodeStyle, labelStyle);

            // Clean up null parent references
            ParentNodes.RemoveAll(item => item == null);

            float currentHeight = _modifyType == ModificationType.Toggle 
                ? ToggleModificationHeight 
                : NodeHeight;
            
            Rect.size = new Vector2(NodeWidth, currentHeight);

            GUILayout.BeginArea(Rect, nodeStyle);
            
            EditorGUILayout.LabelField("Modify Variable", labelStyle);
            EditorGUILayout.Space(3);
            
            if (NodeGraph != null)
                NodeGraph.EnsureVariablesConfig();
            
            DrawVariableSelection();
            DrawModifyTypeSelection();
            DrawValueField();
            DrawPreview();

            GUILayout.EndArea();
        }

        /// <summary>
        /// Removes all connection in a modify variable node
        /// </summary>
        public override void RemoveAllConnections()
        {
            ParentNodes.Clear();
            ChildNode = null;
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
                
                int newIndex = EditorGUILayout.Popup(currentIndex, variableNames, GUILayout.Width(FieldWidth));
                
                if (newIndex >= 0 && newIndex < variableNames.Length)
                    _variableName = variableNames[newIndex];
            }
            else
            {
                _variableName = EditorGUILayout.TextField(_variableName, GUILayout.Width(FieldWidth));
                
                if (NodeGraph?.VariablesConfig != null && NodeGraph.VariablesConfig.Variables.Count == 0)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.HelpBox("No variables found. Create variables in the VariablesConfig.", MessageType.Info);
                    return;
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawModifyTypeSelection()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Action:", GUILayout.Width(LabelWidth));
            Variable selectedVariable = NodeGraph?.VariablesConfig?.GetVariable(_variableName);
            
            if (selectedVariable != null)
            {
                ModificationType[] availableTypes = GetAvailableModifyTypes(selectedVariable.Type);
                string[] typeNames = availableTypes.Select(t => GetModifyTypeDisplayName(t, selectedVariable.Type)).ToArray();
                int currentIndex = System.Array.IndexOf(availableTypes, _modifyType);
                
                if (currentIndex == -1) 
                    currentIndex = 0;
                
                int newIndex = EditorGUILayout.Popup(currentIndex, typeNames, GUILayout.Width(FieldWidth));
                
                if (newIndex >= 0 && newIndex < availableTypes.Length)
                    _modifyType = availableTypes[newIndex];
            }
            else
                _modifyType = (ModificationType)EditorGUILayout.EnumPopup(_modifyType, GUILayout.Width(FieldWidth));
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawValueField()
        {
            Variable selectedVariable = NodeGraph?.VariablesConfig?.GetVariable(_variableName);
            
            if (selectedVariable == null || _modifyType == ModificationType.Toggle)
                return;
            
            EditorGUILayout.BeginHorizontal();
            
            string label = _modifyType switch
            {
                ModificationType.Set => "Set to:",
                ModificationType.Increase => selectedVariable.Type == VariableType.String ? "Append:" : "Add:",
                ModificationType.Decrease => selectedVariable.Type == VariableType.String ? "Remove:" : "Subtract:",
                _ => "Value:"
            };
            
            EditorGUILayout.LabelField(label, GUILayout.Width(LabelWidth));
            
            switch (selectedVariable.Type)
            {
                case VariableType.Bool:
                    _boolValue = EditorGUILayout.Toggle(_boolValue, GUILayout.Width(FieldWidth));
                    break;
                case VariableType.Int:
                    _intValue = EditorGUILayout.IntField(_intValue, GUILayout.Width(FieldWidth));
                    break;
                case VariableType.Float:
                    _floatValue = EditorGUILayout.FloatField(_floatValue, GUILayout.Width(FieldWidth));
                    break;
                case VariableType.String:
                    _stringValue = EditorGUILayout.TextField(_stringValue, GUILayout.Width(FieldWidth));
                    break;
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPreview()
        {
            if (string.IsNullOrEmpty(_variableName))
                return;
                
            Variable selectedVariable = NodeGraph?.VariablesConfig?.GetVariable(_variableName);
            
            if (selectedVariable == null)
            {
                EditorGUILayout.HelpBox($"Variable '{_variableName}' not found", MessageType.Warning);
                return;
            }
            
            string previewText = GetPreviewText(selectedVariable);
            EditorGUILayout.LabelField(previewText, EditorStyles.helpBox);
        }

        private string GetPreviewText(Variable variable)
        {
            string action = _modifyType switch
            {
                ModificationType.Set => $"Set {variable.Name} = {GetCurrentModifyValueAsString()}",
                ModificationType.Increase => variable.Type == VariableType.String 
                    ? $"Append {GetCurrentModifyValueAsString()} to {variable.Name}"
                    : $"Add {GetCurrentModifyValueAsString()} to {variable.Name}",
                ModificationType.Decrease => variable.Type == VariableType.String 
                    ? $"Remove {GetCurrentModifyValueAsString()} from {variable.Name}"
                    : $"Subtract {GetCurrentModifyValueAsString()} from {variable.Name}",
                ModificationType.Toggle => $"Toggle {variable.Name}",
                _ => $"Modify {variable.Name}"
            };
            
            string persistentInfo = variable.SaveToPrefs ? " (Saved)" : "";
            return action + persistentInfo;
        }

        private ModificationType[] GetAvailableModifyTypes(VariableType variableType)
        {
            return variableType switch
            {
                VariableType.Bool => new[] { ModificationType.Set, ModificationType.Toggle },
                VariableType.Int or VariableType.Float => new[] { ModificationType.Set, ModificationType.Increase, ModificationType.Decrease },
                VariableType.String => new[] { ModificationType.Set, ModificationType.Increase, ModificationType.Decrease },
                _ => new[] { ModificationType.Set }
            };
        }

        private string GetModifyTypeDisplayName(ModificationType modifyType, VariableType variableType)
        {
            return modifyType switch
            {
                ModificationType.Set => "Set",
                ModificationType.Increase => variableType == VariableType.String ? "Append" : "Increase",
                ModificationType.Decrease => variableType == VariableType.String ? "Remove" : "Decrease",
                ModificationType.Toggle => "Toggle",
                _ => modifyType.ToString()
            };
        }

        public override bool AddToChildConnectedNode(Node nodeToAdd)
        {
            if (nodeToAdd == this)
                return false;

            if (ChildNode != null && ChildNode != nodeToAdd)
                ChildNode.RemoveFromParentConnectedNode(this);

            ChildNode = nodeToAdd;
    
            return true;
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
        
        public override bool RemoveFromParentConnectedNode(Node nodeToRemove) => ParentNodes.Remove(nodeToRemove);
#endif
    }
}