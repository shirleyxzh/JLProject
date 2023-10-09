using UnityEngine;
using System.Collections.Generic;
public class BulletManager : MonoBehaviour
{
    public static GameObject bulletDump;
    public static List<GameObject> bullets;
    public static ParticleSystem hitVFX;

    private void Start()
    {
        bulletDump = this.gameObject;
        bullets = new List<GameObject>();
        hitVFX = GetComponentInChildren<ParticleSystem>();
    }
    public static GameObject GetBulletFromPool()
    {
        for (int i = 0; i < bullets.Count; i++)
        {
            if (!bullets[i].activeSelf)
            {
                bullets[i].SetActive(true);
                return bullets[i];
            }
        }
        return null;
    }
    public static GameObject GetBulletFromPoolWithType(string type)
    {
        for (int i = 0; i < bullets.Count; i++)
        {
            if (!bullets[i].activeSelf && bullets[i].tag == type)
            {
                bullets[i].SetActive(true);
                return bullets[i];
            }
        }
        return null;
    }

    public static ParticleSystem GetHitVFX()
    {
        return hitVFX;
    }
}
