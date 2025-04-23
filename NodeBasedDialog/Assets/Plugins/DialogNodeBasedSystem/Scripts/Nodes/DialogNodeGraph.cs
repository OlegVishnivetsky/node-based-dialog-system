using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
#if UNITY_LOCALIZATION
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
#endif

namespace cherrydev
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Nodes/Node Graph", fileName = "New Node Graph")]
    public class DialogNodeGraph : ScriptableObject
    {
        public List<Node> NodesList = new();

        [Space(7f)]
        [SerializeField] private string _localizationTableName;
        [SerializeField] private string _characterNamesLocalizationName;
        
        [HideInInspector] public Node NodeToDrawLineFrom;
        [HideInInspector] public Vector2 LinePosition = Vector2.zero;
        
        public string LocalizationTableName => _localizationTableName;
        public string CharacterNamesLocalizationName => _characterNamesLocalizationName;

        [HideInInspector]
        public bool IsLocalizationSetUp;
        
#if UNITY_EDITOR
        
        public static bool ShowLocalizationKeys;

        public void AddLocalizationTable(string name)
        {
            IsLocalizationSetUp = true;
            _localizationTableName = name;
            EditorUtility.SetDirty(this);
        }

        // TODO: Save character names to separate localization table. Since dialogues will most likely use many identical names. And it would be convenient to have a separate table just for names. 
        // public void AddCharacterNamesTable(string name)
        // {
        //     _characterNamesLocalizationName = name;
        //     EditorUtility.SetDirty(this);
        // }

        /// <summary>
        /// Assigning values to nodeToDrawLineFrom and linePosition fields
        /// </summary>
        /// <param name="nodeToDrawLineFrom"></param>
        /// <param name="linePosition"></param>
        public void SetNodeToDrawLineFromAndLinePosition(Node nodeToDrawLineFrom, Vector2 linePosition)
        {
            NodeToDrawLineFrom = nodeToDrawLineFrom;
            LinePosition = linePosition;
        }
#endif
    }
}