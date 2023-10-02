using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject PlayerPrefab;

    [SerializeField]
    private Vector3 spawnPoint;

    [SerializeField]
    private TextMeshProUGUI hud;

    [SerializeField]
    private FollowCam Camera;

    public Agent player { get; private set; }

    private void Awake()
    {
        var obj = Instantiate(PlayerPrefab);
        obj.transform.position = spawnPoint;

        Camera.Player = obj;

        player = obj.GetComponent<Agent>(); ;
        var pi = obj.GetComponent<PlayerInput>();
        pi.DeathCB.AddListener(PlayerDied);
        pi.HitCB.AddListener(PlayerHit);
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

    void PlayerDied()
    {
    }

    void PlayerHit(int hits, int hp)
    {
        hud.text = $"Hits: {hits}\nHP: {hp}";
    }
}
