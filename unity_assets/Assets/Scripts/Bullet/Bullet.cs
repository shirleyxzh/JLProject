using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime;
    public int damage;
    public float spinSpeed;
    public float scaler = 1f;

    public float speed { get; set; }
    public float timer { get; set; }
    public float rotation { get; set; }
    public Vector3 direction { get; set; }

    private void Start()
    {
        transform.localScale *= scaler;
    }
    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        transform.Rotate(Vector3.forward * 360f * spinSpeed * Time.deltaTime, Space.Self);
        timer -= Time.deltaTime;
        if (timer <= 0) gameObject.SetActive(false);
    }
}
