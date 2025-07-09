using System;
using System.Collections.Generic;
using UnityEngine;

namespace cherrydev
{
    public class DialogExternalFunctionsHandler
    {
        public delegate object ExternalFunction();

        private readonly Dictionary<string, ExternalFunction> _externals = new();

        public ExternalFunction CallExternalFunction(string funcName)
        {
            if (_externals.ContainsKey(funcName))
            {
                ExternalFunction external = _externals[funcName];
                external?.Invoke();

                return _externals[funcName];
            }
            else
            {
                Debug.LogWarning($"There is no function with name '{funcName}'");
                return null;
            }
        }

        public void BindExternalFunction(string funcName, Action function)
        {
            BindExternalFunctionBase(funcName, () =>
            {
                function();
                return null;
            });
        }

        public void UnbindExternalFunction(string funcName)
        {
            if (_externals.ContainsKey(funcName))
                _externals.Remove(funcName);
        }

        private void BindExternalFunctionBase(string funcName, ExternalFunction externalFunction)
        {
            if (_externals.ContainsKey(funcName))
                _externals.Remove(funcName);

            _externals[funcName] = externalFunction;
        }
    }
}