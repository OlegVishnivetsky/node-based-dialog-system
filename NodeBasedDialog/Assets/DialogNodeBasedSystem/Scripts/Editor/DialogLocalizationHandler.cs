using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.UI;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

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
        public void SetupLocalization(DialogNodeGraph dialogGraph, bool createNew = true)
        {
            StringTableCollection table;
            
            if (createNew)
            {
                if (LocalizationEditorSettings.GetStringTableCollection(dialogGraph.LocalizationTableName) != null)
                {
                    EditorUtility.DisplayDialog("Localization Setup", 
                        $"Localization Table is already set up for this  graph {dialogGraph.name}", "OK");
                    return;
                }
                
                table = LocalizationEditorSettings.CreateStringTableCollection(dialogGraph.name,
                    $"Assets/Localization/{dialogGraph.name}");
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
                table.ClearAllEntries();
            }

            foreach (Node node in dialogGraph.NodesList)
            {
                if (node is SentenceNode)
                    SetUpSentenceNodeKeys(node, table);
                else if (node is AnswerNode)
                    SetUpAnswerNodeKey(node, table);
            }

            if (createNew)
                EditorUtility.DisplayDialog("Localization Set Up", 
                    "Localization table collection has been successfully configured for this schedule", "OK");
            else
                EditorUtility.DisplayDialog("Update Keys", 
                    $"The keys in table [{dialogGraph.LocalizationTableName}] have been updated successfully", "OK");
        }

        /// <summary>
        /// Adds a new entry and generates keys for each field in each sentence node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="table"></param>
        private void SetUpSentenceNodeKeys(Node node, StringTableCollection table)
        {
            SentenceNode sentenceNode = (SentenceNode)node;

            foreach (StringTable stringTable in table.StringTables)
            {
                string nameKey = sentenceNode.CharacterNameKey ?? $"SentenceName_{Guid.NewGuid()}";
                string textKey = sentenceNode.SentenceTextKey ?? $"SentenceText_{Guid.NewGuid()}";
                        
                stringTable.AddEntry(nameKey, sentenceNode.GetSentenceCharacterName());
                stringTable.AddEntry(textKey, sentenceNode.GetSentenceText());
                
                sentenceNode.CharacterNameKey = nameKey;
                sentenceNode.SentenceTextKey = textKey;
            }
        }

        /// <summary>
        /// Adds a new entry and generates keys for each answer in each answer node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="table"></param>
        private void SetUpAnswerNodeKey(Node node, StringTableCollection table)
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
    }
}