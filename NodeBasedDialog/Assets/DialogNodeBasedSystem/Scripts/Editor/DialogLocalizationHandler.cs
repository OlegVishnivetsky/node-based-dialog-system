using System;
using System.Collections;
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
        public void SetupLocalization(DialogNodeGraph dialogGraph)
        {
            if (!ValidateDialogGraph(dialogGraph))
                return;

            string tableName = GetTableNameFromGraph(dialogGraph);
            CreateLocalizationCSVFile(dialogGraph, tableName);

            bool success = CreateAndPopulateStringTable(dialogGraph, tableName);
            DisplaySetupResult(success, tableName);

            EditorApplication.ExecuteMenuItem("Window/Asset Management/Localization Tables");
        }

        /// <summary>
        /// Updates existing localization entries using custom keys if specified
        /// </summary>
        public void UpdateLocalization(DialogNodeGraph dialogGraph, bool useCustomKeys)
        {
            if (!ValidateDialogGraph(dialogGraph))
                return;

            string tableName = GetTableNameFromGraph(dialogGraph);
    
            if (useCustomKeys)
            {
                UpdateLocalizationWithCustomKeys(dialogGraph, tableName);
                EditorUtility.DisplayDialog("Localization Update Complete", 
                    $"The localization table '{tableName}' has been updated with your custom keys.\nOld entries have been removed.", 
                    "OK");
            }
            else
            {
                DeleteStringTableCollection(tableName);
                CreateLocalizationCSVFile(dialogGraph, tableName);
                CreateAndPopulateStringTable(dialogGraph, tableName);
                EditorUtility.DisplayDialog("Localization Setup Complete", 
                    $"The localization table '{tableName}' has been recreated with automatically generated keys.", 
                    "OK");
            }
        }
        
        /// <summary>
        /// Deletes a string table collection if it exists
        /// </summary>
        private void DeleteStringTableCollection(string tableName)
        {
            try
            {
                Type editorSettingsType = Type.GetType("UnityEditor.Localization.LocalizationEditorSettings, Unity.Localization.Editor");
        
                if (editorSettingsType == null)
                    return;
        
                MethodInfo getStringTableCollectionsMethod = editorSettingsType.GetMethod("GetStringTableCollections");
                IList collections = getStringTableCollectionsMethod.Invoke(null, null) as IList;
        
                object tableToDelete = null;
                
                foreach (var collection in collections)
                {
                    PropertyInfo nameProperty = collection.GetType().GetProperty("TableCollectionName");
                    string name = nameProperty.GetValue(collection) as string;
            
                    if (name == tableName)
                    {
                        tableToDelete = collection;
                        break;
                    }
                }
        
                if (tableToDelete != null)
                {
                    UnityEngine.Object tableAsset = tableToDelete as UnityEngine.Object;
                    string assetPath = AssetDatabase.GetAssetPath(tableAsset);
            
                    if (!string.IsNullOrEmpty(assetPath))
                        AssetDatabase.DeleteAsset(assetPath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error deleting string table: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates if the dialog graph is valid for localization
        /// </summary>
        private bool ValidateDialogGraph(DialogNodeGraph dialogGraph)
        {
            if (dialogGraph == null || dialogGraph.NodesList.Count == 0)
            {
                EditorUtility.DisplayDialog("Localization Setup", "No nodes found to generate localization entries.",
                    "OK");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Displays the result of the localization setup
        /// </summary>
        private void DisplaySetupResult(bool success, string tableName)
        {
            if (success)
            {
                EditorUtility.DisplayDialog("Localization Setup Complete",
                    $"A localization table named '{tableName}' has been created\n\n" +
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
        }

        /// <summary>
        /// Generates a table name from the dialog graph asset name
        /// </summary>
        private string GetTableNameFromGraph(DialogNodeGraph dialogGraph)
        {
            string assetPath = AssetDatabase.GetAssetPath(dialogGraph);
            string fileName = Path.GetFileNameWithoutExtension(assetPath);

            string safeName = Regex.Replace(fileName, @"[^a-zA-Z0-9_]", "_");

            if (string.IsNullOrEmpty(safeName))
                safeName = "Dialog";

            return safeName;
        }

        /// <summary>
        /// Gets all existing keys from a table collection
        /// </summary>
        private HashSet<string> GetExistingKeysFromTable(object tableCollection)
        {
            HashSet<string> existingKeys = new HashSet<string>();

            try
            {
                PropertyInfo sharedDataProperty = tableCollection.GetType().GetProperty("SharedData");
                object sharedData = sharedDataProperty.GetValue(tableCollection);
                PropertyInfo entriesProperty = sharedData.GetType().GetProperty("Entries");
                IEnumerable entries = entriesProperty.GetValue(sharedData) as IEnumerable;

                if (entries != null)
                {
                    foreach (var entry in entries)
                    {
                        PropertyInfo keyProperty = entry.GetType().GetProperty("Key");
                        string key = keyProperty.GetValue(entry) as string;

                        if (!string.IsNullOrEmpty(key))
                            existingKeys.Add(key);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error getting existing keys: {ex.Message}");
            }

            return existingKeys;
        }

        /// <summary>
        /// Removes keys from the table that aren't in the keysToKeep set
        /// </summary>
        private void RemoveOldKeysFromTable(object tableCollection, HashSet<string> existingKeys,
            HashSet<string> keysToKeep)
        {
            try
            {
                PropertyInfo sharedDataProperty = tableCollection.GetType().GetProperty("SharedData");
                object sharedData = sharedDataProperty.GetValue(tableCollection);
                List<string> keysToRemove = new List<string>();

                foreach (string key in existingKeys)
                {
                    if (!keysToKeep.Contains(key))
                        keysToRemove.Add(key);
                }

                foreach (string key in keysToRemove)
                {
                    try
                    {
                        MethodInfo removeKeyMethod =
                            sharedData.GetType().GetMethod("RemoveKey", new Type[] { typeof(string) });
                        if (removeKeyMethod != null)
                        {
                            removeKeyMethod.Invoke(sharedData, new object[] { key });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Error removing key {key}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error removing old keys: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a CSV file with all localizable text from the dialog graph
        /// </summary>
        private void CreateLocalizationCSVFile(DialogNodeGraph dialogGraph, string tableName)
        {
            string outputPath = EnsureLocalizationFolder();
            string filePath = $"{outputPath}/{tableName}_strings.csv";

            Dictionary<string, string> localizationEntries = new Dictionary<string, string>();
            Dictionary<Node, Dictionary<string, string>> nodeKeys =
                ExtractLocalizationData(dialogGraph, localizationEntries);

            WriteCSVFile(filePath, localizationEntries);
            SaveLocalizationKeysToNodes(nodeKeys);

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Ensures the Localization folder exists
        /// </summary>
        private string EnsureLocalizationFolder()
        {
            string outputPath = "Assets/Localization";

            if (!AssetDatabase.IsValidFolder(outputPath))
                AssetDatabase.CreateFolder("Assets", "Localization");

            return outputPath;
        }

        /// <summary>
        /// Extracts localization data from a dialog graph
        /// </summary>
        private Dictionary<Node, Dictionary<string, string>> ExtractLocalizationData(
            DialogNodeGraph dialogGraph,
            Dictionary<string, string> localizationEntries)
        {
            Dictionary<Node, Dictionary<string, string>> nodeKeys = new Dictionary<Node, Dictionary<string, string>>();

            foreach (Node node in dialogGraph.NodesList)
            {
                if (node is SentenceNode sentenceNode)
                    ProcessSentenceNode(sentenceNode, localizationEntries, nodeKeys);
                else if (node is AnswerNode answerNode)
                    ProcessAnswerNode(answerNode, localizationEntries, nodeKeys);
            }

            return nodeKeys;
        }

        /// <summary>
        /// Processes a sentence node for localization
        /// </summary>
        private void ProcessSentenceNode(
            SentenceNode sentenceNode,
            Dictionary<string, string> localizationEntries,
            Dictionary<Node, Dictionary<string, string>> nodeKeys)
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

            nodeKeys[sentenceNode] = keys;
        }

        /// <summary>
        /// Processes an answer node for localization
        /// </summary>
        private void ProcessAnswerNode(
            AnswerNode answerNode,
            Dictionary<string, string> localizationEntries,
            Dictionary<Node, Dictionary<string, string>> nodeKeys)
        {
            Dictionary<string, string> keys = new Dictionary<string, string>();
            List<string> answerKeysList = new List<string>();

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
                    answerKeysList.Add(string.Empty);
            }

            keys["answers"] = string.Join(",", answerKeysList);
            nodeKeys[answerNode] = keys;
        }

        /// <summary>
        /// Writes localization data to a CSV file
        /// </summary>
        private void WriteCSVFile(string filePath, Dictionary<string, string> localizationEntries)
        {
            using (StreamWriter writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                writer.WriteLine("Key,English");

                foreach (KeyValuePair<string, string> entry in localizationEntries)
                {
                    string escapedValue = entry.Value.Replace("\"", "\"\"");
                    writer.WriteLine($"{entry.Key},\"{escapedValue}\"");
                }
            }
        }

        /// <summary>
        /// Creates a string table collection and populates it with the dialog text
        /// </summary>
        private bool CreateAndPopulateStringTable(DialogNodeGraph dialogGraph, string tableName)
        {
            try
            {
                Type locSettingsType = Type.GetType("UnityEngine.Localization.Settings.LocalizationSettings, Unity.Localization");
                Type editorSettingsType = Type.GetType("UnityEditor.Localization.LocalizationEditorSettings, Unity.Localization.Editor");
        
                if (locSettingsType == null || editorSettingsType == null)
                {
                    Debug.LogWarning("Unity Localization package not found or not properly installed.");
                    return false;
                }

                EnsureLocalesExist(editorSettingsType);
        
                object tableCollection = GetOrCreateStringTableCollection(tableName, editorSettingsType);
        
                ClearAllEntriesFromTable(tableCollection);
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
        /// Clears all entries from a table collection
        /// </summary>
        private void ClearAllEntriesFromTable(object tableCollection)
        {
            try
            {
                PropertyInfo sharedDataProperty = tableCollection.GetType().GetProperty("SharedData");
                object sharedData = sharedDataProperty.GetValue(tableCollection);
                PropertyInfo entriesProperty = sharedData.GetType().GetProperty("Entries");
                IEnumerable entries = entriesProperty.GetValue(sharedData) as IEnumerable;
        
                if (entries != null)
                {
                    List<string> keysToRemove = new List<string>();
            
                    foreach (object entry in entries)
                    {
                        PropertyInfo keyProperty = entry.GetType().GetProperty("Key");
                        string key = keyProperty.GetValue(entry) as string;
                
                        if (!string.IsNullOrEmpty(key))
                            keysToRemove.Add(key);
                    }
            
                    foreach (string key in keysToRemove)
                    {
                        try
                        {
                            MethodInfo removeKeyMethod = sharedData.GetType().GetMethod("RemoveKey", new Type[] { typeof(string) });
                            if (removeKeyMethod != null)
                            {
                                removeKeyMethod.Invoke(sharedData, new object[] { key });
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"Error removing key {key}: {ex.Message}");
                        }
                    }
            
                    Debug.Log($"Cleared {keysToRemove.Count} entries from table");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error clearing table entries: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Ensures that at least one locale exists
        /// </summary>
        private void EnsureLocalesExist(Type editorSettingsType)
        {
            MethodInfo getLocalesMethod = editorSettingsType.GetMethod("GetLocales");
            IList locales = getLocalesMethod.Invoke(null, null) as System.Collections.IList;

            if (locales == null || locales.Count == 0)
            {
                MethodInfo addLocaleMethod = editorSettingsType.GetMethod("AddLocale");

                Type localeType = Type.GetType("UnityEngine.Localization.Locale, Unity.Localization");
                MethodInfo createLocaleMethod =
                    localeType.GetMethod("CreateLocale", new Type[] { typeof(SystemLanguage) });
                object englishLocale = createLocaleMethod.Invoke(null, new object[] { SystemLanguage.English });

                addLocaleMethod.Invoke(null, new object[] { englishLocale });
                Debug.Log("Added English locale as none were found.");
            }
        }

        /// <summary>
        /// Gets or creates a string table collection
        /// </summary>
        private object GetOrCreateStringTableCollection(string tableName, Type editorSettingsType)
        {
            MethodInfo getStringTableCollectionsMethod = editorSettingsType.GetMethod("GetStringTableCollections");
            IList collections = getStringTableCollectionsMethod.Invoke(null, null) as IList;

            foreach (object collection in collections)
            {
                PropertyInfo nameProperty = collection.GetType().GetProperty("TableCollectionName");
                string name = nameProperty.GetValue(collection) as string;

                if (name == tableName)
                    return collection;
            }

            string assetPath = EnsureStringTablesFolder();

            MethodInfo createMethod = editorSettingsType.GetMethod("CreateStringTableCollection",
                new Type[] { typeof(string), typeof(string) });

            object newCollection = createMethod.Invoke(null, new object[] { tableName, assetPath });
            Debug.Log($"Created new {tableName} string table collection.");

            return newCollection;
        }

        /// <summary>
        /// Ensures that the StringTables folder exists
        /// </summary>
        private string EnsureStringTablesFolder()
        {
            string outputPath = "Assets/Localization";
            string assetPath = $"{outputPath}/StringTables";

            if (!AssetDatabase.IsValidFolder(outputPath))
                AssetDatabase.CreateFolder("Assets", "Localization");

            if (!AssetDatabase.IsValidFolder(assetPath))
                AssetDatabase.CreateFolder(outputPath, "StringTables");

            return assetPath;
        }

        /// <summary>
        /// Populates a string table collection with text from the dialog graph
        /// </summary>
        private void PopulateStringTable(DialogNodeGraph dialogGraph, object tableCollection)
        {
            try
            {
                PropertyInfo sharedDataProperty = tableCollection.GetType().GetProperty("SharedData");
                object sharedData = sharedDataProperty.GetValue(tableCollection);

                PropertyInfo tablesProperty = tableCollection.GetType().GetProperty("StringTables");
                IList tables = tablesProperty.GetValue(tableCollection) as IList;

                Dictionary<string, string> localizationEntries = new Dictionary<string, string>();
                Dictionary<Node, Dictionary<string, string>> nodeKeys =
                    ExtractLocalizationData(dialogGraph, localizationEntries);

                foreach (KeyValuePair<string, string> entry in localizationEntries)
                {
                    AddKeyToSharedData(sharedData, entry.Key);
                    UpdateTablesWithEntry(tables, sharedData, entry.Key, entry.Value);
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
        /// Updates localization with custom keys
        /// </summary>
        private void UpdateLocalizationWithCustomKeys(DialogNodeGraph dialogGraph, string tableName)
        {
            string outputPath = EnsureLocalizationFolder();

            // Step 1: Extract all current keys and values from nodes
            Dictionary<string, string> currentEntries = ExtractCustomLocalizationEntries(dialogGraph);

            if (currentEntries.Count == 0)
            {
                Debug.LogWarning("No custom keys found. Make sure you've edited localization keys in the node editor.");
                return;
            }

            try
            {
                Type editorSettingsType =
                    Type.GetType("UnityEditor.Localization.LocalizationEditorSettings, Unity.Localization.Editor");

                if (editorSettingsType == null)
                {
                    Debug.LogWarning("Unity Localization package not found.");
                    return;
                }

                EnsureLocalesExist(editorSettingsType);
                object tableCollection = GetOrCreateStringTableCollection(tableName, editorSettingsType);

                HashSet<string> keysToKeep = new HashSet<string>(currentEntries.Keys);
                HashSet<string> existingKeys = GetExistingKeysFromTable(tableCollection);

                RemoveOldKeysFromTable(tableCollection, existingKeys, keysToKeep);
                UpdateTableCollectionWithEntries(tableCollection, currentEntries);

                // Write backup CSV
                string filePath = $"{outputPath}/{tableName}_custom_keys.csv";
                WriteCSVFile(filePath, currentEntries);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating string table: {ex.Message}");
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Extracts localization entries with custom keys
        /// </summary>
        private Dictionary<string, string> ExtractCustomLocalizationEntries(DialogNodeGraph dialogGraph)
        {
            Dictionary<string, string> entries = new Dictionary<string, string>();

            foreach (Node node in dialogGraph.NodesList)
            {
                if (node is SentenceNode sentenceNode)
                {
                    SerializedObject serializedNode = new SerializedObject(sentenceNode);

                    string charName = sentenceNode.GetSentenceCharacterName();
                    SerializedProperty charNameKeyProp = serializedNode.FindProperty("_characterNameKey");

                    if (!string.IsNullOrEmpty(charName))
                    {
                        string key = charNameKeyProp.stringValue;

                        if (string.IsNullOrEmpty(key))
                        {
                            key = "char_" + GenerateSafeKey(charName);
                            charNameKeyProp.stringValue = key;
                            serializedNode.ApplyModifiedProperties();
                        }

                        entries[key] = charName;
                    }

                    string text = sentenceNode.GetSentenceText();
                    SerializedProperty textKeyProp = serializedNode.FindProperty("_sentenceTextKey");

                    if (!string.IsNullOrEmpty(text))
                    {
                        string key = textKeyProp.stringValue;

                        if (string.IsNullOrEmpty(key))
                        {
                            key = "text_" + GenerateSafeKey(text);
                            textKeyProp.stringValue = key;
                            serializedNode.ApplyModifiedProperties();
                        }

                        entries[key] = text;
                    }
                }
                else if (node is AnswerNode answerNode)
                {
                    SerializedObject serializedNode = new SerializedObject(answerNode);
                    SerializedProperty answerKeysProp = serializedNode.FindProperty("_answerKeys");

                    while (answerKeysProp.arraySize < answerNode.Answers.Count)
                    {
                        answerKeysProp.arraySize++;
                    }

                    for (int i = 0; i < answerNode.Answers.Count; i++)
                    {
                        string answer = answerNode.Answers[i];

                        if (!string.IsNullOrEmpty(answer))
                        {
                            string key = answerKeysProp.GetArrayElementAtIndex(i).stringValue;

                            if (string.IsNullOrEmpty(key))
                            {
                                key = "ans_" + GenerateSafeKey(answer);
                                answerKeysProp.GetArrayElementAtIndex(i).stringValue = key;
                                serializedNode.ApplyModifiedProperties();
                            }

                            entries[key] = answer;
                        }
                    }
                }
            }

            return entries;
        }

        /// <summary>
        /// Extracts custom entries from a sentence node
        /// </summary>
        private void ExtractSentenceNodeCustomEntries(SentenceNode sentenceNode, Dictionary<string, string> entries)
        {
            SerializedObject serializedNode = new SerializedObject(sentenceNode);
            string charName = sentenceNode.GetSentenceCharacterName();
            SerializedProperty charNameKeyProp = serializedNode.FindProperty("_characterNameKey");

            if (!string.IsNullOrEmpty(charName) && !string.IsNullOrEmpty(charNameKeyProp.stringValue))
                entries[charNameKeyProp.stringValue] = charName;

            string text = sentenceNode.GetSentenceText();
            SerializedProperty textKeyProp = serializedNode.FindProperty("_sentenceTextKey");

            if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(textKeyProp.stringValue))
                entries[textKeyProp.stringValue] = text;
        }

        /// <summary>
        /// Extracts custom entries from an answer node
        /// </summary>
        private void ExtractAnswerNodeCustomEntries(AnswerNode answerNode, Dictionary<string, string> entries)
        {
            SerializedObject serializedNode = new SerializedObject(answerNode);
            SerializedProperty answerKeysProp = serializedNode.FindProperty("_answerKeys");

            for (int i = 0; i < answerNode.Answers.Count && i < answerKeysProp.arraySize; i++)
            {
                string answer = answerNode.Answers[i];
                string key = answerKeysProp.GetArrayElementAtIndex(i).stringValue;

                if (!string.IsNullOrEmpty(answer) && !string.IsNullOrEmpty(key))
                    entries[key] = answer;
            }
        }

        /// <summary>
        /// Updates a table collection with custom entries
        /// </summary>
        private void UpdateTableCollectionWithEntries(object tableCollection, Dictionary<string, string> entries)
        {
            PropertyInfo sharedDataProperty = tableCollection.GetType().GetProperty("SharedData");
            object sharedData = sharedDataProperty.GetValue(tableCollection);

            PropertyInfo tablesProperty = tableCollection.GetType().GetProperty("StringTables");
            IList tables = tablesProperty.GetValue(tableCollection) as IList;

            foreach (KeyValuePair<string, string> entry in entries)
            {
                AddKeyToSharedData(sharedData, entry.Key);
                UpdateTablesWithEntry(tables, sharedData, entry.Key, entry.Value);
            }

            EditorUtility.SetDirty(tableCollection as UnityEngine.Object);
            AssetDatabase.SaveAssets();

            Debug.Log($"Updated {entries.Count} entries in string table.");
        }

        /// <summary>
        /// Adds a key to the shared data
        /// </summary>
        private void AddKeyToSharedData(object sharedData, string key)
        {
            try
            {
                MethodInfo containsMethod = sharedData.GetType().GetMethod("Contains", new Type[] { typeof(string) });
                bool keyExists = (bool)containsMethod.Invoke(sharedData, new object[] { key });

                if (!keyExists)
                {
                    try
                    {
                        MethodInfo addKeyMethod =
                            sharedData.GetType().GetMethod("AddKey", new Type[] { typeof(string) });

                        if (addKeyMethod != null)
                            addKeyMethod.Invoke(sharedData, new object[] { key });
                        else
                        {
                            MethodInfo addEntryMethod = sharedData.GetType()
                                .GetMethod("AddKey", new Type[] { typeof(string), typeof(ulong) });

                            if (addEntryMethod != null)
                            {
                                ulong id = (ulong)Math.Abs(key.GetHashCode());
                                addEntryMethod.Invoke(sharedData, new object[] { key, id });
                            }
                            else
                                Debug.LogWarning($"Could not add key {key} to shared data");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Error adding key {key}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error checking key {key}: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates all tables with an entry
        /// </summary>
        private void UpdateTablesWithEntry(IList tables, object sharedData, string key, string value)
        {
            foreach (object table in tables)
            {
                try
                {
                    MethodInfo getEntryMethod = table.GetType().GetMethod("GetEntry", new Type[] { typeof(string) });
                    object entry = getEntryMethod?.Invoke(table, new object[] { key });

                    if (entry == null)
                        AddEntryToTable(table, sharedData, key, value);
                    else
                        UpdateEntryInTable(entry, value);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Error processing table entry {key}: {ex.Message}");
                }
            }
        }


        /// <summary>
        /// Adds an entry to a table
        /// </summary>
        private void AddEntryToTable(object table, object sharedData, string key, string value)
        {
            MethodInfo addEntryMethod =
                table.GetType().GetMethod("AddEntry", new Type[] { typeof(string), typeof(string) });

            if (addEntryMethod != null)
                addEntryMethod.Invoke(table, new object[] { key, value });
            else
            {
                try
                {
                    MethodInfo getIdMethod = sharedData.GetType().GetMethod("GetId", new Type[] { typeof(string) });
                    ulong id = (ulong)getIdMethod.Invoke(sharedData, new object[] { key });

                    MethodInfo addEntryByIdMethod =
                        table.GetType().GetMethod("AddEntry", new Type[] { typeof(ulong), typeof(string) });

                    if (addEntryByIdMethod != null)
                        addEntryByIdMethod.Invoke(table, new object[] { id, value });
                    else
                        Debug.LogWarning($"Could not add entry {key} to table");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Error adding entry {key}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Updates an entry in a table
        /// </summary>
        private void UpdateEntryInTable(object entry, string value)
        {
            PropertyInfo valueProperty = entry.GetType().GetProperty("Value");
            string currentValue = valueProperty.GetValue(entry) as string;

            valueProperty.SetValue(entry, value);
        }

        /// <summary>
        /// Saves localization keys back to the nodes
        /// </summary>
        private void SaveLocalizationKeysToNodes(Dictionary<Node, Dictionary<string, string>> nodeKeys)
        {
            foreach (KeyValuePair<Node, Dictionary<string, string>> nodePair in nodeKeys)
            {
                if (nodePair.Key is SentenceNode sentenceNode)
                    SaveKeysToSentenceNode(sentenceNode, nodePair.Value);
                else if (nodePair.Key is AnswerNode answerNode)
                    SaveKeysToAnswerNode(answerNode, nodePair.Value);
            }
        }

        /// <summary>
        /// Saves keys to a sentence node
        /// </summary>
        private void SaveKeysToSentenceNode(SentenceNode sentenceNode, Dictionary<string, string> keys)
        {
            SerializedObject serializedNode = new SerializedObject(sentenceNode);

            if (keys.TryGetValue("characterName", out string charNameKey))
            {
                SerializedProperty keyProperty = serializedNode.FindProperty("_characterNameKey");

                if (keyProperty != null)
                    keyProperty.stringValue = charNameKey;
            }

            if (keys.TryGetValue("sentenceText", out string textKey))
            {
                SerializedProperty keyProperty = serializedNode.FindProperty("_sentenceTextKey");

                if (keyProperty != null)
                    keyProperty.stringValue = textKey;
            }

            serializedNode.ApplyModifiedProperties();
        }

        /// <summary>
        /// Saves keys to an answer node
        /// </summary>
        private void SaveKeysToAnswerNode(AnswerNode answerNode, Dictionary<string, string> keys)
        {
            if (keys.TryGetValue("answers", out string answerKeysStr))
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

        /// <summary>
        /// Generates a safe key from text
        /// </summary>
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