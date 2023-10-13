using System.Collections;
using UnityEngine;

public class GridMgr : MonoBehaviour
{
    public float wallHeight = 1f;

    public Vector3 GetPosition => transform.position;
    
    private bool combineRooms = true;

    private Parallax[] sections;
    private Vector3 targetOffset = Vector3.zero;
    private Bounds bounds;

    private bool rotating;
    private float rotTimer;
    private Vector3 startOff;
    private Vector3 endOff;

    private void Start()
    {
        rotating = false;
        startOff = Vector3.up;
        var off = startOff * wallHeight;
        targetOffset = new Vector3(off.x, 0, off.y);

        sections = GetComponentsInChildren<Parallax>();
        foreach (var s in sections)
        {
            bounds.Encapsulate(s.bounds);
            if (combineRooms)
                s.OnUpdate(targetOffset);
        }
    }

    private void LateUpdate()
    {
        if (rotating)
        {
            var off = Vector3.Lerp(endOff, startOff, rotTimer) * wallHeight;
            targetOffset = new Vector3(off.x, 0, off.y);

            foreach (var s in sections)
            {
                s.OnUpdate(targetOffset);
            }
        }
    }

    public void Rotate(bool rotCW, Vector3 pivot)
    {
        if (rotating) return;
        rotating = true;

        StartCoroutine(RotateGrid(rotCW, pivot));
    }

    IEnumerator RotateGrid(bool rotCW, Vector3 pivot)
    {
        var rot = rotCW ? -90f : 90f;
        pivot.z = transform.position.z;

        if (rotCW)
        {
            if (startOff == Vector3.up)     endOff = Vector3.left;
            if (startOff == Vector3.left)   endOff = Vector3.down;
            if (startOff == Vector3.down)   endOff = Vector3.right;
            if (startOff == Vector3.right)  endOff = Vector3.up;
        }
        else
        {
            if (startOff == Vector3.up) endOff = Vector3.right;
            if (startOff == Vector3.right) endOff = Vector3.down;
            if (startOff == Vector3.down) endOff = Vector3.left;
            if (startOff == Vector3.left) endOff = Vector3.up;
        }

        var savedRot = transform.rotation;
        var savedPos = transform.position;
        transform.RotateAround(pivot, Vector3.forward, rot);
        var endRot = transform.rotation;
        var endPos = transform.position;
        transform.rotation = savedRot;
        transform.position = savedPos;

        rotTimer = 1f;
        while (rotTimer > 0)
        {
            yield return null;
            transform.RotateAround(pivot, Vector3.forward, rot * Time.deltaTime);
            rotTimer -= Time.deltaTime;
        }
        
        transform.rotation = endRot;
        transform.position = endPos;
        startOff = endOff;

        rotating = false;
    }
}
