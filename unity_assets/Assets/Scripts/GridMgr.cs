using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMgr : MonoBehaviour
{
    public Vector3 gridDim = Vector3.zero;

    public float wallHeight = 1f;
    public GameObject backdrop;
    public GameObject nitches;

    private Parallax[] sections;

    private Vector3 pivotPoint;
    private float rotAmount;

    private int rotAccum;
    private float rotTimer;
    private Vector3 endPos;
    private Quaternion endRot;
    
    private Vector3 startPos;
    private Quaternion startRot;

    private Bounds gridBounds = new Bounds();
    private List<int> outlineOnGrid = new List<int>();
    private List<List<int>> doorGroups = new List<List<int>>();
    private List<Parallax.meshInfo> doorWalls = new List<Parallax.meshInfo>();
    public List<RoomData> roomsOnGrid { get; private set; } = new List<RoomData>();
    private Dictionary<int, Parallax.meshInfo> tilesOnGrid = new Dictionary<int, Parallax.meshInfo>();

    private static Parallax.meshInfo emptyTile = new Parallax.meshInfo();

    private Parallax.MeshTypes[][] doorFrames = {
        new Parallax.MeshTypes[] { Parallax.MeshTypes.nitchUR, Parallax.MeshTypes.nitchUL, Parallax.MeshTypes.nitchLR, Parallax.MeshTypes.nitchLL },
        new Parallax.MeshTypes[] { Parallax.MeshTypes.nitchLR, Parallax.MeshTypes.nitchUR, Parallax.MeshTypes.nitchLL, Parallax.MeshTypes.nitchUL },
        new Parallax.MeshTypes[] { Parallax.MeshTypes.nitchLL, Parallax.MeshTypes.nitchLR, Parallax.MeshTypes.nitchUL, Parallax.MeshTypes.nitchUR },
        new Parallax.MeshTypes[] { Parallax.MeshTypes.nitchUL, Parallax.MeshTypes.nitchLL, Parallax.MeshTypes.nitchUR, Parallax.MeshTypes.nitchLR }
    };

    private void Awake()
    {
        startPos = transform.position;
        startRot = transform.rotation;
    }
    private void Start()
    {
        rotTimer = 0;
        backdrop.SetActive(false);

        gridBounds.size = gridDim;
        gridBounds.center = startPos;
    }

    private void Update()
    {
        if (rotTimer > 0)
        {
            var rotStep = rotAmount * Time.deltaTime;
            transform.RotateAround(pivotPoint, Vector3.forward, rotStep);

            rotTimer = Mathf.Clamp01(rotTimer - Time.deltaTime);
            foreach (var s in sections)
                s.OnUpdate(rotTimer, wallHeight);

            if (rotTimer == 0)
            {
                transform.rotation = endRot;
                transform.position = endPos;
            }
        }
    }

    public void StepRotate(bool rotCW, Vector3 pivot)
    {
        if (rotTimer > 0)
            return;

        var rotDir = rotCW ? -1 : 1;
        rotAmount = 90 * rotDir;
        pivot.z = transform.position.z;
        pivotPoint = pivot;
        
        rotAccum += rotDir;
        while (rotAccum > 2) rotAccum -= 4;
        while (rotAccum < -2) rotAccum += 4;

        var savedRot = transform.rotation;
        var savedPos = transform.position;
        transform.RotateAround(pivot, Vector3.forward, rotAmount);
        endRot = transform.rotation;
        endPos = transform.position;
        transform.rotation = savedRot;
        transform.position = savedPos;

        foreach (var s in sections)
            s.SetRot(rotDir);

        rotTimer = 1f;
    }

    public Transform CreateProxy(Vector3 pos)
    {
        var proxy = new GameObject("_proxy_").transform;
        proxy.SetParent(transform);
        proxy.position = pos;
        return proxy;
    }

    public void AddRoom(RoomData newRoom, GameObject player)
    {
        // add new room to the grid
        foreach (var tile in newRoom.GetTiles)
            tilesOnGrid[tile.HASH] = tile;

        // update the outline
        outlineOnGrid.Clear();
        roomsOnGrid.Add(newRoom);
        foreach (var room in roomsOnGrid)
        {
            room.EnableOutline(true);
            foreach (var tile in room.GetOutline)
            {
                var hash = tile.HASH;
                var visible = !tilesOnGrid.ContainsKey(hash) && !outlineOnGrid.Contains(hash);
                tile.setVisible(visible);
                if (visible)
                    outlineOnGrid.Add(hash);
            }
        }

        // add a door
        if (doorGroups.Count > 0)
        {
            // pick door closest to player
            var doorIdx = 0;
            var closest = float.MaxValue;
            var pos = player.transform.position;
            for (int i = 0; i <  doorGroups.Count; i++)
            {
                var dist = Vector3.Distance(DoorCenter(doorGroups[i]), pos);
                if (dist < closest)
                {
                    closest = dist;
                    doorIdx = i;
                }
            }

            var count = 0;
            var door = doorGroups[doorIdx];
            foreach (var tile in tilesOnGrid)
            {
                for (int i = 0; i < door.Count; i++)
                {
                    if (door[i] == tile.Key)
                    {
                        var rot = (tile.Value.meshType == Parallax.MeshTypes.wallLeft || tile.Value.meshType == Parallax.MeshTypes.wallRight) ? 0 : 2;
                        rot = (rot + tile.Value.tileSet.transform.parent.GetComponent<RoomData>().GetRotationIndex) % doorFrames.Length;
                        tile.Value.tileSet.SwapDoorFrame(tile.Value, doorFrames[rot][i], nitches);
                        count++;
                        break;
                    }
                }
                if (count >= 4)
                    break;
            }
        }
    }

    private Vector3 DoorCenter(List<int> group)
    {
        var pos = Vector2.zero;
        foreach(var hash in group)
            pos += emptyTile.REVHASH(hash);
        return pos / group.Count;
    }

    public void StartLevel()
    {
        sections = GetComponentsInChildren<Parallax>(true);
        foreach (var s in sections)
            s.SetOff(true, Vector3.zero);

        pivotPoint = startPos;
        endRot = startRot;
        endPos = startPos;
        rotAmount = 0f;
        rotTimer = 1f;
        rotAccum = 0;
    }

    public IEnumerator EndLevel(Agent player)
    {
        yield return new WaitForSeconds(2);

        // rotate back to start position
        pivotPoint = player.GetPostion;
        rotAmount = 90 * -rotAccum;

        foreach (var s in sections)
            s.SetOff(false, Vector3.zero);

        endRot = startRot;
        endPos = startPos;
        rotTimer = 1f;

        yield return new WaitUntil(() => rotTimer == 0 && player.actionsAllowed);
        yield return null;
    }

    public bool PlaceRoomOnGrid(RoomData room)
    {
        var valid = false;
        if (room.IsOnGrid(gridBounds))
        {
            if (tilesOnGrid.Count == 0)
                return true;

            doorWalls.Clear();
            var tiles = room.GetTiles;
            foreach (var tile in tiles)
            {
                // check for overlapping tiles
                var pos = tile.HASH;
                if (tilesOnGrid.ContainsKey(pos))
                    return false;

                // check for door wall pairs
                if (tile.meshTag.Contains("wall") && wallPair(tile, out var wall))
                {
                    doorWalls.Add(tile);
                    doorWalls.Add(wall);
                }
            }

            // must have 4+ matches to build groups
            if (doorWalls.Count >= 4)
            {
                // must have at least 1 group
                BuildGroups();
                valid = doorGroups.Count > 0;
            }
        }
        return valid;
    }

    private void BuildGroups()
    {
        var walls = new List<int>();
        foreach (var tile in doorWalls)
            walls.Add(tile.HASH);

        doorGroups.Clear();
        for (int i = 0; i < doorWalls.Count; i++)
        {
            var tile = doorWalls[i];
            Vector2 pos = tile.mesh.transform.position;
            addGroups(pos, walls, Vector2.right, Vector2.up);
            addGroups(pos, walls, Vector2.right, Vector2.down);
            addGroups(pos, walls, Vector2.left, Vector2.up);
            addGroups(pos, walls, Vector2.left, Vector2.down);
        }
    }

    private void addGroups(Vector2 pos, List<int> walls, Vector2 dir1, Vector2 dir2)
    {
        var hashes = new List<int>()
        {
            emptyTile.HASHPOS(pos),
            emptyTile.HASHPOS(pos + dir1),
            emptyTile.HASHPOS(pos + dir2),
            emptyTile.HASHPOS(pos + dir1 + dir2)
        };

        var grouped = new List<int>();
        foreach (var hash in hashes)
            if (walls.Contains(hash))
                grouped.Add(hash);

        if (grouped.Count == 4)
        {
            grouped.Sort();
            var match = false;
            foreach (var group in doorGroups)
            {
                int count = 0;
                for (int i = 0; i < grouped.Count; i++)
                    if (group[i] == grouped[i])
                        count++;

                match = count == grouped.Count;
                if (match)
                    break;
            }
            if (!match)
                doorGroups.Add(grouped);
        }
    }

    private bool wallPair(Parallax.meshInfo tile, out Parallax.meshInfo adjacent)
    {
        Vector2 pos = tile.mesh.transform.position;
        if (isWall(pos + Vector2.left, out adjacent))
            return true;
        if (isWall(pos + Vector2.right, out adjacent))
            return true;
        if (isWall(pos + Vector2.up, out adjacent))
            return true;
        if (isWall(pos + Vector2.down, out adjacent))
            return true;
        return false;
    }

    private bool isWall(Vector2 pos, out Parallax.meshInfo tile)
    {
        tile = emptyTile;
        var key = tile.HASHPOS(pos);
        if (tilesOnGrid.ContainsKey(key))
        {
            tile = tilesOnGrid[key];
            return tile.meshTag.Contains("wall");
        }

        return false;
    }
}
