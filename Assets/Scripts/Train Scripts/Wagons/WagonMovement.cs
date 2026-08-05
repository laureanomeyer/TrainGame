using System.IO;
using UnityEngine;

public class WagonMovement
{
    //Referencias a los transforms
    private Transform tail;
    private Transform objectTransform;
    private Transform targetTail;
    private GameObject wagonBack;
    public Transform Tail => tail;


    public WagonMovement(GameObject wagonBack, Transform tail)
    {
        this.wagonBack = wagonBack;
        this.tail = tail;
        Debug.Log(wagonBack);
    }

    public void Initialize(Transform target, Transform objectTransform) //Setea la cabeza de los vagones
    {
        this.targetTail = target;
        this.objectTransform = objectTransform;
        this.objectTransform.position = target.position;
        this.objectTransform.rotation = target.rotation;
        tail.rotation = target.rotation;
    }

    public void Move()
    {
        if (targetTail == null) return;
        objectTransform.position = targetTail.position; //Se pega a la cola del vagon de adelante
        objectTransform.rotation = Quaternion.Lerp(objectTransform.rotation, targetTail.rotation, 0.3f); //Copia la rotacion de la cola del vagon de adelante en menor escala
        tail.rotation = objectTransform.rotation; //Fija la rotación de su cola
    }

}
