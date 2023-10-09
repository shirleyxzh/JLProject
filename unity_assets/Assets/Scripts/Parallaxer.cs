using UnityEngine;

public class Parallaxer : MonoBehaviour
{
    public float wallHeight = 1f;
    public bool combineRooms = false;

    private Parallax[] sections;
    private Vector3 targetOffset = Vector3.zero;
    private Bounds bounds;

    private void Start()
    {
        var off = Vector3.up * wallHeight;
        targetOffset = new Vector3(off.x, 0, off.y);

        sections = GetComponentsInChildren<Parallax>();
        foreach (var s in sections)
        {
            bounds.Encapsulate(s.bounds);
            if (combineRooms)
                s.OnUpdate(targetOffset);
        }
    }

    private void LateUpdate()
    {
        if (combineRooms)
            return;

        var off = Vector3.zero;
        var cam = Camera.main.transform.position;
        foreach (var s in sections)
        {
            if (s.InsideBounds(cam, out off))
                break;
        }
        off *= wallHeight;
        targetOffset = new Vector3(off.x, 0, off.y);

        foreach (var s in sections)
        {
            s.OnUpdate(targetOffset);
        }
    }
}
