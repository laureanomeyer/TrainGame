using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Renderer[] rend;
    [SerializeField] private Material flashMaterial;
    [SerializeField] private float flashDuration;

    private Material[] originalMaterials;

    void Awake()
    {
        originalMaterials = new Material[rend.Length];
        for (int i = 0; i < rend.Length; i++)
            originalMaterials[i] = rend[i].material;
    }

    private IEnumerator DoFlash()
    {
        for (int i = 0; i < rend.Length; i++)
            rend[i].material = flashMaterial;

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < rend.Length; i++)
            rend[i].material = originalMaterials[i];
    }

    public void Flash()
    {
        StopAllCoroutines();
        StartCoroutine(DoFlash());
    }
    public void SetMaterialSingle(Material material)
    {
        originalMaterials[0] = material;
    }
    public void SetMaterialArray(Material[] materials) 
    { 
        originalMaterials = materials;
    }
}
