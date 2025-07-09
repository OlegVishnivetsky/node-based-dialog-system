using UnityEngine;
using UnityEditor;

namespace cherrydev
{
    [CustomEditor(typeof(VariablesConfig))]
    public class VariablesConfigEditor : Editor
    {
        private Vector2 _scrollPosition;
        private string _newVariableName = "";
        private VariableType _newVariableType = VariableType.Bool;
        private bool _newVariableSaveToPrefs;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            VariablesConfig config = (VariablesConfig)target;

            EditorGUILayout.LabelField("Variables Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            DrawAddVariableSection(config);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Variables ({config.Variables.Count})", EditorStyles.boldLabel);

            if (config.Variables.Count > 0)
            {
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MaxHeight(400));
                
                for (int i = config.Variables.Count - 1; i >= 0; i--)
                {
                    if (config.Variables[i] == null)
                    {
                        config.Variables.RemoveAt(i);
                        continue;
                    }
                    
                    DrawVariableElement(config, i);
                }
                
                EditorGUILayout.EndScrollView();
            }
            else
                EditorGUILayout.HelpBox("No variables defined. Add some variables using the section above.", MessageType.Info);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(config);
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawAddVariableSection(VariablesConfig config)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Add New Variable", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name:", GUILayout.Width(50));
            _newVariableName = EditorGUILayout.TextField(_newVariableName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type:", GUILayout.Width(50));
            _newVariableType = (VariableType)EditorGUILayout.EnumPopup(_newVariableType);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Save to PlayerPrefs:", GUILayout.Width(120));
            _newVariableSaveToPrefs = EditorGUILayout.Toggle(_newVariableSaveToPrefs);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUI.enabled = !string.IsNullOrEmpty(_newVariableName) && !config.HasVariable(_newVariableName);
            
            if (GUILayout.Button("Add Variable", GUILayout.Width(100)))
            {
                Variable newVariable = new Variable(_newVariableName, _newVariableType, _newVariableSaveToPrefs);
                config.AddVariable(newVariable);
                _newVariableName = "";
                _newVariableSaveToPrefs = false;
                GUI.FocusControl(null);
            }
            
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_newVariableName) && config.HasVariable(_newVariableName))
                EditorGUILayout.HelpBox($"Variable '{_newVariableName}' already exists!", MessageType.Warning);
            
            EditorGUILayout.EndVertical();
        }

        private void DrawVariableElement(VariablesConfig config, int index)
        {
            Variable variable = config.Variables[index];
            
            if (variable == null)
                return;
            
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{variable.Name}", EditorStyles.boldLabel, GUILayout.Width(120));
            EditorGUILayout.LabelField($"({variable.Type})", GUILayout.Width(60));
            
            bool newSaveToPrefs = EditorGUILayout.Toggle("Save", variable.SaveToPrefs, GUILayout.Width(60));
            
            if (newSaveToPrefs != variable.SaveToPrefs)
                variable.SetSaveToPrefs(newSaveToPrefs);
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(20)))
            {
                if (EditorUtility.DisplayDialog("Delete Variable", 
                    $"Are you sure you want to delete variable '{variable.Name}'?", 
                    "Delete", "Cancel"))
                {
                    config.RemoveVariable(variable.Name);
                    return;
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (variable.SaveToPrefs)
                EditorGUILayout.HelpBox($"PlayerPrefs key: V_{variable.Name}", MessageType.Info);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Value:", GUILayout.Width(50));
            
            object currentValue = variable.GetValue();
            object newValue = DrawValueFieldForType(currentValue, variable.Type);
            
            if (newValue != null && !newValue.Equals(currentValue))
                variable.SetValue(newValue);
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private object DrawValueFieldForType(object currentValue, VariableType type)
        {
            switch (type)
            {
                case VariableType.Bool:
                    return EditorGUILayout.Toggle((bool)currentValue);
                    
                case VariableType.Int:
                    return EditorGUILayout.IntField((int)currentValue);
                    
                case VariableType.Float:
                    return EditorGUILayout.FloatField((float)currentValue);
                    
                case VariableType.String:
                    return EditorGUILayout.TextField((string)currentValue ?? "");
                    
                default:
                    EditorGUILayout.LabelField("Unknown type");
                    return currentValue;
            }
        }
    }
}