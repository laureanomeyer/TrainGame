using System.Collections.Generic;
using UnityEngine;

public class BulletFactory : MonoBehaviour
{
    [Header("Bullet Spawn")]
    [SerializeField] private GameObject[] bullets;
    private Dictionary<string, GameObject> obstaclesDictionary;

    private void Start()
    {
        obstaclesDictionary = new Dictionary<string, GameObject>();

        foreach (var gameObject in bullets)
        {
            obstaclesDictionary.Add(gameObject.GetComponent<IBullet>().id, gameObject);
        }
    }

    public GameObject Create(string name)
    {
        if (!obstaclesDictionary.TryGetValue(name, out GameObject bullet))
        {
            Debug.Log("No se encontro la bala indicada");
            return null;
        }

        return Instantiate(bullet);
    }
}
