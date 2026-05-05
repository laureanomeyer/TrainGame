using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorAmmo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image cursor;
    [SerializeField] private Vector3 offset;

    private void Start()
    {
        GameEvents.OnAmmoChanged += UpdateText;
        
    }
    private void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        ammoText.transform.position = mousePos + (Vector2)offset;
        cursor.transform.position = mousePos ;
    }

    void UpdateText(float currentAmmo)
    {
        ammoText.text = $"{currentAmmo}";
    }

    private void OnDestroy()
    {
        GameEvents.OnAmmoChanged -= UpdateText;
    }
}
