using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem.HID;

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

    void PlayerDied()
    {
    }

    void PlayerHit(int hits, int hp)
    {
        hud.text = $"Hits: {hits}\nHP: {hp}";
    }
}
