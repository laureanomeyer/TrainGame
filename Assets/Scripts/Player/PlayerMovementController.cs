using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;

    private Rigidbody rb;
    private LookObjectToMouse lookToMouseController;
    private Vector2 moveInput;

    private bool canMove = true;
    private bool canRotate = true;

    private void Start()
    {
        lookToMouseController = GetComponent<LookObjectToMouse>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!canMove)
        {
            rb.linearVelocity = Vector3.zero;
        }
        else
        {
            MovePlayer();
        }

        if (!canRotate)
        {
            rb.angularVelocity = Vector3.zero;
            return;
        }

        RotateToMouse();
    }

    private void OnMove(InputValue value)
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
        if (lookToMouseController == null) return;

        Vector3 direction = lookToMouseController.GetMouseDirection(transform);
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        rb.MoveRotation(targetRotation);
    }

    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove)
        {
            moveInput = Vector2.zero;
            rb.linearVelocity = Vector3.zero;
        }
    }

    public void SetCanRotate(bool value)
    {
        canRotate = value;

        if (!canRotate)
        {
            rb.angularVelocity = Vector3.zero;
        }
    }
}