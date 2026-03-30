using System.IO;
using UnityEngine;

public class WagonMovement : MonoBehaviour, IWagon
{
    //Referencias a los transforms
    [SerializeField] private Transform tail;
    private Transform targetTail;
    public Transform Tail => tail;

    [SerializeField] private float speed;

    [SerializeField] public GameObject wagonBack;

    public void Initialize(Transform target) //Setea la cabeza de los vagones
    {
        this.targetTail = target;
        transform.position = target.position;
        transform.rotation = target.rotation;
        tail.rotation = target.rotation;
    }
    
    void LateUpdate()
    {
        Move();
    }

    void Move()
    {
        if (targetTail == null) return;   
        transform.position = targetTail.position; //Se pega a la cola del vagon de adelante
        transform.rotation = Quaternion.Lerp(transform.rotation, targetTail.rotation, 0.07f); //Copia la rotacion de la cola del vagon de adelante en menor escala
        tail.rotation = transform.rotation; //Fija la rotación de su cola

    }

}
