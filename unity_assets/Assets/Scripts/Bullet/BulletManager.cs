using UnityEngine;
using System.Collections.Generic;
public class BulletManager : MonoBehaviour
{
    public static GameObject bulletDump;
    public static List<GameObject> bullets;
    private void Start()
    {
        bulletDump = this.gameObject;
        bullets = new List<GameObject>();
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
            if (!bullets[i].activeSelf && bullets[i].GetComponent<Bullet>().type == type)
                return bullets[i];
        }
        return null;
    }
}
