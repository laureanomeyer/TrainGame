using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class ParticleGroupsGenerator
{
    private const string OutputPath = "Assets/Scripts/ParticleGroups.cs";

    [MenuItem("Tools/Generar ParticleGroups")]
    public static void Generate()
    {
        var names = new HashSet<string>();

        var controllers = Object.FindObjectsByType<ParticleSequenceController>(FindObjectsSortMode.None);
        var so = new SerializedObject(controllers.Length > 0 ? controllers[0] : null);

        foreach (var controller in controllers)
        {
            var serialized = new SerializedObject(controller);
            var groupsProp = serialized.FindProperty("groups");
            if (groupsProp == null) continue;

            for (int i = 0; i < groupsProp.arraySize; i++)
            {
                var g = groupsProp.GetArrayElementAtIndex(i).FindPropertyRelative("group").stringValue;
                if (!string.IsNullOrEmpty(g))
                    names.Add(g);
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generado por Tools/Generar ParticleGroups. No editar a mano.");
        sb.AppendLine("public static class ParticleGroups");
        sb.AppendLine("{");
        foreach (var name in names)
        {
            string constName = ToPascalCase(name);
            sb.AppendLine($"    public const string {constName} = \"{name}\";");
        }
        sb.AppendLine("}");

        File.WriteAllText(OutputPath, sb.ToString());
        AssetDatabase.Refresh();
        Debug.Log($"[ParticleGroupsGenerator] Generadas {names.Count} constantes en {OutputPath}.");
    }

    private static string ToPascalCase(string s)
    {
        var parts = s.Split(new[] { ' ', '_', '-' }, System.StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();
        foreach (var part in parts)
            sb.Append(char.ToUpper(part[0]) + part.Substring(1));
        return sb.ToString();
    }
}