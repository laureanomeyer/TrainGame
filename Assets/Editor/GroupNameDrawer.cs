using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GroupNameAttribute))]
public class GroupNameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var groupsProp = property.serializedObject.FindProperty("groups");

        if (groupsProp == null || groupsProp.arraySize == 0)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        var names = new string[groupsProp.arraySize + 1];
        names[0] = "(ninguno)";

        for (int i = 0; i < groupsProp.arraySize; i++)
        {
            var groupName = groupsProp.GetArrayElementAtIndex(i).FindPropertyRelative("group").stringValue;
            names[i + 1] = string.IsNullOrEmpty(groupName) ? $"(sin nombre #{i})" : groupName;
        }

        int currentIndex = System.Array.IndexOf(names, property.stringValue);
        if (currentIndex < 0) currentIndex = 0;

        EditorGUI.BeginChangeCheck();
        int selected = EditorGUI.Popup(position, label.text, currentIndex, names);
        if (EditorGUI.EndChangeCheck())
        {
            property.stringValue = selected == 0 ? "" : names[selected];
        }
    }
}