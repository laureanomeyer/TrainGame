using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 5f;

    private Rigidbody rb;
    private LookObjectToMouse lookToMouseController;
    private Vector2 moveInput;

    void Start()
    {
        lookToMouseController = GetComponent<LookObjectToMouse>();
        rb = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        MovePlayer();
        RotateToMouse();
    }


    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void MovePlayer()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized * speed;
        movement.y = rb.linearVelocity.y;
        rb.linearVelocity = movement;
    }

    private void RotateToMouse()
    {
        var direction = lookToMouseController.GetMouseDirection(transform);
        direction.y = 0f;
        transform.forward = direction;
    }

}