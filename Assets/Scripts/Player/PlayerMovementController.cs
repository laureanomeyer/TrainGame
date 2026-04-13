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

    private bool canMove = true;

    void Start()
    {
        lookToMouseController = GetComponent<LookObjectToMouse>();
        rb = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

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

    public void SetCanMove(bool value)
    {
        canMove = value;

        if(!canMove)
        {
            moveInput = Vector2.zero;
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

}