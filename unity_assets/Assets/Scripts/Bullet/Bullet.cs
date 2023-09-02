using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public float rotation;
    public float lifeTime;
    public string type;
    public float timer;
    private Vector3 forward;
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        timer -= Time.deltaTime;
        if (timer <= 0) gameObject.SetActive(false);
    }
}
