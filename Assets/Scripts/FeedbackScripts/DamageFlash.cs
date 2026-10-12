using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Renderer[] rend;
    [SerializeField] private Material flashMaterial;
    [SerializeField] private float flashDuration;

    private Material[][] originalMaterials;

    void Awake()
    {
        SetRenderers(rend);
    }

    public void SetRenderers(Renderer[] renderers)
    {
        List<Renderer> validRenderers = new List<Renderer>();
        if (renderers != null)
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    validRenderers.Add(renderer);
                }
            }
        }

        rend = validRenderers.ToArray();
        originalMaterials = new Material[rend.Length][];
        for (int i = 0; i < rend.Length; i++)
            originalMaterials[i] = rend[i].materials;
    }

    private IEnumerator DoFlash()
    {
        for (int i = 0; i < rend.Length; i++)
        {
            if (rend[i] == null)
            {
                continue;
            }

            Material[] flashArray = new Material[rend[i].materials.Length];
            for (int j = 0; j < flashArray.Length; j++)
                flashArray[j] = flashMaterial;

            rend[i].materials = flashArray;
        }
        yield return new WaitForSeconds(flashDuration);

        ResetMaterials();
    
    }

    public void Flash()
    {
        if (gameObject.activeSelf == false) return;
        StopCoroutine();
        StartCoroutine(DoFlash());
    }
    public void StopCoroutine() 
    {
        StopAllCoroutines();
    }

    public void SetMaterialSingle(Material material)
    {
        if (originalMaterials.Length > 0 && originalMaterials[0].Length > 0)
            originalMaterials[0][0] = material;
    }
    public void SetMaterialArray(int rendererIndex, Material[] materials)
    {
        if (rendererIndex >= 0 && rendererIndex < originalMaterials.Length)
            originalMaterials[rendererIndex] = materials;
    }
    public void ResetMaterials()
    {
        for (int i = 0; i < rend.Length; i++)
        {
            if (rend[i] != null)
                rend[i].materials = originalMaterials[i];
        }
    }
}
