// CopySpriteSlicing.cs
// Copia el "recorte" (slicing) de sprites de una textura de referencia
// hacia varias texturas destino que comparten el mismo layout/tamaño.
//
// Requiere el paquete "2D Sprite" instalado (Window > Package Manager > 2D Sprite).
//
// Instalación:
// 1) Crear una carpeta "Editor" dentro de Assets (si no existe).
// 2) Colocar este archivo dentro de esa carpeta: Assets/Editor/CopySpriteSlicing.cs
// 3) En Unity: Tools > Copy Sprite Slicing
//
// Uso:
// - Arrastrá la textura ya cortada (referencia) en "Reference Texture".
// - Arrastrá todas las texturas a las que querés aplicar el mismo corte en "Target Textures".
// - Click en "Copy Slicing to Targets".

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public class CopySpriteSlicingWindow : EditorWindow
{
    private Texture2D referenceTexture;
    private readonly List<Texture2D> targetTextures = new List<Texture2D>();
    private SerializedObject so;
    private SerializedProperty targetsProp;

    [MenuItem("Tools/Copy Sprite Slicing")]
    private static void Open()
    {
        GetWindow<CopySpriteSlicingWindow>("Copy Sprite Slicing");
    }

    private void OnEnable()
    {
        so = new SerializedObject(this);
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Copia el slicing (rects, nombres, pivot, borders) de una textura de referencia " +
            "a otras texturas con el mismo tamaño/layout de grilla.",
            MessageType.Info);

        EditorGUILayout.Space();
        referenceTexture = (Texture2D)EditorGUILayout.ObjectField(
            "Reference Texture (ya cortada)", referenceTexture, typeof(Texture2D), false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Target Textures", EditorStyles.boldLabel);

        // Drag & drop area
        Rect dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Arrastrá acá las texturas destino");
        HandleDragAndDrop(dropArea);

        for (int i = 0; i < targetTextures.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            targetTextures[i] = (Texture2D)EditorGUILayout.ObjectField(
                targetTextures[i], typeof(Texture2D), false);
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                targetTextures.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Clear list"))
            targetTextures.Clear();

        EditorGUILayout.Space();
        GUI.enabled = referenceTexture != null && targetTextures.Count > 0;
        if (GUILayout.Button("Copy Slicing to Targets", GUILayout.Height(30)))
        {
            CopySlicing();
        }
        GUI.enabled = true;
    }

    private void HandleDragAndDrop(Rect dropArea)
    {
        Event evt = Event.current;
        if (!dropArea.Contains(evt.mousePosition)) return;

        if (evt.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.Use();
        }
        else if (evt.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is Texture2D tex && !targetTextures.Contains(tex))
                    targetTextures.Add(tex);
            }
            evt.Use();
        }
    }

    private void CopySlicing()
    {
        string refPath = AssetDatabase.GetAssetPath(referenceTexture);
        var refImporter = AssetImporter.GetAtPath(refPath) as TextureImporter;
        if (refImporter == null)
        {
            Debug.LogError("No se pudo obtener el TextureImporter de la referencia.");
            return;
        }

        var factories = new SpriteDataProviderFactories();
        factories.Init();

        var refDataProvider = factories.GetSpriteEditorDataProviderFromObject(referenceTexture);
        refDataProvider.InitSpriteEditorDataProvider();
        SpriteRect[] refRects = refDataProvider.GetSpriteRects();

        if (refRects == null || refRects.Length == 0)
        {
            Debug.LogError("La textura de referencia no tiene sprites cortados (slice).");
            return;
        }

        int successCount = 0;

        foreach (var targetTex in targetTextures)
        {
            if (targetTex == null) continue;

            string targetPath = AssetDatabase.GetAssetPath(targetTex);
            var targetImporter = AssetImporter.GetAtPath(targetPath) as TextureImporter;
            if (targetImporter == null)
            {
                Debug.LogWarning($"Salteando {targetPath}: no es una textura importable.");
                continue;
            }

            // Aseguramos modo Multiple para que acepte varios sprites
            targetImporter.textureType = TextureImporterType.Sprite;
            targetImporter.spriteImportMode = SpriteImportMode.Multiple;

            var targetDataProvider = factories.GetSpriteEditorDataProviderFromObject(targetTex);
            targetDataProvider.InitSpriteEditorDataProvider();

            // Clonamos los rects de referencia con GUIDs nuevos (uno por textura)
            var newRects = new List<SpriteRect>();
            foreach (var r in refRects)
            {
                var clone = new SpriteRect
                {
                    name = r.name,
                    rect = r.rect,
                    alignment = r.alignment,
                    pivot = r.pivot,
                    border = r.border,
                    spriteID = GUID.Generate()
                };
                newRects.Add(clone);
            }

            targetDataProvider.SetSpriteRects(newRects.ToArray());

            // Copiamos también los nombres asociados al name file id provider (necesario en Unity moderno)
            var nameFileIdProvider = targetDataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (nameFileIdProvider != null)
            {
                var pairs = newRects
                    .Select(r => new SpriteNameFileIdPair(r.name, r.spriteID))
                    .ToList();
                nameFileIdProvider.SetNameFileIdPairs(pairs);
            }

            targetDataProvider.Apply();

            targetImporter.SaveAndReimport();
            successCount++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"Slicing copiado a {successCount} textura(s).");
    }
}
