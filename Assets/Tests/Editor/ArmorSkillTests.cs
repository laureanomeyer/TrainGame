using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using NUnit.Framework;
using UnityEngine;

public class ArmorSkillTests
{
    [Test]
    public void ArmorerSkillAssetUsesArmorSkillAndConfiguredProjectile()
    {
        const string skillAssetPath = "Assets/ScriptableObjects/SOEnemies/EnemyComponents/Skills/ArmorSkill.asset";
        const string armorPrefabPath = "Assets/Prefabs/Particles/Armor/Armor.prefab";

        ScriptableObject skillAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(skillAssetPath);
        Assert.That(skillAsset, Is.Not.Null);

        SerializedObject serializedAsset = new SerializedObject(skillAsset);
        SerializedProperty skillType = serializedAsset.FindProperty("skillType");
        Assert.That(skillType.enumNames[skillType.enumValueIndex], Is.EqualTo("Armor"));

        SerializedProperty skill = serializedAsset.FindProperty("skill");
        object managedSkill = skill.managedReferenceValue;
        Assert.That(managedSkill, Is.Not.Null);
        Assert.That(managedSkill.GetType().Name, Is.EqualTo("ArmorSkill"));

        GameObject armorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(armorPrefabPath);
        Assert.That(armorPrefab, Is.Not.Null);
        Assert.That(managedSkill.GetType().GetField("armorPrefab").GetValue(managedSkill), Is.EqualTo(armorPrefab));
        Assert.That((float)managedSkill.GetType().BaseType.GetField("cooldown").GetValue(managedSkill), Is.EqualTo(4f));
    }

    [Test]
    public void TargetSelectionReturnsFalseWhenOnlyCasterAndArmoredEnemiesAreActive()
    {
        Type enemyType = Type.GetType("Enemy, Assembly-CSharp", true);
        Type healthStateType = Type.GetType("EnemyHealthState, Assembly-CSharp", true);
        Type armorSkillType = Type.GetType("ArmorSkill, Assembly-CSharp", true);
        GameObject casterObject = new GameObject("Caster");
        GameObject armoredObject = new GameObject("Armored target");

        try
        {
            casterObject.SetActive(false);
            armoredObject.SetActive(false);

            Component caster = casterObject.AddComponent(enemyType);
            Component armored = armoredObject.AddComponent(enemyType);
            FieldInfo healthState = enemyType.GetField("healthState", BindingFlags.Instance | BindingFlags.NonPublic);
            healthState.SetValue(armored, Enum.Parse(healthStateType, "Armored"));

            Type enemyListType = typeof(System.Collections.Generic.List<>).MakeGenericType(enemyType);
            IList activeEnemies = (IList)Activator.CreateInstance(enemyListType);
            activeEnemies.Add(caster);
            activeEnemies.Add(armored);

            MethodInfo selectTarget = armorSkillType.GetMethod(
                "TrySelectTarget",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            Assert.That(selectTarget, Is.Not.Null);

            object[] arguments = { activeEnemies, caster, null };
            bool foundTarget = (bool)selectTarget.Invoke(null, arguments);

            Assert.That(foundTarget, Is.False);
            Assert.That(arguments[2], Is.Null);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(casterObject);
            UnityEngine.Object.DestroyImmediate(armoredObject);
        }
    }
}
