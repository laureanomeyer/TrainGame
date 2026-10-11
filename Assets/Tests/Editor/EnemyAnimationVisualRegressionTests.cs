using System.Reflection;
using UnityEditor;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;

public class EnemyAnimationVisualRegressionTests
{
    [Test]
    public void InitializeVisualsReusesPrefabInstanceAndDeactivatesPreviousVisual()
    {
        GameObject enemyObject = new GameObject("Enemy");
        GameObject visualAnchorObject = new GameObject("Visual Anchor");
        GameObject firstPrefab = new GameObject("First Visual");
        GameObject secondPrefab = new GameObject("Second Visual");
        firstPrefab.SetActive(false);
        secondPrefab.SetActive(false);
        System.Type enemyType = System.Type.GetType("Enemy, Assembly-CSharp", true);
        System.Type dataType = System.Type.GetType("EnemyData, Assembly-CSharp", true);
        System.Type visualType = System.Type.GetType("EnemyVisualSO, Assembly-CSharp", true);
        ScriptableObject data = ScriptableObject.CreateInstance(dataType);
        ScriptableObject visual = ScriptableObject.CreateInstance(visualType);
        SetPublicField(data, "visual", visual);

        try
        {
            Component enemy = enemyObject.AddComponent(enemyType);
            SetPrivateField(enemy, "visualAnchor", visualAnchorObject.transform);
            SetPrivateField(enemy, "data", data);

            SetPublicField(visual, "visualPrefab", firstPrefab);
            enemyType.GetMethod("InitializeVisuals").Invoke(enemy, null);
            GameObject firstInstance = visualAnchorObject.transform.GetChild(0).gameObject;

            SetPublicField(visual, "visualPrefab", secondPrefab);
            enemyType.GetMethod("InitializeVisuals").Invoke(enemy, null);
            GameObject secondInstance = visualAnchorObject.transform.GetChild(1).gameObject;

            SetPublicField(visual, "visualPrefab", firstPrefab);
            enemyType.GetMethod("InitializeVisuals").Invoke(enemy, null);

            Assert.That(visualAnchorObject.transform.childCount, Is.EqualTo(2));
            Assert.That(firstInstance.activeSelf, Is.True);
            Assert.That(secondInstance.activeSelf, Is.False);
        }
        finally
        {
            Object.DestroyImmediate(enemyObject);
            Object.DestroyImmediate(visualAnchorObject);
            Object.DestroyImmediate(firstPrefab);
            Object.DestroyImmediate(secondPrefab);
            Object.DestroyImmediate(data);
            Object.DestroyImmediate(visual);
        }
    }

    [Test]
    public void IsPlayingRecognizesStateUsingItsFullPathHash()
    {
        const string controllerPath = "Assets/Tests/Editor/EnemyAnimationVisualRegression.controller";
        GameObject animatorObject = new GameObject("Animator");
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.layers[0].stateMachine.AddState("Idle");

        try
        {
            Animator animator = animatorObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            int idleFullPathHash = Animator.StringToHash("Base Layer.Idle");
            System.Type animationStateType = System.Type.GetType("AnimationState, Assembly-CSharp", true);
            System.Type animatorBrainType = System.Type.GetType("AnimatorBrain, Assembly-CSharp", true);
            object idle = System.Activator.CreateInstance(animationStateType, idleFullPathHash, false, 1f);
            object brain = System.Activator.CreateInstance(animatorBrainType);
            animatorBrainType.GetMethod("SetIdle").Invoke(brain, new[] { idle });
            animatorBrainType.GetMethod("Init").Invoke(brain, new object[] { animator });
            animator.Update(0f);

            bool isPlaying = (bool)animatorBrainType.GetMethod("IsPlaying").Invoke(brain, new[] { idle });
            Assert.That(isPlaying, Is.True);
        }
        finally
        {
            Object.DestroyImmediate(animatorObject);
            AssetDatabase.DeleteAsset(controllerPath);
        }
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
