using System.Collections;
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
        originalMaterials = new Material[rend.Length][];
        for (int i = 0; i < rend.Length; i++)
            originalMaterials[i] = rend[i].materials;
    }

    private IEnumerator DoFlash()
    {
        for (int i = 0; i < rend.Length; i++)
        {
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
        StopAllCoroutines();
        StartCoroutine(DoFlash());
    }
    public void SetMaterialSingle(Material material)
    {
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
            rend[i].materials = originalMaterials[i];
    }
}
