using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI hud;    // TODO: move to mgr class

    [SerializeField]
    private TextMeshProUGUI hudKills;   // TODO: move to mgr class

    [SerializeField]
    private GridMgr gridMgr;        // TODO: move to mgr class

    private Agent agent;
    private PlayerInput pi;
    public Agent Player => agent;
    public GridMgr GridMgr => gridMgr;

    // debug
    private bool godModeOn = false;

    public GameObject CreateAvatar(GameObject avatar, Vector3 pos, Vector3 lookDir)
    {
        var obj = Instantiate(avatar, pos, Quaternion.identity);
        obj.GetComponent<Agent>().SetupForSelection(lookDir);
        return obj;
    }

    public void InitPlayer(GameObject player, GameObject spawnPoint)
    {
        agent = player.GetComponent<Agent>();
        pi = agent.GetComponent<PlayerInput>();
        pi.KillsCB.AddListener(EnemiesKilled);
        pi.DeathCB.AddListener(PlayerDied);
        pi.RotRoomCB.AddListener(RotRoom);
        pi.HitCB.AddListener(PlayerHUD);
        
        var pos = spawnPoint.transform.position + Vector3.back;
        agent.destProxy = gridMgr.CreateProxy(pos);
        pi.gridProxy = gridMgr.CreateProxy(pos);
        agent.transform.position = pos;
    }

    public void StartLevel()
    {
        EnemiesKilled(0);
        PlayerHUD(0, agent.GetHP);

        pi.StartLevel();
    }

    public void EndLevel()
    {
        pi.EndLevel();
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
        // TODO: move to Tetriria class and add total
        hudKills.text = $"Kills: {kills}";
    }

    void RotRoom(bool rotCW)
    {
        gridMgr.StepRotate(rotCW, agent.GetPostion);
    }

    public void ToggleGodMode()
    {
        if (agent != null)
        {
            godModeOn = !godModeOn;
            agent.GodMode = godModeOn;
            hud.color = godModeOn ? Color.yellow : Color.white;
        }
    }

    public void SetOneShot(bool enabled)
    {
        hudKills.color = enabled ? Color.red : Color.white;
    }
}
