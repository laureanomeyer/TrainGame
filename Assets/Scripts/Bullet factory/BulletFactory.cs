using System.Collections.Generic;
using UnityEngine;

public class BulletFactory : MonoBehaviour
{
    //Balas que puede crear la factory
    [Header("Bullet Spawn")]
    [SerializeField] private GameObject[] bullets;

    //Diccionario utilizado para llamar a los objetos que quieren se creado por nombre
    private Dictionary<string, GameObject> obstaclesDictionary;

    private void Awake()
    {
        obstaclesDictionary = new Dictionary<string, GameObject>();

        //Se agregan todos los objetos de la lista en el diccionario
        foreach (var gameObject in bullets)
        {
            obstaclesDictionary.Add(gameObject.GetComponent<IBullet>().id, gameObject);
        }
    }

    //Funcion para crear objectos
    public GameObject Create(string name)
    {
        if (!obstaclesDictionary.TryGetValue(name, out GameObject bullet))
        {
            //Debug.Log("No se encontro la bala indicada");
            return null;
        }

        return Instantiate(bullet);
    }
}
