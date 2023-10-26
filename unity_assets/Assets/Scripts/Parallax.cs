using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Parallax : MonoBehaviour
{
    public bool ParallaxOff { get; set; } = false;

    public Bounds bounds { get; private set; }

    public enum MeshTypes
    {
        cornerLL,
        cornerLR,
        cornerUL,
        cornerUR,
        nitchLL,
        nitchLR,
        nitchUL,
        nitchUR,
        wallLeft,
        wallRight,
        wallTop,
        wallBottom,
        outline
    }
    struct meshInfo
    {
        public MeshFilter mesh;
        public MeshTypes meshType;
        public string meshTag;
    };

    private List<meshInfo> meshList = new List<meshInfo>();
    private Dictionary<string, Vector3[]> vertList = new Dictionary<string, Vector3[]>();

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
        var layer = LayerMask.NameToLayer("parallax");

        bool emptyBBox = true;
        Bounds bbox = new Bounds();
        var meshes = GetComponentsInChildren<MeshFilter>();
        foreach (var mesh in meshes)
        {
            if (mesh.gameObject.layer != layer)
                continue;

            var tag = mesh.gameObject.tag;
            var info = new meshInfo();
            info.mesh = mesh;
            info.meshTag = tag;
            info.meshType = GetMeshType(mesh, tag);
            meshList.Add(info);

            if (!vertList.ContainsKey(tag))
                vertList[tag] = mesh.sharedMesh.vertices;

            if (!tag.Contains("outline"))
            {
                var pos = mesh.transform.position;
                if (emptyBBox)
                {
                    bbox.center = pos;
                    emptyBBox = false;
                }

                bbox.Encapsulate(pos);
            }
        }

        var blockers = GetComponentsInChildren<WallBlock>();
        foreach (var _b in blockers)
        {
            var blocker = new Blocker(_b.transform, _b.transform.localPosition);
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

    private MeshTypes GetMeshType(MeshFilter mesh, string tag)
    {
        // determine the type based on rotation
        var rotY = mesh.transform.localEulerAngles.y;
        if (tag.Contains("corner"))
        {
            if (rotY == 0) return MeshTypes.cornerLR;
            if (rotY == 90) return MeshTypes.cornerUR;
            if (rotY == 180) return MeshTypes.cornerUL;
            return MeshTypes.cornerLL;
        }
        else if (tag.Contains("nitch"))
        {
            if (rotY == 0) return MeshTypes.nitchLR;
            if (rotY == 90) return MeshTypes.nitchUR;
            if (rotY == 180) return MeshTypes.nitchUL;
            return MeshTypes.nitchLL;
        }
        else if (tag.Contains("wall"))
        {
            if (rotY == 0) return MeshTypes.wallBottom;
            if (rotY == 90) return MeshTypes.wallRight;
            if (rotY == 180) return MeshTypes.wallTop;
            return MeshTypes.wallLeft;
        }
        // assume outline
        return MeshTypes.outline;
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

    private void adjustVerts(MeshFilter mf, Vector3[] verts, Vector3 step)
    {
        var vertices = mf.mesh.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            var vert = verts[i];
            if (vert.y > 0)
                vertices[i] = vert + step;
        }
        mf.mesh.vertices = vertices;
        mf.mesh.RecalculateBounds();
        mf.mesh.RecalculateNormals();
    }

    public void OnUpdate(Vector3 offset)
    {
        if (ParallaxOff)
            offset = Vector3.zero;

        foreach (var info in meshList)
        {
            var mesh = info.mesh;
            var step = (Quaternion.Euler(mesh.transform.localEulerAngles) * offset) / mesh.transform.localScale.y;
            adjustVerts(mesh, vertList[info.meshTag], step);
        }

        foreach (var blocker in blockers)
        {
            var step = (Quaternion.Euler(blocker.pos.localEulerAngles) * offset) / blocker.pos.localScale.y;
            blocker.pos.localPosition = blocker.center + step;
        }
    }
}
