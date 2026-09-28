using System.IO;
using UnityEditor;
using UnityEngine;

public static class WagonUpgradePathGenerator
{
    // Multiplicadores de costo de upgrade sobre el precio base del nivel 1
    private const float CostLv1To2 = 1.0f;
    private const float CostLv2To3 = 1.5f;

    private const string MenuPath = "Assets/Train Survival/Create Upgrade Paths";

    [MenuItem(MenuPath)]
    private static void CreateUpgradePaths()
    {
        var selected = Selection.GetFiltered<WagonInStockSO>(SelectionMode.Assets);
        int created = 0;

        foreach (var lv1 in selected)
        {
            string assetPath = AssetDatabase.GetAssetPath(lv1);
            string folder = Path.GetDirectoryName(assetPath).Replace('\\', '/');
            string baseName = GetBaseName(lv1.name);

            string pathAssetPath = $"{folder}/{baseName}_UpgradePath.asset";
            if (AssetDatabase.LoadAssetAtPath<WagonUpgradePathSO>(pathAssetPath) != null)
            {
                Debug.LogWarning($"[UpgradeGen] {baseName}: ya tiene UpgradePath, se saltea.");
                continue;
            }

            float basePrice = lv1.Price;
            float cost1 = Mathf.Round(basePrice * CostLv1To2);
            float cost2 = Mathf.Round(basePrice * CostLv2To3);

            var lv2 = CreateLevelCopy(lv1, folder, baseName, 2, "II", basePrice + cost1);
            var lv3 = CreateLevelCopy(lv1, folder, baseName, 3, "III", basePrice + cost1 + cost2);

            var path = ScriptableObject.CreateInstance<WagonUpgradePathSO>();
            path.levels = new[]
            {
                new WagonUpgradePathSO.WagonLevel { wagon = lv1, upgradeCost = cost1 },
                new WagonUpgradePathSO.WagonLevel { wagon = lv2, upgradeCost = cost2 },
                new WagonUpgradePathSO.WagonLevel { wagon = lv3, upgradeCost = 0f },
            };

            AssetDatabase.CreateAsset(path, pathAssetPath);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[UpgradeGen] Upgrade paths creados: {created}");
    }

    [MenuItem(MenuPath, true)]
    private static bool Validate() =>
        Selection.GetFiltered<WagonInStockSO>(SelectionMode.Assets).Length > 0;

    // "GatlingWagon_InStock 1" -> "GatlingWagon"
    private static string GetBaseName(string assetName)
    {
        int idx = assetName.IndexOf("_InStock");
        return idx >= 0 ? assetName.Substring(0, idx) : assetName;
    }

    private static WagonInStockSO CreateLevelCopy(
        WagonInStockSO source, string folder, string baseName,
        int level, string suffix, float price)
    {
        // Instantiate copia todos los campos serializados (prefabs incluidos, como placeholder)
        var copy = Object.Instantiate(source);

        var so = new SerializedObject(copy);
        so.FindProperty("wagonName").stringValue = $"{source.wagonName} {suffix}";
        so.FindProperty("price").floatValue = price;
        so.ApplyModifiedPropertiesWithoutUndo();

        string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{baseName}_Lv{level}.asset");
        AssetDatabase.CreateAsset(copy, path);
        return copy;
    }
}