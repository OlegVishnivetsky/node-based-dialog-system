using System;
using UnityEngine;

namespace cherrydev
{
    /// <summary>
    /// Non-MonoBehaviour class to handle variables in dialog system
    /// </summary>
    public class DialogVariablesHandler
    {
        private VariablesConfig _variablesConfig;
        
        public event Action<string> VariableChanged;
        public event Action<ModifyVariableNode> VariableModified;

        public VariablesConfig VariablesConfig => _variablesConfig;

        public DialogVariablesHandler(VariablesConfig variablesConfig)
        {
            _variablesConfig = variablesConfig;
            LoadVariables();
        }

        /// <summary>
        /// Load all persistent variables from PlayerPrefs
        /// </summary>
        public void LoadVariables()
        {
            if (_variablesConfig == null)
                return;

            _variablesConfig.Initialize();
        }

        /// <summary>
        /// Execute a modify variable node
        /// </summary>
        /// <param name="modifyNode">The modify variable node to execute</param>
        public void ExecuteModifyVariableNode(ModifyVariableNode modifyNode)
        {
            if (modifyNode == null)
            {
                Debug.LogWarning("ModifyVariableNode is null");
                return;
            }

            if (_variablesConfig == null)
            {
                Debug.LogWarning("VariablesConfig is null");
                return;
            }

            if (string.IsNullOrEmpty(modifyNode.VariableName))
            {
                Debug.LogWarning("Variable name is empty in ModifyVariableNode");
                return;
            }

            Variable variable = _variablesConfig.GetVariable(modifyNode.VariableName);
            
            if (variable == null)
            {
                Debug.LogWarning($"Variable '{modifyNode.VariableName}' not found");
                return;
            }

            modifyNode.ExecuteModification();
            
            VariableChanged?.Invoke(modifyNode.VariableName);
            VariableModified?.Invoke(modifyNode);
        }

        /// <summary>
        /// Get variable value by name
        /// </summary>
        /// <typeparam name="T">Type of the variable</typeparam>
        /// <param name="variableName">Name of the variable</param>
        /// <returns>Variable value</returns>
        public T GetVariableValue<T>(string variableName)
        {
            if (_variablesConfig == null)
            {
                Debug.LogWarning("VariablesConfig is null");
                return default!;
            }

            Variable variable = _variablesConfig.GetVariable(variableName);
            
            if (variable == null)
            {
                Debug.LogWarning($"Variable '{variableName}' not found");
                return default!;
            }

            return variable.GetValue<T>();
        }

        /// <summary>
        /// Set variable value by name
        /// </summary>
        /// <param name="variableName">Name of the variable</param>
        /// <param name="value">Value to set</param>
        public void SetVariableValue(string variableName, object value)
        {
            if (_variablesConfig == null)
            {
                Debug.LogWarning("VariablesConfig is null");
                return;
            }

            Variable variable = _variablesConfig.GetVariable(variableName);
            
            if (variable == null)
            {
                Debug.LogWarning($"Variable '{variableName}' not found");
                return;
            }

            variable.SetValue(value);
            
            if (variable.SaveToPrefs)
                _variablesConfig.SaveVariable(variable);
            
            VariableChanged?.Invoke(variableName);
        }

        /// <summary>
        /// Set variable value directly without boxing (for performance)
        /// </summary>
        public void SetVariableValueDirect(string variableName, bool value)
        {
            Variable variable = _variablesConfig?.GetVariable(variableName);
            
            if (variable?.Type == VariableType.Bool)
            {
                variable.SetValue(value);
                
                if (variable.SaveToPrefs)
                    _variablesConfig.SaveVariable(variable);
                
                VariableChanged?.Invoke(variableName);
            }
        }

        public void SetVariableValueDirect(string variableName, int value)
        {
            Variable variable = _variablesConfig?.GetVariable(variableName);
            
            if (variable?.Type == VariableType.Int)
            {
                variable.SetValue(value);
                
                if (variable.SaveToPrefs)
                    _variablesConfig.SaveVariable(variable);
                
                VariableChanged?.Invoke(variableName);
            }
        }

        public void SetVariableValueDirect(string variableName, float value)
        {
            Variable variable = _variablesConfig?.GetVariable(variableName);
            
            if (variable?.Type == VariableType.Float)
            {
                variable.SetValue(value);
                
                if (variable.SaveToPrefs)
                    _variablesConfig.SaveVariable(variable);
                
                VariableChanged?.Invoke(variableName);
            }
        }

        public void SetVariableValueDirect(string variableName, string value)
        {
            Variable variable = _variablesConfig?.GetVariable(variableName);
            
            if (variable?.Type == VariableType.String)
            {
                variable.SetValue(value);
                
                if (variable.SaveToPrefs)
                    _variablesConfig.SaveVariable(variable);
                
                VariableChanged?.Invoke(variableName);
            }
        }

        /// <summary>
        /// Check if a variable exists
        /// </summary>
        /// <param name="variableName">Name of the variable</param>
        /// <returns>True if variable exists</returns>
        public bool HasVariable(string variableName) => _variablesConfig?.HasVariable(variableName) ?? false;

        /// <summary>
        /// Get variable by name
        /// </summary>
        /// <param name="variableName">Name of the variable</param>
        /// <returns>Variable or null if not found</returns>
        public Variable GetVariable(string variableName) => _variablesConfig?.GetVariable(variableName);

        /// <summary>
        /// Save all persistent variables to PlayerPrefs
        /// </summary>
        public void SaveAllPersistentVariables()
        {
            if (_variablesConfig == null)
                return;

            _variablesConfig.Save();
        }

        /// <summary>
        /// Reset all variables to their default values
        /// </summary>
        public void ResetAllVariables()
        {
            if (_variablesConfig == null)
                return;

            foreach (Variable variable in _variablesConfig.Variables)
            {
                if (variable == null)
                    continue;

                switch (variable.Type)
                {
                    case VariableType.Bool:
                        variable.SetValue(false);
                        break;
                    case VariableType.Int:
                        variable.SetValue(0);
                        break;
                    case VariableType.Float:
                        variable.SetValue(0f);
                        break;
                    case VariableType.String:
                        variable.SetValue("");
                        break;
                }

                if (variable.SaveToPrefs)
                    _variablesConfig.SaveVariable(variable);

                VariableChanged?.Invoke(variable.Name);
            }

            Debug.Log("All variables reset to defaults");
        }

        /// <summary>
        /// Update the variables config (useful when dialog graph changes)
        /// </summary>
        /// <param name="newVariablesConfig">New variables config</param>
        public void UpdateVariablesConfig(VariablesConfig newVariablesConfig)
        {
            _variablesConfig = newVariablesConfig;
            
            if (_variablesConfig != null)
                LoadVariables();
        }
    }
}