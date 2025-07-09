using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace cherrydev
{
    /// <summary>
    /// Processes dialog text to replace variable placeholders with actual values
    /// </summary>
    public static class DialogTextProcessor
    {
        // Regex pattern to match {variableName} or {variableName:format}
        private static readonly Regex VariablePattern = 
            new(@"\{([a-zA-Z0-9_]+)(?::[^}]+)?\}", RegexOptions.Compiled);
        
        /// <summary>
        /// Process text and replace all variable placeholders with their values
        /// </summary>
        /// <param name="text">The text to process</param>
        /// <param name="variablesHandler">The variables handler to get values from</param>
        /// <returns>Processed text with variables replaced</returns>
        public static string ProcessText(string text, DialogVariablesHandler variablesHandler)
        {
            if (string.IsNullOrEmpty(text) || variablesHandler == null)
            {
                Debug.LogWarning($"ProcessText called with null text or handler. Text: '{text}', Handler null: {variablesHandler == null}");
                return text;
            }

            string result = VariablePattern.Replace(text, match =>
            {
                string variableName = match.Groups[1].Value.Trim();
                string format = match.Groups[2].Value.Trim();
                string value = GetVariableValueAsString(variableName, format, variablesHandler);
        
                return value;
            });

            return result;
        }
        
        /// <summary>
        /// Get variable value as formatted string
        /// </summary>
        private static string GetVariableValueAsString(string variableName, string format, DialogVariablesHandler variablesHandler)
        {
            Variable variable = variablesHandler.GetVariable(variableName);
            
            if (variable == null)
            {
                Debug.LogWarning($"Variable '{variableName}' not found in dialog text");
                return $"{{?{variableName}}}";
            }

            return variable.GetValueAsString();
        }
        
        /// <summary>
        /// Check if text contains any variable placeholders
        /// </summary>
        public static bool ContainsVariables(string text) => !string.IsNullOrEmpty(text) && VariablePattern.IsMatch(text);

        /// <summary>
        /// Get all variable names referenced in the text
        /// </summary>
        public static string[] GetReferencedVariables(string text)
        {
            if (string.IsNullOrEmpty(text))
                return Array.Empty<string>();
                
            MatchCollection matches = VariablePattern.Matches(text);
            string[] variableNames = new string[matches.Count];
            
            for (int i = 0; i < matches.Count; i++)
                variableNames[i] = matches[i].Groups[1].Value.Trim();
            
            return variableNames;
        }
    }
}