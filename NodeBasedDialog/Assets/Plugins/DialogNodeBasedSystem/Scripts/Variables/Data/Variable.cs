using System;
using UnityEngine;

namespace cherrydev
{
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
                case VariableType.String:
                    return _stringValue ?? string.Empty;
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
        public string GetStringValue() => _stringValue ?? string.Empty;
        
        public void SetValue(bool value) => _boolValue = value;
        
        public void SetValue(int value) => _intValue = value;
        
        public void SetValue(float value) => _floatValue = value;

        public void SetValue(string value) => _stringValue = value;

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
                case VariableType.String:
                    _stringValue = Convert.ToString(value);
                    break;
            }
        }
    }
}