using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Renderer[] rend;
    [SerializeField] private Material flashMaterial;
    [SerializeField] private float flashDuration;

    private Material[][] originalMaterials;
    private Material[][] flashMaterials;
    private WaitForSeconds flashWait;

    void Awake()
    {
        flashWait = new WaitForSeconds(flashDuration);

        originalMaterials = new Material[rend.Length][];
        flashMaterials = new Material[rend.Length][];

        for (int i = 0; i < rend.Length; i++)
        {
            originalMaterials[i] = rend[i].sharedMaterials;
            BuildFlashArray(i);
        }
    }

    private void BuildFlashArray(int index)
    {
        int count = originalMaterials[index].Length;
        var arr = new Material[count];
        for (int j = 0; j < count; j++)
            arr[j] = flashMaterial;
        flashMaterials[index] = arr;
    }

    private IEnumerator DoFlash()
    {
        for (int i = 0; i < rend.Length; i++)
            rend[i].sharedMaterials = flashMaterials[i];

        yield return flashWait;

        ResetMaterials();
    }

    public void Flash()
    {
        if (!gameObject.activeInHierarchy) return;
        StopAllCoroutines();
        StartCoroutine(DoFlash());
    }

    public void StopCoroutine()
    {
        StopAllCoroutines();
    }

    public void SetMaterialSingle(Material material)
    {
        originalMaterials[0][0] = material;
    }

    public void SetMaterialArray(int rendererIndex, Material[] materials)
    {
        if (rendererIndex < 0 || rendererIndex >= originalMaterials.Length) return;

        originalMaterials[rendererIndex] = materials;
        BuildFlashArray(rendererIndex); 
    }

    public void ResetMaterials()
    {
        for (int i = 0; i < rend.Length; i++)
            rend[i].sharedMaterials = originalMaterials[i];
    }
}
