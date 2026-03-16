using UnityEngine;
using UnityEngine.InputSystem;

public class MovimientoVelocity : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;

    [Header("Debug")]
    public bool mostrarRayo = true;
    public Color colorRayo = Color.red;

    public Camera camaraJugador;

    private Rigidbody rb;
    private Vector2 inputMovimiento;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (camaraJugador == null)
            camaraJugador = Camera.main;
    }

    void OnMove(InputValue value)
    {
        inputMovimiento = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        MoverPersonaje();
        RotarHaciaElMouse();
    }

    void MoverPersonaje()
    {
        Vector3 movimiento = new Vector3(inputMovimiento.x, 0f, inputMovimiento.y).normalized * velocidad;
        movimiento.y = rb.linearVelocity.y;
        rb.linearVelocity = movimiento;
    }

    void RotarHaciaElMouse()
    {
        // Leemos la posicion del mouse directamente, sin depender de OnLook
        Vector2 posicionMouse = Mouse.current.position.ReadValue();

        Ray rayo = camaraJugador.ScreenPointToRay(posicionMouse);
        Plane plano = new Plane(Vector3.up, transform.position);

        if (plano.Raycast(rayo, out float distancia))
        {
            Vector3 puntoEnElMundo = rayo.GetPoint(distancia);

            VisualizarRayo(rayo, puntoEnElMundo);

            Vector3 direccion = puntoEnElMundo - transform.position;
            direccion.y = 0f;

            if (direccion.sqrMagnitude > 0.01f)
            {
                Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
                rb.MoveRotation(rotacionObjetivo);
            }
        }
    }

    void VisualizarRayo(Ray rayo, Vector3 puntoImpacto)
    {
        if (!mostrarRayo) return;

        // Rayo desde la camara hasta el plano
        Debug.DrawLine(rayo.origin, puntoImpacto, colorRayo);

        // Linea desde el jugador hasta el punto en el suelo
        Debug.DrawLine(transform.position, puntoImpacto, Color.green);

        // Pequeña cruz en el punto de impacto
        float tamanioCruz = 0.2f;
        Debug.DrawLine(puntoImpacto + Vector3.left * tamanioCruz,
                       puntoImpacto + Vector3.right * tamanioCruz, Color.yellow);
        Debug.DrawLine(puntoImpacto + Vector3.back * tamanioCruz,
                       puntoImpacto + Vector3.forward * tamanioCruz, Color.yellow);
    }
}