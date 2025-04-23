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

#if UNITY_EDITOR

        [HideInInspector] public Node NodeToDrawLineFrom;
        [HideInInspector] public Vector2 LinePosition = Vector2.zero;

        private string _localizationTableName;
        private string _characterNamesLocalizationName;
        
        public string LocalizationTableName => _localizationTableName;
        public string CharacterNamesLocalizationName => _characterNamesLocalizationName;
        
        public bool IsLocalizationSetUp { get; private set; }
        public static bool ShowLocalizationKeys { get; set; }

        public void AddLocalizationTable(string name)
        {
            IsLocalizationSetUp = true;
            _localizationTableName = name;
            EditorUtility.SetDirty(this);
        }

        public void AddCharacterNamesTable(string name)
        {
            _characterNamesLocalizationName = name;
            EditorUtility.SetDirty(this);
        }

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

        /// <summary>
        /// Draging all selected nodes
        /// </summary>
        /// <param name="delta"></param>
        public void DragAllSelectedNodes(Vector2 delta)
        {
            foreach (Node node in NodesList)
            {
                if (node.IsSelected)
                    node.DragNode(delta);
            }
        }

        /// <summary>
        /// Returning amount of selected nodes
        /// </summary>
        /// <returns></returns>
        public int GetAmountOfSelectedNodes()
        {
            int amount = 0;

            foreach (Node node in NodesList)
            {
                if (node.IsSelected)
                    amount++;
            }

            return amount;
        }

#endif
    }
}