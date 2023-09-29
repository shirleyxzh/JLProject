using UnityEngine;
using System.Collections;
using UnityEngine.XR;
using static Unity.VisualScripting.Metadata;
using System.Collections.Generic;

public class BlockerMaker : MonoBehaviour
{
    public GameObject blockerPrefab;

    public GameObject createObject()
    {
        // delete blockers base obj
        var root = transform.parent.Find("Blockers");
        if (root != null)
            DestroyImmediate(root.gameObject);

        // create a new one
        var baseObj = new GameObject("Blockers", typeof(Parallax));
        baseObj.transform.SetParent(transform.parent, false);

        // add blockers to base
        if (blockerPrefab != null)
        {
            // create a bbox for every active roof tile
            var bboxes = new List<Bounds>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var obj = transform.GetChild(i);
                if (obj.gameObject.activeSelf)
                {
                    var bbox = new Bounds(obj.position, Vector3.one);
                    bboxes.Add(bbox);
                }
            }

            while (bboxes.Count > 0)
            {
                var bbox = bboxes[0];
                bboxes.RemoveAt(0);

                var expanding = true;
                while (expanding)
                {
                    var remove = new List<Bounds>();
                    foreach (var box in bboxes)
                    {
                        var adjacent = false;
                        if (bbox.size.y == 1f
                            && approx(box.center.y, bbox.center.y)
                            && (approx(box.max.x, bbox.min.x) || approx(box.min.x, bbox.max.x)))
                        {
                            adjacent = true;
                        }
                        else if (bbox.size.x == 1f
                                 && approx(box.center.x, bbox.center.x)
                                 && (approx(box.max.y, bbox.min.y) || approx(box.min.y, bbox.max.y)))
                        {
                            adjacent = true;
                        }

                        if (adjacent)
                        {
                            bbox.Encapsulate(box);
                            remove.Add(box);
                        }
                    }

                    expanding = remove.Count > 0;
                    foreach (var box in remove)
                        bboxes.Remove(box);
                }

                GameObject obj = (GameObject)Instantiate(blockerPrefab, baseObj.transform);
                obj.transform.position = bbox.center;
                obj.name = "blocker";

                // expand blocker size
                bbox.Expand(0.6f);
                var col = obj.GetComponent<BoxCollider>();
                col.size = new Vector3(bbox.size.x, 2, bbox.size.y);
            }
        }

        return baseObj;
    }

    private bool approx(float a, float b)
    {
        return (Mathf.Abs(a - b) < 0.001f);
    }
}