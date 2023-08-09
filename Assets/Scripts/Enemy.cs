using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private int x;
    private int z;
    private bool isMoving;
    int[,] move;
    private Vector3 rotationAngles;
    System.Random rnd;

    public GameObject footStep;

    private void Awake()
    {
        x = z = 1;
        isMoving = false;
        move = new int[,] { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } };
        rnd = new System.Random();
    }

    private void Start()
    {
        Vector2Int pos = Grid.grid.GetCoordinate();
        //Debug.Log($"pos is {pos}");
        x = pos.x;
        z = pos.y;
    }

    private void OnEnable()
    {
        GameEvents.OnDestroyBomb += SelfDestroyHandler;
    }

    private void OnDisable()
    {
        GameEvents.OnDestroyBomb -= SelfDestroyHandler;
        GameEvents.OnDestroyEnemyInvoke();
        SoundManagement.sm.PlayHurt();
    }

    private void SelfDestroyHandler(Vector2Int cord) {
        if (!Grid.grid.SafeFromBomb(cord, new Vector2Int(x, z))) {
            //Debug.Log("Enemy Destroyed");
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        int i = rnd.Next(0, 4);
        Vector2Int next = new (move[i, 0], move[i, 1]);
        OnMovementPerformed(next);
    }

    private void OnMovementPerformed(Vector2Int temp) {
        Vector2Int newPos = new(x + temp.x, z + temp.y);
        if (!isMoving && Grid.grid.CanMove(newPos)) {
            Vector3 dest = Grid.grid.MatToWorldPos(newPos.x, newPos.y);
            float y_rotation = 0;
            if (temp.y == 1) {
                rotationAngles.y = y_rotation = 90;
            } else if (temp.y == -1) {
                rotationAngles.y = y_rotation = -90;
            } else if (temp.x == 1) {
                rotationAngles.y = y_rotation = 180;
            } else {
                rotationAngles.y = y_rotation = 0;
            }

            InstantiateFootSteps(y_rotation);
            StartCoroutine(Move(dest));
            x = newPos.x;
            z = newPos.y;
        }
    }

    private IEnumerator Move(Vector3 dest)
    {
        isMoving = true;
        //Vector3 end = transform.position + dir, start = transform.position;
        Vector3 end = dest, start = transform.position;
        end.y = 0.95f;
        float timeElapsed = 0;
        float timeToMove = 0.8f;


        while (timeElapsed < timeToMove)
        {
            transform.position = Vector3.Lerp(start, end, timeElapsed / timeToMove);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rotationAngles), timeElapsed / timeToMove);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
        isMoving = false;
    }

    private void InstantiateFootSteps(float y) {
        var obj = Instantiate(footStep, new Vector3(transform.position.x, 0.1f, transform.position.z), transform.rotation);
        obj.transform.rotation = Quaternion.Euler(90, y, 0);
    }
}
