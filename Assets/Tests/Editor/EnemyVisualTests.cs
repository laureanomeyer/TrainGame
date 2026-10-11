using System.Reflection;
using UnityEditor;
using NUnit.Framework;
using UnityEngine;

public class EnemyVisualTests
{
    [Test]
    public void EnemyDataReferencesVisualWithoutOwningMeshOrAnimationFields()
    {
        System.Type enemyDataType = System.Type.GetType("EnemyData, Assembly-CSharp", true);

        Assert.That(enemyDataType.GetField("visualSO"), Is.Not.Null);
        Assert.That(enemyDataType.GetField("enemyMesh"), Is.Null);
        Assert.That(enemyDataType.GetField("horseMesh"), Is.Null);
        Assert.That(enemyDataType.GetField("material"), Is.Null);
        Assert.That(enemyDataType.GetField("animation"), Is.Null);
    }

    [Test]
    public void ApplyVisualBuildsSeparateOverridesFromDirectClipReferences()
    {
        const string cowboyOverridePath = "Assets/Assets/Assests3D/Enemy/EnemyAssets/Visual Prefabs/Visual_BandidoOverride.overrideController";
        const string cowboyControllerPath = "Assets/Assets/Assests3D/Enemy/Animations/EnemyAnimationController.controller";
        const string mountControllerPath = "Assets/Assets/Assests3D/Enemy/Animations/HorseAnimationController.controller";
        AnimatorOverrideController sourceCowboyOverride = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(cowboyOverridePath);
        RuntimeAnimatorController cowboyBaseController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(cowboyControllerPath);
        RuntimeAnimatorController mountBaseController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(mountControllerPath);
        Assert.That(sourceCowboyOverride, Is.Not.Null);
        Assert.That(cowboyBaseController, Is.Not.Null);
        Assert.That(mountBaseController, Is.Not.Null);

        var cowboySourceClips = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>>();
        sourceCowboyOverride.GetOverrides(cowboySourceClips);
        Assert.That(cowboySourceClips, Is.Not.Empty);
        AnimationClip cowboyBaseClip = cowboySourceClips[0].Key;
        AnimationClip cowboyReplacementClip = new AnimationClip();

        AnimatorOverrideController sourceMountOverride = new AnimatorOverrideController(mountBaseController);
        var mountSourceClips = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>>();
        sourceMountOverride.GetOverrides(mountSourceClips);
        Assert.That(mountSourceClips, Is.Not.Empty);
        AnimationClip mountBaseClip = mountSourceClips[0].Key;
        AnimationClip mountReplacementClip = new AnimationClip();

        GameObject visualPrefab = new GameObject("Visual");
        visualPrefab.SetActive(false);
        GameObject cowboyObject = new GameObject("Cowboy");
        GameObject mountObject = new GameObject("Mount");
        cowboyObject.transform.SetParent(visualPrefab.transform);
        mountObject.transform.SetParent(visualPrefab.transform);
        Animator cowboyAnimator = cowboyObject.AddComponent<Animator>();
        Animator mountAnimator = mountObject.AddComponent<Animator>();
        System.Type bindingsType = System.Type.GetType("EnemyVisualAnimatorBindings, Assembly-CSharp", true);
        Component bindings = visualPrefab.AddComponent(bindingsType);
        SetPrivateField(bindings, "cowboyAnimator", cowboyAnimator);
        SetPrivateField(bindings, "mountAnimator", mountAnimator);
        cowboyAnimator.runtimeAnimatorController = cowboyBaseController;
        mountAnimator.runtimeAnimatorController = mountBaseController;

        System.Type visualType = System.Type.GetType("EnemyVisualSO, Assembly-CSharp", true);
        ScriptableObject visualDefinition = ScriptableObject.CreateInstance(visualType);
        System.Type clipOverrideType = System.Type.GetType("AnimationClipOverride, Assembly-CSharp", true);
        System.Array cowboyClipOverrides = CreateClipOverrideArray(clipOverrideType, cowboyBaseClip, cowboyReplacementClip);
        System.Array mountClipOverrides = CreateClipOverrideArray(clipOverrideType, mountBaseClip, mountReplacementClip);
        SetPublicField(visualDefinition, "visualPrefab", visualPrefab);
        SetPublicField(visualDefinition, "cowboyClipOverrides", cowboyClipOverrides);
        SetPublicField(visualDefinition, "mountClipOverrides", mountClipOverrides);

        GameObject enemyObject = new GameObject("Enemy");
        try
        {
            System.Type enemyType = System.Type.GetType("Enemy, Assembly-CSharp", true);
            Component enemy = enemyObject.AddComponent(enemyType);
            MethodInfo applyVisual = enemyType.GetMethod("ApplyVisual", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(applyVisual, Is.Not.Null);
            applyVisual.Invoke(enemy, new object[] { visualDefinition });

            Transform visualAnchor = (Transform)enemyType.GetProperty("VisualAnchor").GetValue(enemy);
            Component instanceBindings = visualAnchor.GetComponentInChildren(bindingsType, true);
            Assert.That(instanceBindings, Is.Not.Null);
            Animator instanceCowboy = (Animator)bindingsType.GetProperty("CowboyAnimator").GetValue(instanceBindings);
            Animator instanceMount = (Animator)bindingsType.GetProperty("MountAnimator").GetValue(instanceBindings);
            AssertAppliedClip(instanceCowboy, cowboyBaseClip, cowboyReplacementClip);
            AssertAppliedClip(instanceMount, mountBaseClip, mountReplacementClip);
        }
        finally
        {
            Object.DestroyImmediate(enemyObject);
            Object.DestroyImmediate(visualPrefab);
            Object.DestroyImmediate(visualDefinition);
            Object.DestroyImmediate(sourceMountOverride);
            Object.DestroyImmediate(cowboyReplacementClip);
            Object.DestroyImmediate(mountReplacementClip);
        }
    }

    private static System.Array CreateClipOverrideArray(System.Type entryType, AnimationClip baseClip, AnimationClip replacementClip)
    {
        object entry = System.Activator.CreateInstance(entryType);
        SetPublicField(entry, "baseClip", baseClip);
        SetPublicField(entry, "replacementClip", replacementClip);
        System.Array entries = System.Array.CreateInstance(entryType, 1);
        entries.SetValue(entry, 0);
        return entries;
    }

    private static void AssertAppliedClip(Animator animator, AnimationClip baseClip, AnimationClip replacementClip)
    {
        AnimatorOverrideController appliedOverride = animator.runtimeAnimatorController as AnimatorOverrideController;
        Assert.That(appliedOverride, Is.Not.Null);

        var appliedClips = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>>();
        appliedOverride.GetOverrides(appliedClips);
        int clipIndex = appliedClips.FindIndex(entry => entry.Key == baseClip);
        Assert.That(clipIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(appliedClips[clipIndex].Value, Is.SameAs(replacementClip));
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null);
        field.SetValue(target, value);
    }

    private static void SetPublicField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
        Assert.That(field, Is.Not.Null);
        field.SetValue(target, value);
    }
}