using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeCamera : MonoBehaviour
{
    public AnimationCurve curve;
    private bool ismoving;
    private GameObject bomberman;
    private Vector3 initialPos, finalPos;
    private bool isMoving;

    private void Awake()
    {
        ismoving = false;
        initialPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        finalPos = new Vector3(transform.position.x, transform.position.y, -1);
        isMoving = false;
    }

    private void Start()
    {
        //bomberman = FindAnyObjectByType<GameObject>(CompareTag("bomberman"));
        bomberman = GameObject.FindGameObjectWithTag("bomberman");
    }

    private void FixedUpdate()
    {
       if (isMoving || bomberman == null) {
            return;
        }

       if (transform.position == initialPos && bomberman.transform.position.z < 1) {
            StartCoroutine(Move(initialPos, finalPos));
       } else if (transform.position == finalPos && bomberman.transform.position.z >= 1) {
            StartCoroutine(Move(finalPos, initialPos));
       }
    }

    private void Shake(Vector2Int _)
    {
        if (ismoving) {
            return;
        }

        StartCoroutine(ShakeUtil());
    }

    IEnumerator Move(Vector3 start, Vector3 end) {
        isMoving = true;
        float dur = 1, time = 0;

        while (time < dur) {
            time += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, time / dur);
            yield return null;
        }

        isMoving = false;
    }

    IEnumerator ShakeUtil() {
        ismoving = true;
        Vector3 curr = transform.position;

        float timeToShake = 0.5f, time =  0;
        while (time < timeToShake) {
            time += Time.deltaTime;
            float strength = curve.Evaluate(time / timeToShake);
            transform.position = curr+ Random.insideUnitSphere * strength;
            yield return null;
        }

        transform.position = curr;
        ismoving = false;
    }

    private void OnEnable()
    {
        GameEvents.OnDestroyBomb += Shake;
        GameEvents.OnDestroyBomberman += BombermanHandler;
    }

    private void OnDisable()
    {
        GameEvents.OnDestroyBomb -= Shake;
        GameEvents.OnDestroyBomberman -= BombermanHandler;
    }

    private void BombermanHandler() {
        bomberman = null;
    }

}
