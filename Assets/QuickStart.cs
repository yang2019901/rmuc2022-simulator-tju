using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LobbyUI;
using Mirror;

public class QuickStart : MonoBehaviour {
    // Start is called before the first frame update
    public BasicController robot;
    [Tooltip("unlimited")]
    public bool godmode = true;

    void Start() {
        if (!NetworkServer.active && !NetworkClient.active) {
            BattleNetworkManager.singleton.StartHost();
            BattleNetworkManager.singleton.playerSyncs.Add(new PlayerSync(0, "admin", true, robot.name, true));
            BattleNetworkManager.singleton.ServerChangeScene("BattleField");
        }
    }

    void Update() {
        if (godmode) {
            RoboState rs = robot.robo_state;
            Weapon wpn = robot.GetComponent<Weapon>();
            rs.maxblood = 1000;
            rs.currblood = 1000;
            rs.bullspd = 20;
            wpn.bullnum = 100;
            wpn.currheat = 0;
            BattleField.singleton.money_red = 1000;
            BattleField.singleton.money_red_max = 1000;
            BattleField.singleton.money_blue = 1000;
            BattleField.singleton.money_blue_max = 1000;
        }

    }
}