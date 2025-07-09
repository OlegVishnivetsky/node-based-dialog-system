using System;
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
            }
            
            PlayerPrefs.Save();
        }
    }

    [Serializable]
    public class Variable
    {
        [SerializeField] private string _name;
        [SerializeField] private VariableType _type;
        [SerializeField] private bool _saveToPrefs;
        
        [Header("Value")]
        [SerializeField] private bool _boolValue;
        [SerializeField] private int _intValue;
        [SerializeField] private float _floatValue;
        [SerializeField] private string _stringValue;
        
        public string Name => _name;
        public VariableType Type => _type;
        public bool SaveToPrefs => _saveToPrefs;

        public Variable(string name, VariableType type, bool saveToPrefs)
        {
            _name = name;
            _type = type;
            _saveToPrefs = saveToPrefs;
        }

        public T GetValue<T>()
        {
            object value = GetValue();
            return (T)Convert.ChangeType(value, typeof(T));
        }
        
        public object GetValue()
        {
            switch (_type)
            {
                case VariableType.Bool:
                    return _boolValue;
                case VariableType.Int:
                    return _intValue;
                case VariableType.Float:
                    return _floatValue;
            }

            return null;
        }

        public string GetValueAsString() => GetValue()?.ToString() ?? "";

        public void SetName(string name) => _name = name;
        
        public void SetType(VariableType type) => _type = type;
        
        public void SetSaveToPrefs(bool saveToPrefs) => _saveToPrefs = saveToPrefs;
        
        public bool GetBoolValue() => _boolValue;
        
        public int GetIntValue() => _intValue;
        
        public float GetFloatValue() => _floatValue;
        public string GetStringValue() => _stringValue;
        
        public void SetValue(bool value) => _boolValue = value;
        
        public void SetValue(int value) => _intValue = value;
        
        public void SetValue(float value) => _floatValue = value;
        
        public void SetValue(object value)
        {
            switch (_type)
            {
                case VariableType.Bool:
                    _boolValue = Convert.ToBoolean(value);
                    break;
                case VariableType.Int:
                    _intValue = Convert.ToInt32(value);
                    break;
                case VariableType.Float:
                    _floatValue = Convert.ToSingle(value);
                    break;
            }
        }
    }
}