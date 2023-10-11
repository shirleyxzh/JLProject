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
    public float rotation { get; set; }
    public Vector3 direction { get; set; }
    public ParticleSystem hitVFX { get; set; }

    private float timer;
    private bool removing;
    private Vector3 fullScale;

    private void Start()
    {
        fullScale = transform.localScale * scaler;
        transform.localScale = Vector3.zero;
    }
    private void OnEnable()
    {
        timer = 0;
        removing = false;
    }
    private void OnDisable()
    {
        transform.localScale = Vector3.zero;
    }
    void Update()
    {        
        if (removing)
        {
            timer -= Time.deltaTime;
            var timeStep = Mathf.Clamp01(timer / 0.1f);
            transform.localScale = fullScale * timeStep;
            transform.position += direction * speed * Time.deltaTime * timeStep;
            if (timeStep == 0)
                gameObject.SetActive(false);
        }
        else
        {
            timer += Time.deltaTime;
            transform.position += direction * speed * Time.deltaTime;
            transform.localScale = fullScale * Mathf.Clamp01(timer / 0.25f);
            transform.Rotate(Vector3.forward * 360f * spinSpeed * Time.deltaTime, Space.Self);

            if (timer >= lifeTime)
                gameObject.SetActive(false);
        }
    }

    public void RemoveWithVFX(Vector3 pos)
    {
        timer = 0.1f;
        removing = true;
        hitVFX.Emit(
            new ParticleSystem.EmitParams
            {
                position = pos + Vector3.back * 0.1f,
                applyShapeToPosition = true
            },
            1
        );
    }
}
