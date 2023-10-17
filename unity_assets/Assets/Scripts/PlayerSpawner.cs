using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject PlayerPrefab;

    [SerializeField]
    private Transform spawnPoint;

    [SerializeField]
    private TextMeshProUGUI hud;

    [SerializeField]
    private TextMeshProUGUI hudKills;

    [SerializeField]
    private FollowCam Camera;

    [SerializeField]
    private GridMgr gridMgr;

    public Agent agent { get; private set; }

    private void Awake()
    {
        var obj = Instantiate(PlayerPrefab);
        var pos = spawnPoint.position + Vector3.back;
        obj.transform.position = pos;

        Camera.Player = obj;

        agent = obj.GetComponent<Agent>();
        var pi = agent.GetComponent<PlayerInput>();
        pi.KillsCB.AddListener(EnemiesKilled);
        pi.DeathCB.AddListener(PlayerDied);
        pi.RotRoomCB.AddListener(RotRoom);
        pi.HitCB.AddListener(PlayerHUD);

        pi.gridProxy = gridMgr.CreateProxy(pos);
        agent.destProxy = gridMgr.CreateProxy(pos);
    }

    private void Start()
    {
        EnemiesKilled(0);
        PlayerHUD(0, agent.GetHP);
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

    void PlayerHUD(int hits, int hp)
    {
        hud.text = $"Hits: {hits}\nHP: {hp}";
    }

    void EnemiesKilled(int kills)
    {
        hudKills.text = $"Kills: {kills}";
    }

    void RotRoom(bool rotCW)
    {
        gridMgr.Rotate(rotCW, agent.GetPostion);
    }
}
