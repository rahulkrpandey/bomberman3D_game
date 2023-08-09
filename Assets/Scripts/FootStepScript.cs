using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStepScript : MonoBehaviour
{
    private float dur;
    private float time;
    private SpriteRenderer sr;

    private void Awake()
    {
        dur = 15;
        time = 0;

        sr = transform.GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (time > dur) {
            time = 0;
            Destroy(gameObject);
        } else {
            time += Time.deltaTime;
            float _a = sr.color.a;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, _a - (_a * time / dur));
        }
    }
}
