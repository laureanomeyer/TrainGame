using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] private Transform finalLoc;
    [SerializeField] private Transform startLoc;
    [SerializeField] private GameObject Tile;

    void Start()
    {
        
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        Tile.transform.position = Vector3.MoveTowards(Tile.transform.position, finalLoc.position, 10 * Time.deltaTime);
        if (Vector3.Magnitude(finalLoc.position - Tile.transform.position) < 0.1f)
        {
            Tile.transform.position = startLoc.position;
        }
    }
}
