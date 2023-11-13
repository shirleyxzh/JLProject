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

    public bool IsType(RoomTypes inRoomType)
    {
        return roomType == inRoomType;
    }

    public bool GetSpawnPoint(string name, out GameObject point)
    {
        point = null;
        var obj = SpawnPoints.transform.Find(name);
        if (obj)
            point = obj.gameObject;
        return obj != null;
    }

    public bool IsOnGrid(Bounds bounds)
    {
        return Tiles.isInside(bounds);
    }

    public void EnableFilter(bool enable)
    {
        Filter.SetActive(enable);
    }

    public void EnableOutline(bool enable)
    {
        Outline.gameObject.SetActive(enable);
    }

    public void UpdateFilter(bool isValid)
    {
        var color = isValid ? validColor : invalidColor;
        var rends = Filter.GetComponentsInChildren<Renderer>();
        foreach (var rend in rends)
            rend.material.SetColor("_Color", color);
    }

    public void MoveAndRot(Vector3 pos, int rotDir)
    {
        transform.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), 0);
        transform.RotateAround(transform.position, Vector3.forward, 90 * rotDir);
        Outline.SetRot(rotDir);
        Tiles.SetRot(rotDir);
    }
}
