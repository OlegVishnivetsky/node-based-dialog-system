using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace cherrydev
{
    public class DialogLocalizationHandler
    {
        private static DialogLocalizationHandler _instance;
        public static DialogLocalizationHandler Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DialogLocalizationHandler();
                return _instance;
            }
        }

        /// <summary>
        /// Creates localization tables based on the dialog graph
        /// </summary>
        /// <param name="dialogGraph">The dialog graph to create localization for</param>
        public void SetupLocalization(DialogNodeGraph dialogGraph)
        {
            if (dialogGraph == null || dialogGraph.NodesList.Count == 0)
            {
                EditorUtility.DisplayDialog("Localization Setup", "No nodes found to generate localization entries.", "OK");
                return;
            }

            // Get table name based on the dialog graph asset name
            string tableName = GetTableNameFromGraph(dialogGraph);
            
            // Create CSV file first (as a backup)
            CreateLocalizationCSVFile(dialogGraph, tableName);
            
            // Create and populate the string table
            bool success = CreateAndPopulateStringTable(dialogGraph, tableName);
            
            if (success)
            {
                EditorUtility.DisplayDialog("Localization Setup Complete", 
                    $"A localization table named '{tableName}' has been created and populated with all dialog text from your nodes.\n\n" +
                    "You can view and edit it in the Localization Tables window.", 
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Partial Setup Completed", 
                    $"A CSV file with all dialog text has been created at 'Assets/Localization/{tableName}_strings.csv'.\n\n" +
                    "However, we couldn't automatically import it. You can import this file manually in the Localization Tables window.", 
                    "OK");
            }
            
            // Open the Localization Tables window
            EditorApplication.ExecuteMenuItem("Window/Asset Management/Localization Tables");
        }

        /// <summary>
        /// Updates existing localization entries for a dialog graph
        /// </summary>
        /// <param name="dialogGraph">The dialog graph to update localization for</param>
        public void UpdateLocalization(DialogNodeGraph dialogGraph)
        {
            SetupLocalization(dialogGraph);
        }

        /// <summary>
        /// Generates a table name from the dialog graph asset name
        /// </summary>
        /// <param name="dialogGraph">The dialog graph</param>
        /// <returns>A safe table name</returns>
        private string GetTableNameFromGraph(DialogNodeGraph dialogGraph)
        {
            string assetPath = AssetDatabase.GetAssetPath(dialogGraph);
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            
            // Ensure the name is safe for use as a table name
            string safeName = Regex.Replace(fileName, @"[^a-zA-Z0-9_]", "_");
            
            // If empty, use a default name
            if (string.IsNullOrEmpty(safeName))
                safeName = "Dialog";
                
            return safeName;
        }

        /// <summary>
        /// Creates a CSV file with all localizable text from the dialog graph
        /// </summary>
        /// <param name="dialogGraph">The dialog graph to extract text from</param>
        /// <param name="tableName">The name for the localization table</param>
        private void CreateLocalizationCSVFile(DialogNodeGraph dialogGraph, string tableName)
        {
            string outputPath = "Assets/Localization";
            if (!AssetDatabase.IsValidFolder(outputPath))
            {
                AssetDatabase.CreateFolder("Assets", "Localization");
            }
            
            string filePath = $"{outputPath}/{tableName}_strings.csv";
            
            // Collect all texts that need localization
            Dictionary<string, string> localizationEntries = new Dictionary<string, string>();
            Dictionary<Node, Dictionary<string, string>> nodeKeys = new Dictionary<Node, Dictionary<string, string>>();
            
            // Process all nodes and collect text
            foreach (Node node in dialogGraph.NodesList)
            {
                if (node is SentenceNode sentenceNode)
                {
                    Dictionary<string, string> keys = new Dictionary<string, string>();
                    
                    string charName = sentenceNode.GetSentenceCharacterName();
                    
                    if (!string.IsNullOrEmpty(charName))
                    {
                        string key = "char_" + GenerateSafeKey(charName);
                        localizationEntries[key] = charName;
                        keys["characterName"] = key;
                    }
                    
                    string text = sentenceNode.GetSentenceText();
                    
                    if (!string.IsNullOrEmpty(text))
                    {
                        string key = "text_" + GenerateSafeKey(text);
                        localizationEntries[key] = text;
                        keys["sentenceText"] = key;
                    }
                    
                    nodeKeys[node] = keys;
                }
                else if (node is AnswerNode answerNode)
                {
                    Dictionary<string, string> keys = new Dictionary<string, string>();
                    List<string> answerKeysList = new List<string>();
                    
                    // Process each answer
                    for (int i = 0; i < answerNode.Answers.Count; i++)
                    {
                        string answer = answerNode.Answers[i];
                        if (!string.IsNullOrEmpty(answer))
                        {
                            string key = "ans_" + GenerateSafeKey(answer);
                            localizationEntries[key] = answer;
                            answerKeysList.Add(key);
                        }
                        else
                        {
                            answerKeysList.Add(string.Empty);
                        }
                    }
                    
                    keys["answers"] = string.Join(",", answerKeysList);
                    nodeKeys[node] = keys;
                }
            }
            
            // Write to CSV file
            using (StreamWriter writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                // Write header
                writer.WriteLine("Key,English");
                
                // Write entries
                foreach (var entry in localizationEntries)
                {
                    // Escape quotes in the value
                    string escapedValue = entry.Value.Replace("\"", "\"\"");
                    writer.WriteLine($"{entry.Key},\"{escapedValue}\"");
                }
            }
            
            // Save keys to nodes
            SaveLocalizationKeysToNodes(nodeKeys);
            
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Creates a string table collection and populates it with the dialog text
        /// </summary>
        /// <param name="dialogGraph">The dialog graph to extract text from</param>
        /// <param name="tableName">The name for the localization table</param>
        /// <returns>True if successful, false otherwise</returns>
        private bool CreateAndPopulateStringTable(DialogNodeGraph dialogGraph, string tableName)
        {
            try
            {
                // Check if the Localization package is properly installed and accessible
                Type locSettingsType = Type.GetType("UnityEngine.Localization.Settings.LocalizationSettings, Unity.Localization");
                Type editorSettingsType = Type.GetType("UnityEditor.Localization.LocalizationEditorSettings, Unity.Localization.Editor");
                
                if (locSettingsType == null || editorSettingsType == null)
                {
                    Debug.LogWarning("Unity Localization package not found or not properly installed.");
                    return false;
                }

                // Get available locales
                var getLocalesMethod = editorSettingsType.GetMethod("GetLocales");
                var locales = getLocalesMethod.Invoke(null, null) as System.Collections.IList;
                
                if (locales == null || locales.Count == 0)
                {
                    // No locales found, let's add English
                    var addLocaleMethod = editorSettingsType.GetMethod("AddLocale");
                    
                    // Create an English locale
                    Type localeType = Type.GetType("UnityEngine.Localization.Locale, Unity.Localization");
                    var createLocaleMethod = localeType.GetMethod("CreateLocale", new Type[] { typeof(SystemLanguage) });
                    var englishLocale = createLocaleMethod.Invoke(null, new object[] { SystemLanguage.English });
                    
                    addLocaleMethod.Invoke(null, new object[] { englishLocale });
                    Debug.Log("Added English locale as none were found.");
                }
                
                // Check if the table collection already exists
                var getStringTableCollectionsMethod = editorSettingsType.GetMethod("GetStringTableCollections");
                var collections = getStringTableCollectionsMethod.Invoke(null, null) as System.Collections.IList;
                
                object tableCollection = null;
                foreach (var collection in collections)
                {
                    var nameProperty = collection.GetType().GetProperty("TableCollectionName");
                    string name = nameProperty.GetValue(collection) as string;
                    
                    if (name == tableName)
                    {
                        tableCollection = collection;
                        break;
                    }
                }
                
                // If not found, create one
                if (tableCollection == null)
                {
                    string assetPath = "Assets/Localization/StringTables";
                    
                    // Create directories if needed
                    if (!AssetDatabase.IsValidFolder("Assets/Localization"))
                        AssetDatabase.CreateFolder("Assets", "Localization");
                        
                    if (!AssetDatabase.IsValidFolder(assetPath))
                        AssetDatabase.CreateFolder("Assets/Localization", "StringTables");
                    
                    // Create the table collection
                    var createMethod = editorSettingsType.GetMethod("CreateStringTableCollection", 
                        new Type[] { typeof(string), typeof(string) });
                        
                    tableCollection = createMethod.Invoke(null, new object[] { tableName, assetPath });
                    Debug.Log($"Created new {tableName} string table collection.");
                }
                
                // Now populate the collection with data from our nodes
                PopulateStringTable(dialogGraph, tableCollection);
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating string table: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Populates a string table collection with text from the dialog graph
        /// </summary>
        /// <param name="dialogGraph">The dialog graph to extract text from</param>
        /// <param name="tableCollection">The string table collection object (accessed via reflection)</param>
        private void PopulateStringTable(DialogNodeGraph dialogGraph, object tableCollection)
        {
            try
            {
                // Get the SharedData to check for existing keys
                var sharedDataProperty = tableCollection.GetType().GetProperty("SharedData");
                var sharedData = sharedDataProperty.GetValue(tableCollection);
                
                // Get the string tables
                var tablesProperty = tableCollection.GetType().GetProperty("StringTables");
                var tables = tablesProperty.GetValue(tableCollection) as System.Collections.IList;
                
                // Collect all texts and their keys
                Dictionary<string, string> localizationEntries = new Dictionary<string, string>();
                Dictionary<Node, Dictionary<string, string>> nodeKeys = new Dictionary<Node, Dictionary<string, string>>();
                
                // Process all nodes and collect text
                foreach (Node node in dialogGraph.NodesList)
                {
                    if (node is SentenceNode sentenceNode)
                    {
                        Dictionary<string, string> keys = new Dictionary<string, string>();
                        
                        // Process character name
                        string charName = sentenceNode.GetSentenceCharacterName();
                        if (!string.IsNullOrEmpty(charName))
                        {
                            string key = "char_" + GenerateSafeKey(charName);
                            localizationEntries[key] = charName;
                            keys["characterName"] = key;
                        }
                        
                        // Process dialog text
                        string text = sentenceNode.GetSentenceText();
                        if (!string.IsNullOrEmpty(text))
                        {
                            string key = "text_" + GenerateSafeKey(text);
                            localizationEntries[key] = text;
                            keys["sentenceText"] = key;
                        }
                        
                        nodeKeys[node] = keys;
                    }
                    else if (node is AnswerNode answerNode)
                    {
                        Dictionary<string, string> keys = new Dictionary<string, string>();
                        List<string> answerKeysList = new List<string>();
                        
                        // Process each answer
                        for (int i = 0; i < answerNode.Answers.Count; i++)
                        {
                            string answer = answerNode.Answers[i];
                            if (!string.IsNullOrEmpty(answer))
                            {
                                string key = "ans_" + GenerateSafeKey(answer);
                                localizationEntries[key] = answer;
                                answerKeysList.Add(key);
                            }
                            else
                            {
                                answerKeysList.Add(string.Empty);
                            }
                        }
                        
                        keys["answers"] = string.Join(",", answerKeysList);
                        nodeKeys[node] = keys;
                    }
                }
                
                // Add entries to the table collection
                foreach (var entry in localizationEntries)
                {
                    try
                    {
                        // Check if the key already exists
                        var containsMethod = sharedData.GetType().GetMethod("Contains", new Type[] { typeof(string) });
                        bool keyExists = (bool)containsMethod.Invoke(sharedData, new object[] { entry.Key });
                        
                        if (!keyExists)
                        {
                            // Try to add the key to the shared data
                            try
                            {
                                var addKeyMethod = sharedData.GetType().GetMethod("AddKey", new Type[] { typeof(string) });
                                
                                if (addKeyMethod != null)
                                {
                                    addKeyMethod.Invoke(sharedData, new object[] { entry.Key });
                                }
                                else
                                {
                                    // Try alternative method
                                    var addEntryMethod = sharedData.GetType().GetMethod("AddKey", new Type[] { typeof(string), typeof(ulong) });
                                    if (addEntryMethod != null)
                                    {
                                        // Use a hash of the key as the ID
                                        ulong id = (ulong)Math.Abs(entry.Key.GetHashCode());
                                        addEntryMethod.Invoke(sharedData, new object[] { entry.Key, id });
                                    }
                                    else
                                    {
                                        Debug.LogWarning($"Could not add key {entry.Key} to shared data");
                                        continue;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"Error adding key {entry.Key}: {ex.Message}");
                                continue;
                            }
                        }
                        
                        // Add/update the entry in each table
                        foreach (var table in tables)
                        {
                            try
                            {
                                // Try to get the entry
                                var getEntryMethod = table.GetType().GetMethod("GetEntry", new Type[] { typeof(string) });
                                var entry1 = getEntryMethod?.Invoke(table, new object[] { entry.Key });
                                
                                if (entry1 == null)
                                {
                                    // Entry doesn't exist, add it
                                    var addEntryMethod = table.GetType().GetMethod("AddEntry", new Type[] { typeof(string), typeof(string) });
                                    MethodInfo addEntryByIdMethod = table.GetType().GetMethod("AddEntry", new Type[] { typeof(ulong), typeof(string) });
                                    if (addEntryMethod != null)
                                    {
                                        addEntryMethod.Invoke(table, new object[] { entry.Key, entry.Value });
                                    }
                                    else
                                    {
                                        var getIdMethod = sharedData.GetType().GetMethod("GetId", new Type[] { typeof(string) });
                                        ulong id = (ulong)getIdMethod.Invoke(sharedData, new object[] { entry.Key });

                                        if (addEntryByIdMethod != null)
                                            addEntryByIdMethod.Invoke(table, new object[] { id, entry.Value });
                                        else
                                            Debug.LogWarning($"Could not add entry {entry.Key} to table");
                                    }
                                }
                                else
                                {
                                    PropertyInfo valueProperty = entry1.GetType().GetProperty("Value");
                                    string currentValue = valueProperty.GetValue(entry1) as string;
                                    
                                    if (string.IsNullOrEmpty(currentValue))
                                        valueProperty.SetValue(entry1, entry.Value);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"Error processing table entry {entry.Key}: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Error processing entry {entry.Key}: {ex.Message}");
                    }
                }
                
                SaveLocalizationKeysToNodes(nodeKeys);
                
                EditorUtility.SetDirty(tableCollection as UnityEngine.Object);
                AssetDatabase.SaveAssets();
                
                Debug.Log("Dialog table populated successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error populating dialog table: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Saves localization keys back to the nodes
        /// </summary>
        /// <param name="nodeKeys">Dictionary mapping nodes to their keys</param>
        private void SaveLocalizationKeysToNodes(Dictionary<Node, Dictionary<string, string>> nodeKeys)
        {
            foreach (var nodePair in nodeKeys)
            {
                if (nodePair.Key is SentenceNode sentenceNode)
                {
                    SerializedObject serializedNode = new SerializedObject(sentenceNode);
                    
                    if (nodePair.Value.TryGetValue("characterName", out string charNameKey))
                    {
                        SerializedProperty keyProperty = serializedNode.FindProperty("_characterNameKey");
                        
                        if (keyProperty != null)
                        {
                            keyProperty.stringValue = charNameKey;
                        }
                    }
                    
                    if (nodePair.Value.TryGetValue("sentenceText", out string textKey))
                    {
                        SerializedProperty keyProperty = serializedNode.FindProperty("_sentenceTextKey");
                        
                        if (keyProperty != null)
                        {
                            keyProperty.stringValue = textKey;
                        }
                    }
                    
                    serializedNode.ApplyModifiedProperties();
                }
                else if (nodePair.Key is AnswerNode answerNode)
                {
                    if (nodePair.Value.TryGetValue("answers", out string answerKeysStr))
                    {
                        string[] answerKeys = answerKeysStr.Split(',');
                        
                        SerializedObject serializedNode = new SerializedObject(answerNode);
                        SerializedProperty keysProperty = serializedNode.FindProperty("_answerKeys");
                        
                        if (keysProperty != null)
                        {
                            keysProperty.ClearArray();
                            
                            for (int i = 0; i < answerKeys.Length; i++)
                            {
                                keysProperty.arraySize++;
                                keysProperty.GetArrayElementAtIndex(i).stringValue = answerKeys[i];
                            }
                        }
                        
                        serializedNode.ApplyModifiedProperties();
                    }
                }
            }
        }

        /// <summary>
        /// Generates a safe key from text
        /// </summary>
        /// <param name="text">The input text</param>
        /// <returns>A safe key string</returns>
        private string GenerateSafeKey(string text)
        {
            string shortText = text.Length > 20 ? text.Substring(0, 20) : text;
            string safeKey = Regex.Replace(shortText, @"[^a-zA-Z0-9_]", "_");
            int hashCode = text.GetHashCode();
            string uniqueKey = safeKey + "_" + Math.Abs(hashCode).ToString("X8");
            
            return uniqueKey;
        }
    }
}