using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/*
[CustomPropertyDrawer(typeof(GroupNameSuggestAttribute))]
public class GroupNameSuggestDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var particlesProp = property.serializedObject.FindProperty("particles");

        var distinctNames = new List<string>();
        if (particlesProp != null)
        {
            for (int i = 0; i < particlesProp.arraySize; i++)
            {
                var g = particlesProp.GetArrayElementAtIndex(i).FindPropertyRelative("group").stringValue;
                if (!string.IsNullOrEmpty(g) && !distinctNames.Contains(g))
                    distinctNames.Add(g);
            }
        }

        if (distinctNames.Count == 0)
        {
            // No hay grupos cargados todavía en particles: queda como texto libre.
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        const string customOption = "(escribir nuevo...)";
        var options = new List<string>(distinctNames) { customOption };

        int currentIndex = options.IndexOf(property.stringValue);
        bool isCustom = currentIndex < 0;

        float popupWidth = position.width * 0.5f;
        var popupRect = new Rect(position.x, position.y, isCustom ? popupWidth : position.width, position.height);
        var textRect = new Rect(position.x + popupWidth + 4, position.y, position.width - popupWidth - 4, position.height);

        int shownIndex = isCustom ? options.Count - 1 : currentIndex;

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();
        int selected = EditorGUI.Popup(popupRect, label.text, shownIndex, options.ToArray());
        if (EditorGUI.EndChangeCheck())
        {
            if (selected != options.Count - 1)
                property.stringValue = options[selected];
            else if (!isCustom)
                property.stringValue = ""; // recién pasa a modo custom, arranca vacío
        }

        if (selected == options.Count - 1)
        {
            property.stringValue = EditorGUI.TextField(textRect, property.stringValue);
        }
        EditorGUI.EndProperty();
    }
}
*/