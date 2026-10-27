using UnityEngine;

public class MapChangingTile : MapTile, TileChange
{
    [SerializeField] private Mesh[] variatons;

    public void SetUpWithMesh(Transform followTransform)
    {
        head = followTransform;
        isMapHead = false;

        Mesh newMesh = (variatons[Random.Range(0, variatons.Length)]);

        if (newMesh != null)
            tileMesh.mesh = newMesh;
    }
}
