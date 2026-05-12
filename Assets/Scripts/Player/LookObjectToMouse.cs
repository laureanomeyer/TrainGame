using UnityEngine;
using UnityEngine.InputSystem;

public class LookObjectToMouse
{
    private LayerMask groundMask;

    private Camera mainCamera;

    public LookObjectToMouse(LayerMask groundMask)
    {
        this.groundMask = groundMask;
        mainCamera = Camera.main;
    }


    public Vector3 GetMouseDirection(Transform objectTransform)
    {
        var (success, position) = GetMousePosition();

        Vector3 direction = position - objectTransform.position;
        return direction;
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
