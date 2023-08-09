using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoftTile : MonoBehaviour
{
    private Vector2Int cord;
    public ParticleSystem p;

    private void Start()
    {
        cord = Grid.grid.PosToMat(transform.position);
    }

    private void OnEnable()
    {
        GameEvents.OnDestroyBomb += SelfDestroyHandler;
    }

    private void OnDisable()
    {
        GameEvents.OnDestroyBomb -= SelfDestroyHandler;
        Grid.grid.mat[cord.x, cord.y] = 0;
    }

    private void SelfDestroyHandler(Vector2Int cord) {
        if (!Grid.grid.SafeFromBomb(cord, this.cord)) {
            //Debug.Log("Tile Destroyed");
            GameEvents.OnDestroyTileInvoke();
            Instantiate(p, transform.position, transform.rotation);
            gameObject.SetActive(false);
        }
    }
}
