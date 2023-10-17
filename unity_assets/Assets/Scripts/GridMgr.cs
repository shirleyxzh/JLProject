using System.Collections;
using UnityEngine;

public class GridMgr : MonoBehaviour
{
    public float wallHeight = 1f;

    private Parallax[] sections;
    private Vector3 targetOffset = Vector3.zero;
    private Bounds bounds;

    private Vector3 pivotPoint;
    private float rotDir;

    private float rotTimer;
    private Vector3 startOff;
    private Vector3 endOff;
    private Vector3 endPos;
    private Quaternion endRot;

    private void Start()
    {
        rotTimer = 0;
        startOff = Vector3.up;
        var off = startOff * wallHeight;
        targetOffset = new Vector3(off.x, 0, off.y);

        sections = GetComponentsInChildren<Parallax>();
        foreach (var s in sections)
        {
            bounds.Encapsulate(s.bounds);
            s.OnUpdate(targetOffset);
        }
    }

    private void LateUpdate()
    {
        if (rotTimer > 0)
        {
            var rotStep = rotDir * Time.deltaTime;
            transform.RotateAround(pivotPoint, Vector3.forward, rotStep);

            rotTimer = Mathf.Clamp01(rotTimer - Time.deltaTime);
            var off = Vector3.Lerp(endOff, startOff, rotTimer) * wallHeight;
            targetOffset = new Vector3(off.x, 0, off.y);

            foreach (var s in sections)
            {
                s.OnUpdate(targetOffset);
            }

            if (rotTimer == 0)
            {
                transform.rotation = endRot;
                transform.position = endPos;
                startOff = endOff;
            }
        }
    }

    public void Rotate(bool rotCW, Vector3 pivot)
    {
        if (rotTimer > 0) 
            return;

        rotDir = rotCW ? -90f : 90f;
        pivot.z = transform.position.z;
        pivotPoint = pivot;

        if (rotCW)
        {
            if (startOff == Vector3.up) endOff = Vector3.left;
            if (startOff == Vector3.left) endOff = Vector3.down;
            if (startOff == Vector3.down) endOff = Vector3.right;
            if (startOff == Vector3.right) endOff = Vector3.up;
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
        transform.RotateAround(pivot, Vector3.forward, rotDir);
        endRot = transform.rotation;
        endPos = transform.position;
        transform.rotation = savedRot;
        transform.position = savedPos;

        rotTimer = 1f;
    }

    public Transform CreateProxy(Vector3 pos)
    {
        var proxy = new GameObject("_proxy_").transform;
        proxy.SetParent(transform);
        proxy.position = pos;
        return proxy;
    }
}
