using System;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
#if UNITY_LOCALIZATION
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;
#endif

namespace cherrydev
{
    public class NodeGraphLocalizer
    {
        private const string LocalizationPackageId = "com.unity.localization";
        private const string LocalizationDefineSymbol = "UNITY_LOCALIZATION";

        private static NodeGraphLocalizer _instance;

        public static NodeGraphLocalizer Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NodeGraphLocalizer();
                return _instance;
            }
        }

        /// <summary>
        /// Checks if the Unity Localization package is installed by looking for a key type.
        /// </summary>
        private bool IsLocalizationPackageInstalled() => 
            Type.GetType("UnityEngine.Localization.Tables.StringTable, Unity.Localization") != null;

        /// <summary>
        /// Creates localization tables based on the dialog graph.
        /// </summary>
        public void SetupLocalization(DialogNodeGraph dialogGraph, bool createNew = true)
        {
            if (dialogGraph == null)
            {
                EditorUtility.DisplayDialog("Error", "Dialog graph is null.", "OK");
                return;
            }

            if (!IsLocalizationPackageInstalled())
            {
                bool installPackage = EditorUtility.DisplayDialog(
                    "Localization Package Required",
                    "The Unity Localization package is required for this feature. Would you like to install it now?",
                    "Install", "Cancel");

                if (installPackage)
                    InstallLocalizationPackage();

                return;
            }

#if UNITY_LOCALIZATION
            StringTableCollection table;

            if (createNew)
            {
                if (LocalizationEditorSettings.GetStringTableCollection(dialogGraph.LocalizationTableName) != null)
                {
                    EditorUtility.DisplayDialog("Localization Setup",
                        $"Localization Table is already set up for this graph {dialogGraph.name}", "OK");
                    return;
                }

                table = LocalizationEditorSettings.CreateStringTableCollection(dialogGraph.name,
                    $"Assets/Localization/{dialogGraph.name}");
                
                if (table == null)
                {
                    EditorUtility.DisplayDialog("Error", "Failed to create localization table.", "OK");
                    return;
                }
                
                table.ClearAllEntries();
                dialogGraph.AddLocalizationTable(table.name);
            }
            else
            {
                if (!dialogGraph.IsLocalizationSetUp)
                {
                    EditorUtility.DisplayDialog("Update Keys",
                        "Click 'Set Up Localization Table' button at first!", "OK");
                    return;
                }

                table = LocalizationEditorSettings.GetStringTableCollection(dialogGraph.LocalizationTableName);
                
                if (table == null)
                {
                    EditorUtility.DisplayDialog("Error", "Localization table not found.", "OK");
                    return;
                }
                table.ClearAllEntries();
            }

            foreach (Node node in dialogGraph.NodesList)
            {
                if (node is SentenceNode)
                    SetUpSentenceNodeKeys(node, table);
                else if (node is AnswerNode)
                    SetUpAnswerNodeKeys(node, table);
            }

            if (createNew)
                EditorUtility.DisplayDialog("Localization Set Up",
                    "Localization table collection has been successfully configured for this dialog graph", "OK");
            else
                EditorUtility.DisplayDialog("Update Keys",
                    $"The keys in table [{dialogGraph.LocalizationTableName}] have been updated successfully", "OK");
#else
            EditorUtility.DisplayDialog(
                "Localization Define Missing",
                "UNITY_LOCALIZATION define symbol is not set. Localization features will not work properly.",
                "OK");
#endif
        }

        /// <summary>
        /// Checks if the UNITY_LOCALIZATION define symbol is set.
        /// </summary>
        private bool IsLocalizationDefineSymbolSet()
        {
#if UNITY_LOCALIZATION
            return true;
#else
            return false;
#endif
        }

#if UNITY_LOCALIZATION
        private void SetUpSentenceNodeKeys(Node node, StringTableCollection table)
        {
            SentenceNode sentenceNode = (SentenceNode)node;
            string characterName = sentenceNode.Sentence.CharacterName ?? string.Empty;
            string sentenceText = sentenceNode.Sentence.Text ?? string.Empty;
            string textKey = sentenceNode.SentenceTextKey ?? $"SentenceText_{Guid.NewGuid()}";
            
            sentenceNode.SentenceTextKey = textKey;

            string nameKey = sentenceNode.CharacterNameKey;
            
            if (string.IsNullOrEmpty(nameKey))
            {
                foreach (StringTable stringTable in table.StringTables)
                {
                    foreach (StringTableEntry entry in stringTable.Values)
                    {
                        if (entry.Value == characterName)
                        {
                            nameKey = entry.Key;
                            break;
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(nameKey))
                        break;
                }

                if (string.IsNullOrEmpty(nameKey))
                    nameKey = $"SentenceName_{Guid.NewGuid()}";

                sentenceNode.CharacterNameKey = nameKey;
            }

            foreach (var stringTable in table.StringTables)
            {
                if (stringTable.GetEntry(textKey) == null)
                    stringTable.AddEntry(textKey, sentenceText);

                if (stringTable.GetEntry(nameKey) == null)
                    stringTable.AddEntry(nameKey, characterName);
            }
        }

        private void SetUpAnswerNodeKeys(Node node, StringTableCollection table)
        {
            AnswerNode answerNode = (AnswerNode)node;

            while (answerNode.AnswerKeys.Count < answerNode.Answers.Count)
                answerNode.AnswerKeys.Add(null);

            for (int i = 0; i < answerNode.Answers.Count; i++)
            {
                string answer = answerNode.Answers[i];
                string answerKey = answerNode.AnswerKeys[i];

                if (string.IsNullOrEmpty(answerKey))
                {
                    answerKey = $"Answer_{Guid.NewGuid()}";
                    answerNode.AnswerKeys[i] = answerKey;
                }

                foreach (StringTable stringTable in table.StringTables)
                    stringTable.AddEntry(answerKey, answer);
            }
        }
#endif

        /// <summary>
        /// Installs the Unity Localization package via the Package Manager.
        /// </summary>
        private void InstallLocalizationPackage()
        {
            if (IsLocalizationPackageInstalled())
                return;
            
            AddRequest request = Client.Add(LocalizationPackageId);
            EditorUtility.DisplayProgressBar("Installing Package",
                "Installing the Unity Localization package...", 0.5f);

            EditorApplication.update += CheckInstallation;

            void CheckInstallation()
            {
                if (request.IsCompleted)
                {
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update -= CheckInstallation;

                    if (request.Status == StatusCode.Success)
                    {
                        EditorUtility.DisplayDialog("Installation Complete",
                            "The Unity Localization package has been installed successfully. " +
                            "Please restart Unity to complete the installation.",
                            "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Installation Failed",
                            $"Failed to install the Unity Localization package. Error: {request.Error.message}",
                            "OK");
                    }
                }
            }
        }
    }
}