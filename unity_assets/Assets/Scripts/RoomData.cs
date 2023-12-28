using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomData : MonoBehaviour
{
    public enum RoomTypes
    {
        Square,
        L_Shape,
        Z_Shape,
        T_Shape,
        I_Shape,
        S_Shape,
        L2_Shape
    }

    [SerializeField] private int MaxSpawnPerSpot = 10;
    [SerializeField] private GameObject Filter;
    [SerializeField] private GameObject SpawnPoints;
    [SerializeField] private Parallax Tiles;
    [SerializeField] private Parallax Outline;
    [SerializeField] private RoomTypes roomType;

    public List<Parallax.meshInfo> GetTiles => Tiles.meshList;
    public List<Parallax.meshInfo> GetOutline => Outline.meshList;
    public int GetRotationIndex => Mathf.FloorToInt((transform.localEulerAngles.y + 360) % 360) / 90;    // return 0-3

    private Color validColor = new Color(0, 1, 0, 0.1f);
    private Color invalidColor = new Color(1, 0, 0, 0.1f);

    public RoomTypes GetRoomType()
    {
        return roomType;
    }

    public bool IsType(RoomTypes inRoomType)
    {
        return roomType == inRoomType;
    }

    public List<GameObject> GetSpawnPoints()
    {
        var spawns = new List<GameObject>();
        foreach (Transform point in SpawnPoints.transform)
        {
            spawns.Add(point.gameObject);
            point.gameObject.SetActive(true);
        }
        return spawns;
    }

    public bool IsOnGrid(Bounds gridBounds)
    {
        var bbox = Tiles.boxCollider.bounds;
        bbox.Expand(-0.1f);
        if (!(gridBounds.Contains(bbox.min) && gridBounds.Contains(bbox.max)))
            return false;

        // bbox must be on multiple of 4 boundry
        return (Mathf.RoundToInt(Mathf.Abs(bbox.min.x)) % 4) == 0
                && (Mathf.RoundToInt(Mathf.Abs(bbox.min.y)) % 4) == 0;
    }

    public bool IsBlockedByIcon(GameObject gridIcons)
    {
        foreach (Transform spawn in SpawnPoints.transform)
        {
            foreach (Transform child in gridIcons.transform)
            {
                if ((Mathf.Abs(child.position.x - spawn.position.x) < 0.001f)
                    && (Mathf.Abs(child.position.y - spawn.position.y) < 0.001f))
                {
                    return spawn.gameObject.activeSelf;
                }
            }
        }
        return false;
    }

    public bool ContainsIcon(GameObject gridIcon)
    {
        var quads = Filter.GetComponentsInChildren<BoxCollider>(true);
        foreach (var quad in quads)
        {
            if (quad.bounds.Contains(gridIcon.transform.position))
                return true;
        }
        return false;
    }

    public void EnableFilter(bool enable)
    {
        SetFilterColor(Color.clear);
        Filter.SetActive(enable);
    }

    public void EnableOutline(bool enable)
    {
        Outline.gameObject.SetActive(enable);
    }

    public GameObject GetFilter()
    {
        return Filter;
    }

    public void UpdateFilter(bool isValid)
    {
        var color = isValid ? validColor : invalidColor;
        SetFilterColor(color);
    }

    private void SetFilterColor(Color color)
    {
        var rends = Filter.GetComponentsInChildren<Renderer>();
        foreach (var rend in rends)
            rend.material.SetColor("_Color", color);
    }

    public void MoveAndRot(Vector3 newPos, int rotDir, Bounds gridBounds)
    {
        // TODO: keep room on multiple of 4 boundry

        transform.position = new Vector3(Mathf.RoundToInt(newPos.x), Mathf.RoundToInt(newPos.y), 0);
        transform.RotateAround(transform.position, Vector3.forward, 90 * rotDir);
        Outline.SetRotVector(rotDir);
        Tiles.SetRotVector(rotDir);

        // TODO: keep room within grid bounds
    }

    public void HideSpawnPoints()
    {
        SpawnPoints.SetActive(false);
    }

    public int GetMaxSpawnPerSpot()
    {
        return MaxSpawnPerSpot; 
    }
}
