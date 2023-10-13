using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Parallax : MonoBehaviour
{
    private bool ParallaxOff = false;

    public Bounds bounds { get; private set; }

    private MeshFilter[] meshes;
    private Dictionary<string, Vector3[]> meshList = new Dictionary<string, Vector3[]>();

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
            if (!name.Contains("floor"))
            {
                if (!meshList.ContainsKey(name))
                    meshList[name] = wall.sharedMesh.vertices;
            }

            if (!name.Contains("roof"))
            {
                var pos = wall.transform.position;
                if (emptyBBox)
                {
                    bbox.center = pos;
                    emptyBBox = false;
                }

                bbox.Encapsulate(pos);
            }
        }

        var blockers = GetComponentsInChildren<WallBlock>();
        foreach (var wall in blockers)
        {
            var blocker = new Blocker(wall.transform, wall.transform.localPosition);
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
        var inside = !ParallaxOff && bounds.Contains(pos);
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

    private void adjustVerts(MeshFilter wall, Vector3[] mesh, Vector3 step)
    {
        var vertices = wall.mesh.vertices;
        for (int i = 0; i < mesh.Length; i++)
        {
            var vert = mesh[i];
            if (vert.y > 0)
                vertices[i] = vert + step;
        }
        wall.mesh.vertices = vertices;
        wall.mesh.RecalculateBounds();
        wall.mesh.RecalculateNormals();
    }

    public void OnUpdate(Vector3 offset)
    {
        if (ParallaxOff)
            offset = Vector3.zero;

        foreach (var wall in meshes)
        {
            var name = wall.gameObject.name;
            if (meshList.ContainsKey(name))
            {
                var step = (Quaternion.Euler(wall.transform.localEulerAngles) * offset) / wall.transform.localScale.y;
                adjustVerts(wall, meshList[name], step);
            }
        }

        foreach (var blocker in blockers)
        {
            var step = (Quaternion.Euler(blocker.pos.localEulerAngles) * offset) / blocker.pos.localScale.y;
            blocker.pos.localPosition = blocker.center + step;
        }
    }
}
