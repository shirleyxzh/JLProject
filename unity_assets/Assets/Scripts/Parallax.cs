using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Parallax : MonoBehaviour
{
    public Bounds bounds { get; private set; }

    private int[] roofVerts = { 8, 7, 6, 5, 4, 3, 2, 1, 0 };
    private int[] nitchVerts = { 24, 21,20,17, 5, 4, 3, 2, 1, 0 };
    private int[] wallVerts = { 19, 18, 15, 14, 13, 12, 11, 10, 9 };
    private int[] cornerVerts = { 33, 31, 29, 28, 26, 23, 22, 21, 20, 19, 18, 17, 7, 6, 5, 4, 3, 2, 1, 0 };

    private MeshFilter[] meshes;

    private Vector3[] wallMesh = null;
    private Vector3[] roofMesh = null;
    private Vector3[] nitchMesh = null;
    private Vector3[] cornerMesh = null;

    struct Blocker
    {
        public Transform pos;
        public Vector3 center;
        public Blocker(Transform p, Vector3 c) { pos = p; center = c; }
    };
    private List<Blocker> blockers = new List<Blocker>();

    private Connector isConnector;

    private void Awake()
    {
        isConnector = GetComponent<Connector>();

        bool emptyBBox = true;
        Bounds bbox = new Bounds();
        meshes = GetComponentsInChildren<MeshFilter>();
        foreach (var wall in meshes)
        {
            var name = wall.gameObject.name;
            if (name.Contains("roof")  && roofMesh == null)
                roofMesh = wall.sharedMesh.vertices;
            else if (name.Contains("wall") && wallMesh == null)
                wallMesh = wall.sharedMesh.vertices;
            else if (name.Contains("nitch") && nitchMesh == null)
                nitchMesh = wall.sharedMesh.vertices;
            else if (name.Contains("corner") && cornerMesh == null)
                cornerMesh = wall.sharedMesh.vertices;

            if (name.Contains("roof"))
                continue;

            var pos = wall.transform.position;
            if (emptyBBox)
            {
                bbox.center = pos;
                emptyBBox = false;
            }

            bbox.Encapsulate(pos);
        }

        var blockers = GetComponentsInChildren<WallBlock>();
        foreach (var wall in blockers)
        {
            var blocker = new Blocker(wall.transform, wall.transform.position);
            this.blockers.Add(blocker);
        }

        if (emptyBBox)
        {
            // set extents to negative to void any Contains calls
            bbox.Expand(-Vector3.one);
        }
        else
        {
            // expand to fit cell and include camera Z
            bbox.Expand(Vector3.forward * 100f);
            bbox.Expand(Vector3.one);
        }
        bounds = bbox;
    }

    public bool InsideBounds(Vector3 pos, out Vector3 offset)
    {        
        offset = Vector3.zero;
        var inside = bounds.Contains(pos);
        if (inside)
        {
            var off = pos - bounds.center;
            offset = new Vector2(off.x / bounds.extents.x, off.y / bounds.extents.y);
            if (isConnector)
            {
                offset.Scale(-Vector3.one);     // reverse direction of parallax in a connector
                var newOff = calcOffset(pos, offset);
                if (isConnector.IsHorz)
                    offset.y = newOff;
                else
                    offset.x = newOff;
            }
        }
        return inside;
    }

    // match the offset of the section the cam is closest to
    private float calcOffset(Vector3 cam, Vector3 offset)
    {
        var forHorz = isConnector.IsHorz;
        var a = isConnector.section_a.bounds;
        var b = isConnector.section_b.bounds;

        var pos = forHorz ? cam.y : cam.x;
        var off = forHorz ? offset.x : offset.y;
        var ac = forHorz ? a.center.y : a.center.x;
        var bc = forHorz ? b.center.y : b.center.x;
        var ae = forHorz ? a.extents.y : a.extents.x;
        var be = forHorz ? b.extents.y : b.extents.x;
        
        var _t = (-off + 1f) / 2f;
        var center = Mathf.Lerp(ac, bc, _t);
        var extents = Mathf.Lerp(ae, be, _t); ;
        return (pos - center) / extents;
    }

    private void adjustVerts(MeshFilter wall, Vector3[] mesh, int[] idx, Vector3 step)
    {
        var vertices = wall.mesh.vertices;
        for (int i = 0; i < mesh.Length; i++)
        {
            var vert = mesh[i];
            if (vert.y > 0)
                vertices[i] = vert + step;
        }
        //for (int i = 0; i < idx.Length; i++)
        //{
        //    vertices[idx[i]] = mesh[idx[i]] + step;
        //}
        wall.mesh.vertices = vertices;
        wall.mesh.RecalculateBounds();
        wall.mesh.RecalculateNormals();
    }

    public void OnUpdate(Vector3 offset)
    {
        foreach (var wall in meshes)
        {
            var step = (Quaternion.Euler(wall.transform.localEulerAngles) * offset) / wall.transform.localScale.y;
            var name = wall.gameObject.name;
            if (name.Contains("roof"))
                adjustVerts(wall, roofMesh, roofVerts, step);
            else if (name.Contains("wall"))
                adjustVerts(wall, wallMesh, wallVerts, step);
            else if (name.Contains("nitch"))
                adjustVerts(wall, nitchMesh, nitchVerts, step);
            else if (name.Contains("corner"))
                adjustVerts(wall, cornerMesh, cornerVerts, step);
        }

        foreach (var blocker in blockers)
        {
            var step = (Quaternion.Euler(blocker.pos.rotation.eulerAngles) * offset) / blocker.pos.localScale.y;
            blocker.pos.position = blocker.center + step;
        }
    }
}
