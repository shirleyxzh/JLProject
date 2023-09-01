using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LayoutMgr : MonoBehaviour
{
    
    public Vector2 MinLimits;
    public Vector2 MaxLimits;
    public TextMeshProUGUI Hits;

    private int totalHits = 0;
    private float hpCurrent = 0;

    private void Start()
    {
        Hits.text = $"HITS: {totalHits}\nHP: {hpCurrent}";
    }

    public void LimitMovement(GameObject go)
    {
        var newPos = go.transform.position;
        newPos.x = Mathf.Min(MaxLimits.x, Mathf.Max(MinLimits.x, newPos.x));
        newPos.z = Mathf.Min(MaxLimits.y, Mathf.Max(MinLimits.y, newPos.z));
        go.transform.position = newPos; 
    }

    public void PlayerHP(float hpCount)
    {
        hpCurrent = hpCount;
    }

    public void PlayerWasHit()
    {
        totalHits++;
        hpCurrent--;
        Hits.text = $"HITS: {totalHits}\nHP: {hpCurrent}";
      
    }
}
