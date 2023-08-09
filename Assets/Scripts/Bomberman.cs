using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Bomberman : MonoBehaviour
{
    private CustomMovement input;
    private int x;
    private int z;
    private bool isMoving;
    private Vector3 rotationAngles;
    public Animator anim;
    public GameObject bomb;
    public FloatingJoystick joystick;

    private int maxBombs;
    private int spawnedBombs;
    private Vector3 position, rotation, scale;

    private float time;
    public float dur;
    public GameObject footStep;

    private void Awake()
    {
        x = z = 1;
        time = 0;
        isMoving = false;
        maxBombs = 1;
        spawnedBombs = 0;
        input = new CustomMovement();
        position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        rotation = new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z);
        scale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    private void Start()
    {
        joystick = FindFirstObjectByType<FloatingJoystick>();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Bomberman.Movement.performed += OnMovementPerformed;
        input.Bomberman.Attack.performed += InstantiateBomb;
        GameEvents.OnRequest += InstantiateBombHandler;
        GameEvents.OnDestroyBomb += SelfDestroyHandler;
    }

    private void OnDisable()
    {
        input.Bomberman.Movement.performed -= OnMovementPerformed;
        input.Disable();
        GameEvents.OnDestroyBomb -= SelfDestroyHandler;
        GameEvents.OnRequest -= InstantiateBombHandler;
        GameEvents.OnDestroyBombermanInvoke();
        SoundManagement.sm.PlayHurt();
    }

    private void SelfDestroyHandler(Vector2Int cord) {
        if (!Grid.grid.SafeFromBomb(cord, new Vector2Int(x, z))) {
            //Debug.Log("Bomberman Destroyed");
            Destroy(gameObject);
        } else {
            spawnedBombs--;
        }
    }

    private void FixedUpdate()
    {
        anim.SetBool("Walk", isMoving);
        anim.SetBool("Idle", !isMoving);

        int _x = Mathf.RoundToInt(joystick.Horizontal);
        int _y = Mathf.RoundToInt(joystick.Vertical);

        if (_y == 1 || _y == -1) {
            _x = 0;
        }

        Vector2 val = new(_x, _y);
        if (_x != 0 || _y != 0) {
            OnMovementPerformedUtil(val);
	    }


    }

    private void Update()
    {
        //if (isMoving) {
        //    if (time >= dur) {
        //        time = 0;
        //        SoundManagement.sm.PlayFootStep();
        //    }
        //    time += Time.deltaTime;
        //} else {
        //    time = 0;
        //}
    }

    private void InstantiateBomb(InputAction.CallbackContext _v) {
        InstantiateBombHandler();
    }

    public void InstantiateBombHandler() {
        if (spawnedBombs == maxBombs)
        {
            return;
        }

        //Debug.Log("Called");
        spawnedBombs++;
        Instantiate(bomb, transform.position, transform.rotation);
    }

    private void OnMovementPerformed(InputAction.CallbackContext _v) {
        Vector2 val = _v.ReadValue<Vector2>();
        OnMovementPerformedUtil(val);
    }

    private void OnMovementPerformedUtil(Vector2 val) {
        Vector2Int temp = new(Mathf.RoundToInt(val.x), Mathf.RoundToInt(val.y));
        //Debug.Log(temp);

        Vector2Int newPos = new(x - temp.y, z + temp.x);
        if (!isMoving && Grid.grid.CanMove(newPos)) {
            Vector3 dest = Grid.grid.MatToWorldPos(newPos.x, newPos.y);
            float y_rotation = 0;
            if (temp.x == -1) {
                rotationAngles.y = y_rotation = -90;
            } else if (temp.x == 1) {
                rotationAngles.y = y_rotation = 90;
            } else if (temp.y == 1) {
                rotationAngles.y = y_rotation = 0;
            } else {
                rotationAngles.y = y_rotation = 180;
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
        SoundManagement.sm.PlayFootStep();
        //Vector3 end = transform.position + dir, start = transform.position;
        Vector3 end = dest, start = transform.position;
        end.y = 0;
        float timeElapsed = 0;
        float timeToMove = 0.2f;


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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("enemy")) {
            Destroy(gameObject);
        } else if (collision.collider.CompareTag("powerUp")) {
            //Debug.Log("Collected powerup");
            GameEvents.OnPowerUpCollectedInvoke();
            SoundManagement.sm.PlayCollectible();
            Destroy(collision.collider.transform.gameObject);
            maxBombs++;
        } else if (collision.collider.CompareTag("door")) {
            gameObject.transform.position = position;
            gameObject.transform.rotation = Quaternion.Euler(rotation);
            gameObject.transform.localScale = scale;
            SoundManagement.sm.PlayCollectible();
            GameEvents.OnGateEnterInvoke();
        }
    }

    private void InstantiateFootSteps(float y) {
        var obj = Instantiate(footStep, new Vector3(transform.position.x, 0.1f, transform.position.z), transform.rotation);
        obj.transform.rotation = Quaternion.Euler(90, y, 0);
    }
}
