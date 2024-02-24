using System;
using System.Collections.Generic;
using UnityEngine;

namespace cherrydev
{
    public class DialogExternalFunctionsHandler
    {
        public delegate object ExternalFunction();

        private Dictionary<string, ExternalFunction> externals = new Dictionary<string, ExternalFunction>();

        public ExternalFunction CallExternalFunction(string funcName)
        {
            if (externals.ContainsKey(funcName))
            {
                ExternalFunction external = externals[funcName];
                external?.Invoke();

                return externals[funcName];
            }
            else
            {
                Debug.LogWarning($"There is no function with name '{funcName}'");
                return null;
            }
        }

        public void BindExternalFunctionBase(string funcName, ExternalFunction externalFunction)
        {
            if (externals.ContainsKey(funcName))
            {
                Debug.LogWarning($"This function ({funcName}) is already binded");
                return;
            }

            externals[funcName] = externalFunction;
        }

        public void BindExternalFunction(string funcName, Action function)
        {
            BindExternalFunctionBase(funcName, () =>
            {
                function();
                return null;
            });
        }
    }
}