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
    public struct meshInfo
    {
        public MeshFilter mesh;
        public MeshTypes meshType;
        public string meshTag;
        public Parallax tileSet;
        public bool isVisible => mesh.gameObject.activeSelf;
        public void setVisible(bool visible) => mesh.gameObject.SetActive(visible);
        public int HASHPOS(Vector2 pos) => ((Mathf.FloorToInt(pos.x) + 1000) + ((Mathf.FloorToInt(pos.y) - 1000) * -10000));
        public int HASH => HASHPOS(mesh.transform.position);
        public Vector2 REVHASH(int hash) { var d = Mathf.FloorToInt(hash / -10000);  var y = d + 1000; var x = hash - (((y - 1000) * -10000)) - 1000; return new Vector2(x, y); }
    };

    public List<meshInfo> meshList { get; private set; } = new List<meshInfo>();
    private Dictionary<string, Vector3[]> vertList = new Dictionary<string, Vector3[]>();

    struct Blocker
    {
        public Transform pos;
        public Vector3 center;
        public Blocker(Transform p, Vector3 c) { pos = p; center = c; }
    };
    private List<Blocker> blockers = new List<Blocker>();
    
    private Connector isConnector;
    private BoxCollider boxCollider;
    private Vector3 defaultVector;
    private Vector3 startVector;
    private Vector3 lastVector;
    private Vector3 endVector;

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

            var info = AddToMeshList(mesh);

            if (!info.meshTag.Contains("outline"))
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
            // expand to fit cell
            bbox.Expand(Vector3.one);
        }
        bounds = bbox;
        lastVector = Vector3.up;
        defaultVector = lastVector;
        boxCollider = GetComponent<BoxCollider>();
    }

    private meshInfo AddToMeshList(MeshFilter mesh)
    {
        var info = new meshInfo();
        info.mesh = mesh;
        info.tileSet = this;
        info.meshTag = mesh.gameObject.tag;
        info.meshType = GetMeshType(info.mesh, info.meshTag);
        meshList.Add(info);

        if (!vertList.ContainsKey(info.meshTag))
            vertList[info.meshTag] = info.mesh.sharedMesh.vertices;

        return info;
    }

    public bool isInside(Bounds gridBounds)
    {
        var bbox = boxCollider.bounds;
        bbox.Expand(-Vector3.one * 0.1f);
        return gridBounds.Contains(bbox.min) && gridBounds.Contains(bbox.max);
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
        var extents = Mathf.Lerp(ae, be, _t);
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

    public void OnUpdate(float rotTimer, float wallHeight)
    {
        var off = Vector3.Lerp(endVector, startVector, rotTimer) * wallHeight;
        var offset = new Vector3(off.x, 0, off.y);

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

        if (rotTimer == 0)
            startVector = endVector;
    }

    public void SetRot(int rotDir)
    {
        if (rotDir < 0)
        {
            // CW
            if (lastVector == Vector3.up) endVector = Vector3.left;
            else if (lastVector == Vector3.right) endVector = Vector3.up;
            else if (lastVector == Vector3.left) endVector = Vector3.down;
            else if (lastVector == Vector3.down) endVector = Vector3.right;
           lastVector = endVector;
        }
        else if (rotDir > 0)
        {
            // CCW
            if (lastVector == Vector3.up) endVector = Vector3.right;
            else if (lastVector == Vector3.left) endVector = Vector3.up;
            else if (lastVector == Vector3.down) endVector = Vector3.left;
            else if (lastVector == Vector3.right) endVector = Vector3.down;
           lastVector = endVector;
        }
    }

    public void SetOff(bool isStartOff, Vector3 offset)
    {
        if (isStartOff)
        {
            startVector = offset;
            endVector = lastVector;
            defaultVector = lastVector;
        }
        else
        {
            endVector = offset;
            lastVector = defaultVector;
        }
    }

    public void SwapDoorFrame(meshInfo tile, MeshTypes frame, GameObject nitches)
    {
        var meshes = nitches.GetComponentsInChildren<MeshFilter>();
        foreach (var mesh in meshes)
        {
            if (GetMeshType(mesh, "nitch") == frame)
            {
                var frameMesh = Instantiate(mesh, this.transform);
                frameMesh.transform.position = tile.mesh.transform.position;
                AddToMeshList(frameMesh);
                tile.setVisible(false);
                break;
            }
        }
    }
}
