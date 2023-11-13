using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.InputSystem;

public class Tetriria : MonoBehaviour
{
    [SerializeField] private GridMgr grid;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject blockerPrefab;

    [SerializeField] private InputActionReference pointerClick, pointerPosition, rotFilterCW, rotFilterCCW;

    [SerializeField] private GameObject[] playerPrefabs;
    [SerializeField] private RoomData[] rooms;

    private PlayerSpawner playerSpawn;
    private EnemySpawner enemySpawn;
    private CursorMgr cursorMgr;
    private FollowCam followCam;

    private float ZOOMED_IN = 3f;
    private float ZOOMED_OUT = 17f;

    private GameObject pickedPlayer;
    private GameObject playerSpawnPoint;
    private GameObject blockerBase;

    private static float WALL_THICKNESS = 0.6f;
    private static float SECTION_LENGTH = 1f + WALL_THICKNESS;

    private UnityEngine.InputSystem.PlayerInput inputMaps;
    private int rotRoomDir = 0;

    // 90 degrees
    Dictionary<Parallax.MeshTypes, Parallax.MeshTypes> rot90 = new Dictionary<Parallax.MeshTypes, Parallax.MeshTypes>
    {
        { Parallax.MeshTypes.wallLeft,      Parallax.MeshTypes.wallTop },
        { Parallax.MeshTypes.wallRight,     Parallax.MeshTypes.wallBottom },
        { Parallax.MeshTypes.wallTop,       Parallax.MeshTypes.wallRight },
        { Parallax.MeshTypes.wallBottom,    Parallax.MeshTypes.wallLeft },
        { Parallax.MeshTypes.cornerLL,      Parallax.MeshTypes.cornerUL },
        { Parallax.MeshTypes.cornerLR,      Parallax.MeshTypes.cornerLL },
        { Parallax.MeshTypes.cornerUL,      Parallax.MeshTypes.cornerUR },
        { Parallax.MeshTypes.cornerUR,      Parallax.MeshTypes.cornerLR },
    };
    // 180 degrees
    Dictionary<Parallax.MeshTypes, Parallax.MeshTypes> rot180 = new Dictionary<Parallax.MeshTypes, Parallax.MeshTypes>
    {
        { Parallax.MeshTypes.wallLeft,      Parallax.MeshTypes.wallRight },
        { Parallax.MeshTypes.wallRight,     Parallax.MeshTypes.wallLeft },
        { Parallax.MeshTypes.wallTop,       Parallax.MeshTypes.wallBottom },
        { Parallax.MeshTypes.wallBottom,    Parallax.MeshTypes.wallTop },
        { Parallax.MeshTypes.cornerLL,      Parallax.MeshTypes.cornerUR },
        { Parallax.MeshTypes.cornerLR,      Parallax.MeshTypes.cornerUL },
        { Parallax.MeshTypes.cornerUL,      Parallax.MeshTypes.cornerLR },
        { Parallax.MeshTypes.cornerUR,      Parallax.MeshTypes.cornerLL },
    };
    // 270 degrees
    Dictionary<Parallax.MeshTypes, Parallax.MeshTypes> rot270 = new Dictionary<Parallax.MeshTypes, Parallax.MeshTypes>
    {
        { Parallax.MeshTypes.wallLeft,      Parallax.MeshTypes.wallBottom },
        { Parallax.MeshTypes.wallRight,     Parallax.MeshTypes.wallTop },
        { Parallax.MeshTypes.wallTop,       Parallax.MeshTypes.wallLeft },
        { Parallax.MeshTypes.wallBottom,    Parallax.MeshTypes.wallRight },
        { Parallax.MeshTypes.cornerLL,      Parallax.MeshTypes.cornerLR },
        { Parallax.MeshTypes.cornerLR,      Parallax.MeshTypes.cornerUR },
        { Parallax.MeshTypes.cornerUL,      Parallax.MeshTypes.cornerLL },
        { Parallax.MeshTypes.cornerUR,      Parallax.MeshTypes.cornerUL },
    };
    
    void Awake()
    {
        cursorMgr = GetComponent<CursorMgr>();
        enemySpawn = GetComponent<EnemySpawner>();
        playerSpawn = GetComponent<PlayerSpawner>();
        followCam = Camera.main.GetComponent<FollowCam>();
        inputMaps = GetComponent<UnityEngine.InputSystem.PlayerInput>();
    }

    private void Start()
    {
        hudPanel.SetActive(false);
        StartCoroutine(GameLoop());
    }

    private void OnEnable()
    {
        rotFilterCW.action.started += ctx => rotRoomDir = -1;
        rotFilterCCW.action.started += ctx => rotRoomDir = 1;
    }

    IEnumerator GameLoop()
    {
        // pick player avatar
        inputMaps.SwitchCurrentActionMap("UI");
        yield return StartCoroutine(PickPlayer());

        // scramble rooms - put square first
        // TODO: pull from datasheet
        var roomOrder = new List<RoomData.RoomTypes>();
        roomOrder.Add(RoomData.RoomTypes.Square);
        roomOrder.Add(RoomData.RoomTypes.L_Shape);
        roomOrder.Add(RoomData.RoomTypes.Z_Shape);
        roomOrder.Add(RoomData.RoomTypes.I_Shape);
        roomOrder.Add(RoomData.RoomTypes.T_Shape);
        roomOrder.Add(RoomData.RoomTypes.S_Shape);
        roomOrder.Add(RoomData.RoomTypes.L2_Shape);

        // play thru all the rooms
        int roomIdx = 0;
        while (roomIdx < roomOrder.Count)
        {
            cursorMgr.SetDefault();
            followCam.Target = null;
            hudPanel.SetActive(false);
            grid.backdrop.SetActive(true);
            yield return StartCoroutine(SetCamera(grid.gameObject, ZOOMED_OUT));

            // TODO: display new room order on screen

            // spawn the room
            var room = CreateRoom(roomOrder[roomIdx]);
            room.EnableOutline(false);

            // wait to position room on grid
            room.EnableFilter(true);
            bool placed = false;
            while (!placed)
            {
                var pos = Camera.main.ScreenToWorldPoint(pointerPosition.action.ReadValue<Vector2>());
                room.MoveAndRot(pos, rotRoomDir);
                var isValid = grid.PlaceRoomOnGrid(room);
                room.UpdateFilter(isValid);
                rotRoomDir = 0;

                placed = isValid && pointerClick.action.ReadValue<float>() > 0;
                yield return null;
            }
            room.EnableFilter(false);
            GetSpawnPoints(room);
            grid.AddRoom(room, pickedPlayer);
            CreateBlockers();

            // spawn player in room and zoom in
            hudPanel.SetActive(true);
            cursorMgr.SetTargeting();
            pickedPlayer.SetActive(true);
            grid.backdrop.SetActive(false);

            grid.StartLevel();
            playerSpawn.StartLevel();
            enemySpawn.StartLevel(pickedPlayer);
            yield return StartCoroutine(SetCamera(pickedPlayer, ZOOMED_IN));

            // spawn enemies until room is cleared
            followCam.SmoothFollow = true;
            followCam.Target = pickedPlayer;
            inputMaps.SwitchCurrentActionMap("Player");
            yield return new WaitWhile(() => enemySpawn.EnemiesLeft);

            // close out the level
            enemySpawn.EndLevel();
            playerSpawn.EndLevel();
            followCam.SmoothFollow = false;
            inputMaps.SwitchCurrentActionMap("UI");
            yield return grid.EndLevel(playerSpawn.Player);

            foreach (var rm in grid.roomsOnGrid)
                rm.EnableOutline(false);

            roomIdx++;
            if (roomIdx >= roomOrder.Count)
                roomIdx = 0;
        }
    }

    void Update()
    {
        if (Input.GetKey("escape"))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif 
        }
    }

    private void GetSpawnPoints(RoomData room)
    {
        if (playerSpawnPoint == null)
        {
            room.GetSpawnPoint("PlayerSpawn", out playerSpawnPoint);
            playerSpawn.InitPlayer(pickedPlayer, playerSpawnPoint);
        }

        enemySpawn.TotolToSpawn = 0;            // TODO: pull total from datasheet
        enemySpawn.spawnPoints.Clear();
        var found = true;
        int num = 1;
        while (found)
        {
            GameObject point;
            found = room.GetSpawnPoint($"EnemySpawn{num++}", out point);
            if (found)
            {
                enemySpawn.spawnPoints.Add(point);
                enemySpawn.TotolToSpawn++;
            }
        }
    }

    IEnumerator PickPlayer()
    {
        Camera.main.orthographicSize = 1;

        var p1 = playerSpawn.CreateAvatar(playerPrefabs[0], new Vector3(-0.5f, -0.3f, 0), Vector3.right);
        var p2 = playerSpawn.CreateAvatar(playerPrefabs[1], new Vector3(0.5f, -0.3f, 0), Vector3.left);

        var p1BBox = p1.GetComponent<BoxCollider>().bounds;
        var p2BBox = p2.GetComponent<BoxCollider>().bounds;

        pickedPlayer = null;
        while (pickedPlayer == null) 
        {
            var pos = Camera.main.ScreenToWorldPoint(pointerPosition.action.ReadValue<Vector2>());
            pos.z = 0;
            var inP1 = p1BBox.Contains(pos);
            var inP2 = p2BBox.Contains(pos);
            if (inP1 || inP2) cursorMgr.SetTargeting();
            else              cursorMgr.SetDefault();

            if (pointerClick.action.ReadValue<float>() > 0)
            {
                if (inP1) pickedPlayer = p1;
                if (inP2) pickedPlayer = p2;
            }
            
            yield return null;
        }

        if (pickedPlayer == p1) Destroy(p2);
        if (pickedPlayer == p2) Destroy(p1);

        Camera.main.orthographicSize = ZOOMED_OUT - 5;
        pickedPlayer.SetActive(false);
    }

    IEnumerator SetCamera(GameObject target, float endSize)
    {
        var _t = 0f;
        var startSize = Camera.main.orthographicSize;
        var startPos = Camera.main.transform.position;
        var endPos = target.transform.position;
        endPos.z = startPos.z;
        while (_t < 1f)
        {
            _t = Mathf.Clamp01(_t + Time.deltaTime);
            Camera.main.orthographicSize = Mathf.Lerp(startSize, endSize, _t);
            Camera.main.transform.position = Vector3.Lerp(startPos, endPos, _t);
            yield return null;
        }
    }

    private RoomData CreateRoom(RoomData.RoomTypes roomType)
    {
        RoomData obj = null;
        foreach (var room in rooms)
        {
            if (room.IsType(roomType))
            {
                obj = Instantiate(room.gameObject, grid.transform, true).GetComponent<RoomData>();
                break;
            }
        }
        return obj;
    }

    private void CreateBlockers()
    {
        if (blockerBase)
            DestroyImmediate(blockerBase);

        blockerBase = new GameObject("Blockers");
        blockerBase.transform.SetParent(grid.transform, false);

        // create bboxes for walls/corners in horz and vert lists
        var vertList = new List<Bounds>();
        var horzList = new List<Bounds>();
        var rooms = grid.transform.GetComponentsInChildren<RoomData>();
        foreach (var room in rooms)
        {
            var tiles = room.GetTiles;
            var rot = room.GetRotationIndex;
            foreach (var tile in tiles)
            {
                if (tile.isVisible)
                {
                    var type = ConvertType(rot, tile.meshType);
                    if (type == Parallax.MeshTypes.wallLeft || type == Parallax.MeshTypes.cornerLL || type == Parallax.MeshTypes.cornerUL)
                        SaveBBox(tile.mesh, vertList, Vector3.left, WALL_THICKNESS, SECTION_LENGTH);
                    else if (type == Parallax.MeshTypes.wallRight || type == Parallax.MeshTypes.cornerLR || type == Parallax.MeshTypes.cornerUR)
                        SaveBBox(tile.mesh, vertList, Vector3.right, WALL_THICKNESS, SECTION_LENGTH);
                    if (type == Parallax.MeshTypes.wallTop || type == Parallax.MeshTypes.cornerUL || type == Parallax.MeshTypes.cornerUR)
                        SaveBBox(tile.mesh, horzList, Vector3.up, SECTION_LENGTH, WALL_THICKNESS);
                    else if (type == Parallax.MeshTypes.wallBottom || type == Parallax.MeshTypes.cornerLL || type == Parallax.MeshTypes.cornerLR)
                        SaveBBox(tile.mesh, horzList, Vector3.down, SECTION_LENGTH, WALL_THICKNESS);
                }
            }
        }

        // combine overlapping bboxes and create blocker objs
        CombineBBox(horzList);
        CombineBBox(vertList);

        blockerBase.AddComponent(typeof(Parallax));
    }

    private Parallax.MeshTypes ConvertType(int rotIndex, Parallax.MeshTypes type)
    {
        if (rotIndex > 0)
        {
            var matrix = rotIndex == 1 ? rot90 : rotIndex == 2 ? rot180 : rot270;
            if (matrix.ContainsKey(type))
                type = matrix[type];
        }
        return type;
    }

    private void SaveBBox(MeshFilter mesh, List<Bounds> list, Vector3 dir, float xdim, float ydim)
    {
        var bbox = new Bounds(mesh.transform.position + (dir * 0.5f), new Vector3(xdim, ydim, 1));
        list.Add(bbox);
    }

    private void CombineBBox(List<Bounds> list)
    {
        while (list.Count > 0)
        {
            var bbox = list[0];
            list.RemoveAt(0);

            var expanding = true;
            while (expanding)
            {
                var remove = new List<Bounds>();
                foreach (var box in list)
                {
                    if (box.Intersects(bbox))
                    {
                        bbox.Encapsulate(box);
                        remove.Add(box);
                    }
                }

                expanding = remove.Count > 0;
                foreach (var box in remove)
                    list.Remove(box);
            }

            CreateBlocker(bbox, "blocker");
        }
    }

    private void CreateBlocker(Bounds bbox, string layer)
    {
        GameObject obj = (GameObject)Instantiate(blockerPrefab, blockerBase.transform);
        obj.layer = LayerMask.NameToLayer(layer);
        obj.transform.position = bbox.center;
        obj.name = "blocker";

        var col = obj.GetComponent<BoxCollider>();
        col.size = new Vector3(bbox.size.x, 2, bbox.size.y);
    }

    private bool approx(float a, float b)
    {
        return (Mathf.Abs(a - b) < 0.001f);
    }
}
