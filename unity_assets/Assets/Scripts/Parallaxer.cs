using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;
using static UnityEngine.Rendering.CoreUtils;

public class Parallaxer : MonoBehaviour
{
    public float wallHeight = 1f;

    private Parallax[] sections;
    private Vector3 targetOffset = Vector3.zero;

    private void Awake()
    {
        sections = GetComponentsInChildren<Parallax>();
    }

    private void LateUpdate()
    {
        var cam = Camera.main.transform.position;
        foreach (var s in sections)
        {
            if (s.InsideBounds(cam, out var off))
            {
                off *= wallHeight;
                targetOffset = new Vector3(off.x, 0, off.y);
                break;
            }
        }

        foreach (var section in sections)
        {
            section.OnUpdate(targetOffset);
        }
    }
}
