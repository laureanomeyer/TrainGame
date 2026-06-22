using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 80f;

    private Rigidbody rb;
    private LookObjectToMouse lookToMouseController;
    private Transform playerTransform;


    private Vector2 moveInput;
    private bool canMove = true;
    private bool canRotate = true;

    public bool CanMove => canMove;

    public PlayerMovementController(Rigidbody rb, LookObjectToMouse lookToMouseController, Transform playerTransform, float speed)
    {
        this.rb = rb;
        this.lookToMouseController = lookToMouseController;
        this.playerTransform = playerTransform;
        this.speed = speed;
    }

    public void FixedUpdate()
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

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    private void MovePlayer()
    {
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        Vector3 targetVelocity = inputDirection * speed;

        Vector3 currentHorizontalVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        float currentAcceleration = inputDirection.sqrMagnitude > 0.001f
            ? acceleration //lo dejo comentado porque esta medio raro jajaja pero si inputDirection tiene dirección, osea estás apretando W, usa acceleration.
            : deceleration; //Si inputDirection es Vector3.zero, porque soltaste WASD, usa deceleration

        Vector3 smoothHorizontalVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity,
            targetVelocity,
            currentAcceleration * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector3(
            smoothHorizontalVelocity.x,
            rb.linearVelocity.y,
            smoothHorizontalVelocity.z
        );
    }

    /*Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized * speed;
    movement.y = rb.linearVelocity.y;
    rb.linearVelocity = movement;*/

    private void RotateToMouse()
    {
        if (lookToMouseController == null) return;

        Vector3 direction = lookToMouseController.GetMouseDirection(playerTransform);
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