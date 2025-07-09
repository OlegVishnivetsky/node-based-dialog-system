using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace cherrydev
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Variables Config", fileName = "Variables Config")]
    public class VariablesConfig : ScriptableObject
    {
        [SerializeField] private List<Variable> _variables = new();
        
        public List<Variable> Variables => _variables;

        public void Initialize()
        {
            foreach (Variable variable in _variables)
            {
                if (!variable.SaveToPrefs)
                    continue;

                string key = $"V_{variable.Name}";
        
                if (!PlayerPrefs.HasKey(key))
                    continue;

                switch (variable.Type)
                {
                    case VariableType.Bool:
                        variable.SetValue(PlayerPrefs.GetInt(key) == 1);
                        break;
                    case VariableType.Int:
                        variable.SetValue(PlayerPrefs.GetInt(key));
                        break;
                    case VariableType.Float:
                        variable.SetValue(PlayerPrefs.GetFloat(key));
                        break;
                    case VariableType.String:
                        variable.SetValue(PlayerPrefs.GetString(key));
                        break;
                }
            }
        }

        public void Save()
        {
            foreach (Variable variable in _variables)
            {
                if (!variable.SaveToPrefs)
                    return;
                
                SaveVariable(variable);
            }
        }

        public Variable GetVariable(string variableName) => 
            _variables.FirstOrDefault(v => v.Name == variableName);

        public void AddVariable(Variable variable)
        {
            if (!string.IsNullOrEmpty(variable.Name) && !HasVariable(variable.Name))
                _variables.Add(variable);
        }

        public void RemoveVariable(string variableName) => 
            _variables.RemoveAll(v => v.Name == variableName);

        public bool HasVariable(string variableName) => 
            _variables.Any(v => v.Name == variableName);

        public void SaveVariable(Variable variable)
        {
            if (!variable.SaveToPrefs)
                return;

            string key = $"V_{variable.Name}";
    
            switch (variable.Type)
            {
                case VariableType.Bool:
                    PlayerPrefs.SetInt(key, variable.GetValue<bool>() ? 1 : 0);
                    break;
                case VariableType.Int:
                    PlayerPrefs.SetInt(key, variable.GetValue<int>());
                    break;
                case VariableType.Float:
                    PlayerPrefs.SetFloat(key, variable.GetValue<float>());
                    break;
                case VariableType.String:
                    PlayerPrefs.SetString(key, variable.GetValue<string>());
                    break;
            }
    
            PlayerPrefs.Save();
        }
    }
}