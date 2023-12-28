using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GridMgr : MonoBehaviour
{
    public enum IconTypes
    {
        Mark,
        Start,
        Target,
        Mystery,
        Chest,
        Salvage,
        Key,
    }

    [System.Serializable]
    public struct IconInfo
    {
        public IconTypes iconType;
        public GameObject iconSprite;
    }

    public Vector3 gridDim = Vector3.zero;

    public float wallHeight = 1f;
    public float rotateTime = 1f;
    public GameObject backdrop;
    public GameObject gridMarks;
    public GameObject nitches;
    public GameObject roomQue;
    [Space(10)]
    public GameObject gridIconsBase;
    public IconInfo[] gridIcons;

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

    private Dictionary<IconTypes, GameObject> iconsList = new Dictionary<IconTypes, GameObject>();

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
        gridBounds.size = gridDim * 4;
        gridBounds.center = startPos;

        var mark = GetIconSprite(IconTypes.Mark);
        var halfx = Mathf.FloorToInt(gridBounds.size.x / 2);
        var halfy = Mathf.FloorToInt(gridBounds.size.y / 2);
        for (int x = -halfx; x <= halfx; x += 4)
        {
            for (int y = -halfy; y <= halfy; y += 4)
            {
                var obj = Instantiate(mark, gridMarks.transform, true);
                obj.transform.position = new Vector3(x, y, 0);
            }
        }
        
        ResetLevel(true);
    }

    private GameObject GetIconSprite(IconTypes iconType)
    {
        foreach (var icon in gridIcons)
        {
            if (icon.iconType == iconType)
                return icon.iconSprite;
        }
        return gridIcons[0].iconSprite;
    }

    private Vector3 GetIconCell(int minDist)
    {
        minDist *= 4;
        var posOk = false;
        var pos = Vector3.zero;
        while (!posOk)
        {
            int x = Random.Range(0, (int)gridDim.x);
            int y = Random.Range(0, (int)gridDim.y);
            // ignore corner cells
            posOk = !(x == 0 || x == gridDim.x - 1) && !(y == 0 || y == gridDim.y - 1);
            if (posOk)
            {
                // make sure no other icons are within the min dist
                pos.x = ((x * 4) - (gridBounds.size.x / 2)) + 2;
                pos.y = ((y * 4) - (gridBounds.size.y / 2)) + 2;
                foreach (var cell in iconsList)
                {
                    if (Mathf.Abs(pos.x - cell.Value.transform.position.x) < minDist
                        && Mathf.Abs(pos.y - cell.Value.transform.position.y) < minDist)
                    {
                        posOk = false;
                        break;
                    }
                }
            }
        }
        return pos;
    }

    private GameObject CreateIcon(IconTypes iconType, Vector3 pos)
    {
        var icon = GetIconSprite(iconType);
        var obj = Instantiate(icon, gridIconsBase.transform, true);
        obj.transform.position = pos;
        return obj;
    }

    private void Update()
    {
        if (rotTimer > 0)
        {
            var rotStep = rotAmount / rotateTime * Time.deltaTime;
            transform.RotateAround(pivotPoint, Vector3.forward, rotStep);

            rotTimer = Mathf.Clamp(rotTimer - Time.deltaTime, 0, rotateTime);
            foreach (var s in sections)
                s.OnUpdate(rotTimer / rotateTime, wallHeight);

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
            s.SetRotVector(rotDir);

        rotTimer = rotateTime;
    }

    public Transform CreateProxy(Vector3 pos)
    {
        var proxy = new GameObject("_proxy_").transform;
        proxy.SetParent(transform);
        proxy.position = pos;
        return proxy;
    }

    public void AddRoom(RoomData newRoom)
    {
        doorGroups.Clear();
        roomsOnGrid.Add(newRoom);
        newRoom.EnableOutline(false);
    }

    public void AttachRoom(RoomData newRoom, GameObject player)
    {
        // attach new room to the grid
        foreach (var tile in newRoom.GetTiles)
            tilesOnGrid[tile.HASH] = tile;

        // update the outline
        outlineOnGrid.Clear();
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

        roomQue.SetActive(false);
        pivotPoint = startPos;
        rotTimer = rotateTime;
        endRot = startRot;
        endPos = startPos;
        rotAmount = 0f;
        rotAccum = 0;
    }

    public IEnumerator EndLevel(Agent player)
    {
        yield return new WaitForSeconds(2);

        // hide the spawn points
        foreach (var room in roomsOnGrid)
            room.HideSpawnPoints();

        // rotate back to start position
        pivotPoint = player.GetPostion;
        rotAmount = 90 * -rotAccum;

        foreach (var s in sections)
            s.SetOff(false, Vector3.zero);

        endRot = startRot;
        endPos = startPos;
        rotTimer = rotateTime;

        yield return new WaitUntil(() => rotTimer == 0 && player.actionsAllowed);
        yield return null;

        foreach (var room in roomsOnGrid)
            room.EnableOutline(false);
    }

    public void ShowRoomSequence(List<RoomData.RoomTypes> roomOrder, int roomIdx, RoomData[] rooms)
    {
        // delete current seq
        roomQue.SetActive(true);
        foreach (Transform child in roomQue.transform)
            Destroy(child.gameObject);

        var iconPosition = Vector3.zero;
        for (int i = roomIdx; i < roomIdx + 4; i++)
        {
            if (i >= roomOrder.Count)
                continue;

            var roomType = roomOrder[i];
            foreach (var room in rooms)
            {
                if (room.IsType(roomType))
                {
                    var obj = room.GetFilter();
                    var icon = Instantiate(obj.gameObject, roomQue.transform);
                    icon.transform.localPosition = iconPosition;
                    icon.transform.localScale = Vector3.one;
                    iconPosition += Vector3.forward * 20;
                    icon.SetActive(true);
                    break;
                }
            }
        }
    }

    public bool RoomHasTarget(RoomData room)
    {
        return room.ContainsIcon(iconsList[IconTypes.Target]);
    }

    public bool PlaceRoomOnGrid(RoomData room, Vector3 pos, int rotDir)
    {
        room.MoveAndRot(pos, rotDir, gridBounds);

        var valid = false;
        if (room.IsOnGrid(gridBounds))
        {
            // reject if an enemy spawn point is over a grid icon
            if (room.IsBlockedByIcon(gridIconsBase))
                return false;

            if (tilesOnGrid.Count == 0)
            {
                // first room must be over the start icon
                return room.ContainsIcon(iconsList[IconTypes.Start]);
            }

            doorWalls.Clear();
            var tiles = room.GetTiles;
            foreach (var tile in tiles)
            {
                // check for overlapping tiles
                if (tilesOnGrid.ContainsKey(tile.HASH))
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

    public GameObject GetStartPoint()
    {
        return iconsList[IconTypes.Start];
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

    public void ResetLevel(bool clearAll)
    {
        rotTimer = 0;
        backdrop.SetActive(false);
        
        transform.position = startPos;
        transform.rotation = startRot;

        foreach (var room in roomsOnGrid)
            Destroy(room.gameObject);

        tilesOnGrid.Clear();
        roomsOnGrid.Clear();

        if (clearAll)
        {
            foreach (var cell in iconsList)
                Destroy(cell.Value);

            iconsList.Clear();
            iconsList[IconTypes.Start] = CreateIcon(IconTypes.Start, GetIconCell(0));
            iconsList[IconTypes.Target] = CreateIcon(IconTypes.Target, GetIconCell(4));
            iconsList[IconTypes.Mystery] = CreateIcon(IconTypes.Mystery, GetIconCell(2));
            iconsList[IconTypes.Salvage] = CreateIcon(IconTypes.Salvage, GetIconCell(2));
            iconsList[IconTypes.Chest] = CreateIcon(IconTypes.Chest, GetIconCell(2));
            iconsList[IconTypes.Key] = CreateIcon(IconTypes.Key, GetIconCell(2));
        }

        foreach (var cell in iconsList)
            cell.Value.SetActive(true);
    }
}
