using UnityEngine;
using UnityEngine.InputSystem;

public class LookObjectToMouse : MonoBehaviour
{
    [Header("Aim")]
    [SerializeField] private LayerMask groundMask;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public Vector3 GetMouseDirection(Transform objectTransform)
    {
        var (success, position) = GetMousePosition();
        if (success)
        {

            Vector3 direction = position - objectTransform.position;

            return direction;
        }
        else
        {
            return Vector3.zero;
        }
    }

    private (bool success, Vector3 position) GetMousePosition()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        var ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, groundMask))
        {
            return (success: true, position: hitInfo.point);
        }
        else
        {
            return (success: false, position: Vector3.zero);
        }
    }
}
