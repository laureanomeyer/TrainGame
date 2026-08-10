// Editor/SerializeReferenceDropdownDrawer.cs
#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnemySkill), true)]
public class EnemySkillDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        string currentTypeName = property.managedReferenceFullTypename;
        string displayName = string.IsNullOrEmpty(currentTypeName)
            ? "None (seleccionar skill)"
            : currentTypeName.Split('.', ' ').Last();

        if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(displayName), FocusType.Keyboard))
        {
            GenericMenu menu = new GenericMenu();
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(EnemySkill).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

            foreach (var type in types)
            {
                var capturedType = type;
                menu.AddItem(new GUIContent(capturedType.Name), false, () =>
                {
                    property.managedReferenceValue = Activator.CreateInstance(capturedType);
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        }

        Rect fieldsRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2,
            position.width, EditorGUI.GetPropertyHeight(property, true));

        EditorGUI.PropertyField(fieldsRect, property, GUIContent.none, true);
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight + 2 + EditorGUI.GetPropertyHeight(property, true);
    }
}
#endif