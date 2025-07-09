using UnityEditor;
using UnityEngine;

namespace cherrydev
{
    [CustomPropertyDrawer(typeof(Variable))]
    public class VariablePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty nameProperty = property.FindPropertyRelative("_name");
            SerializedProperty typeProperty = property.FindPropertyRelative("_type");
            SerializedProperty saveProperty = property.FindPropertyRelative("_saveToPrefs");

            Rect nameRect = new Rect(position.x, position.y, position.width * 0.4f, EditorGUIUtility.singleLineHeight);
            Rect typeRect = new Rect(position.x + position.width * 0.4f, position.y, position.width * 0.3f, EditorGUIUtility.singleLineHeight);
            Rect saveRect = new Rect(position.x + position.width * 0.7f, position.y, position.width * 0.3f, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(nameRect, nameProperty, GUIContent.none);
            EditorGUI.PropertyField(typeRect, typeProperty, GUIContent.none);
            EditorGUI.PropertyField(saveRect, saveProperty, new GUIContent("Save"));

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
    }
}