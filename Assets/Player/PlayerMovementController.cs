using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 5f;

    [Header("Debug")]
    public bool showRay = true;
    public Color rayColor = Color.red;

    public Camera playerCamera;

    private Rigidbody rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (playerCamera == null)
            playerCamera = Camera.main;
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
        // Leemos la posicion del mouse directamente, sin depender de OnLook
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Ray ray = playerCamera.ScreenPointToRay(mousePosition);
        Plane plane = new Plane(Vector3.up, transform.position);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPoint = ray.GetPoint(distance);

            ShowRay(ray, worldPoint);

            Vector3 direction = worldPoint - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion rotation = Quaternion.LookRotation(direction);
                rb.MoveRotation(rotation);
            }
        }
    }

    private void ShowRay(Ray ray, Vector3 collisionPoint)
    {
        if (!showRay) return;

        // Rayo desde la camara hasta el plano
        Debug.DrawLine(ray.origin, collisionPoint, rayColor);

        // Linea desde el jugador hasta el punto en el suelo
        Debug.DrawLine(transform.position, collisionPoint, Color.green);

    }
}