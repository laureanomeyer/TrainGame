using UnityEngine;

public class WagonRenderController
{
    private WagonBrain wagonBrain;
    private Mesh wagonTopMesh;

    public WagonRenderController (WagonBrain brain)
    {
        wagonBrain = brain;

        if (wagonBrain.topMeshFilterWagon != null)
        {
            wagonTopMesh = wagonBrain.topMeshFilterWagon.mesh;
        }
    }

    public void CheckWagonToChangeRender(bool canBreak)
    {
        if (canBreak)
        {
            ChangeToDestroyWagon();
        }
        else
        {
            ChangeToDestroyWagonFloor();
        }
    }

    private void ChangeToDestroyWagon()
    {
        //Change floor
        if (wagonBrain.floorMeshFilterWagon != null)
        {
            wagonBrain.floorMeshFilterWagon.mesh = wagonBrain.floorMeshDestroyWagon;
            wagonBrain.floorRenderWagon.material = wagonBrain.destroyWagonMaterial;
        }

        //Change body
        if (wagonBrain.bodyMeshFilterWagon != null)
        {
            wagonBrain.bodyMeshFilterWagon.mesh = wagonBrain.bodyMeshDestroyWagon;
            wagonBrain.bodyRenderWagon.material = wagonBrain.destroyWagonMaterial;
        }

        //Change top
        if (wagonBrain.topMeshFilterWagon != null)
        {
            wagonBrain.topMeshFilterWagon.mesh = null;
            wagonBrain.topRenderWagon.material = null;
        }

        if (wagonBrain.particles != null)
        {
            wagonBrain.particles.Play();
        }

        //Change extra elements in wagon
    }

    private void ChangeToDestroyWagonFloor()
    {
        //Change floor
        if (wagonBrain.floorMeshFilterWagon != null)
        {
            wagonBrain.floorMeshFilterWagon.mesh = wagonBrain.floorMeshDestroyWagon;
            wagonBrain.floorRenderWagon.material = wagonBrain.destroyWagonMaterial;
        }
    }

    public void SetWagonMeshAndMaterial(Mesh floor, Mesh body)
    {
        wagonBrain.floorMeshFilterWagon.mesh = floor;
        wagonBrain.bodyMeshFilterWagon.mesh = body;

        wagonBrain.floorRenderWagon.material = wagonBrain.destroyWagonMaterial;
        wagonBrain.bodyRenderWagon.material = wagonBrain.destroyWagonMaterial;
    }

    public void ActivateWagonTop()
    {
        if (wagonBrain.topMeshFilterWagon != null && wagonBrain.Broken == false)
        {
            wagonBrain.topMeshFilterWagon.mesh = wagonTopMesh;
        }
    }

    public void DeactivateWagonTop()
    {
        if (wagonBrain.topMeshFilterWagon != null)
        {
            wagonBrain.topMeshFilterWagon.mesh = null;
        }
    }
}
