using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] Renderer rend;
    [SerializeField] Color flashColor;
    [SerializeField] float flashDuration;

    private Color originalColor;
    void Start()
    {
        originalColor = rend.material.color;
    }

    private IEnumerator DoFlash()
    {
        rend.material.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        rend.material.color = originalColor;
    }

    public void Flash()
    {
        StopAllCoroutines();
        StartCoroutine(DoFlash());
    }


}
