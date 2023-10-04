using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;
using static UnityEditor.PlayerSettings;
using static UnityEngine.Rendering.CoreUtils;

public class Parallaxer : MonoBehaviour
{
    public float wallHeight = 1f;
    public bool combineRooms = false;

    private Parallax[] sections;
    private Vector3 targetOffset = Vector3.zero;
    private Bounds bounds;

    private void Start()
    {
        sections = GetComponentsInChildren<Parallax>();
        foreach (var s in sections)
        {
            bounds.Encapsulate(s.bounds);
        }
    }

    private void LateUpdate()
    {
        var off = Vector3.zero;
        var cam = Camera.main.transform.position;
        if (combineRooms)
        {
            off = cam - bounds.center;
            off = new Vector2(off.x / bounds.extents.x, off.y / bounds.extents.y);
        }
        else
        {
            foreach (var s in sections)
            {
                if (s.InsideBounds(cam, out off))
                    break;
            }
        }
        off *= wallHeight;
        targetOffset = new Vector3(off.x, 0, off.y);

        foreach (var s in sections)
        {
            s.OnUpdate(targetOffset);
        }
    }
}
