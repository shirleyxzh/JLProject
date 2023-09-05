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
    public int damage;
    public float scaler = 1f;

    private void Start()
    {
        transform.localScale *= scaler;
    }
    void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;
        timer -= Time.deltaTime;
        if (timer <= 0) gameObject.SetActive(false);
    }
}
