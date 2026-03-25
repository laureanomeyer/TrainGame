using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemyAI: se mueve hacia el vagón más cercano usando Rigidbody.
/// No requiere NavMesh. Ideal para escenas sin obstáculos.
///
/// Setup:
///   1. Asignar el tag "Wagon" a cada vagón.
///   2. Adjuntar este script al enemigo.
///   3. El enemigo necesita un Rigidbody con:
///        - Constraints: Freeze Rotation X, Y, Z  (para que no se vuelque)
///        - Use Gravity: según necesites
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detección")]
    public string wagonTag = "Train";
    public float targetUpdateInterval = 0.3f;

    [Header("Movimiento")]
    public float moveSpeed = 20f;

    [Header("vida")]
    public float vida = 20f;

    [Header("Combate")]
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;

    // ── Estado interno ──────────────────────────────────────────────────────
    private Rigidbody rb;
    private GameObject currentTarget;
    private float lastAttackTime;
    private float lastTargetCheckTime;

    private List<GameObject> wagons = new List<GameObject>();
    private float cacheRefreshInterval = 2f;
    private float lastCacheRefreshTime = -999f;

    // ── Ciclo de vida ────────────────────────────────────────────────────────

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Time.time >= lastCacheRefreshTime + cacheRefreshInterval)
            RefreshWagonCache();

        if (wagons.Count == 0)
        {
            StopMovement();
            return;
        }

        if (Time.time >= lastTargetCheckTime + targetUpdateInterval)
        {
            lastTargetCheckTime = Time.time;
            UpdateTarget();
        }

        if (currentTarget == null)
        {
            StopMovement();
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.transform.position);

        MoveTowardsTarget();

        /*
        if (dist <= attackRange)
        {
            StopMovement();
            TryAttack();
        }
        else
        {
        }
        */
    }

    // ── Movimiento con Rigidbody ─────────────────────────────────────────────

    void MoveTowardsTarget()
    {
        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;

        // Preservar la velocidad vertical (gravedad) y solo mover en XZ
        Vector3 velocity = direction * moveSpeed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;

        // Rotar hacia el objetivo suavemente
        Vector3 lookDir = direction;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(lookDir),
                Time.deltaTime * 1f
            );
    }

    void StopMovement()
    {
        // Detener solo en XZ, mantener Y para que la gravedad siga funcionando
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
    }

    // ── Detección por Tag ────────────────────────────────────────────────────

    void RefreshWagonCache()
    {
        lastCacheRefreshTime = Time.time;
        wagons.Clear();
        wagons.AddRange(GameObject.FindGameObjectsWithTag(wagonTag));
    }

    void UpdateTarget()
    {
        GameObject nearest = FindNearestWagon();
        if (nearest == null) return;

        if (nearest != currentTarget)
        {
            currentTarget = nearest;
        }
    }

    GameObject FindNearestWagon()
    {
        GameObject nearest = null;
        float shortestDistance = Mathf.Infinity;

        for (int i = wagons.Count - 1; i >= 0; i--)
        {
            if (wagons[i] == null || !wagons[i].activeInHierarchy)
            {
                wagons.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(transform.position, wagons[i].transform.position);
            if (dist < shortestDistance)
            {
                shortestDistance = dist;
                nearest = wagons[i];
            }
        }

        return nearest;
    }

    // ── Ataque ───────────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("deadWall"))
        {
            Dead();
        }
        if (other.gameObject.CompareTag("bullet"))
        {
            TakeDamage(10);
        }
        if (other.gameObject.CompareTag("Train"))
        {
            other.TryGetComponent<WagonBrain>(out WagonBrain wagonBrain);
            if (wagonBrain != null)
            wagonBrain.TakeDamage(10);
            other.TryGetComponent<LocomotiveBrain>(out LocomotiveBrain locomotiveBrain);
            if(locomotiveBrain != null)
            locomotiveBrain.TakeDamage(10);
        }
    }

    // ── Gizmos ───────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }

    public void TakeDamage(int damage)
    {
        vida -= damage;

        if (vida <= 0)
        {
            Dead();
        }
    }

    public void Dead()
    {
        Destroy(this.gameObject);
    }
}