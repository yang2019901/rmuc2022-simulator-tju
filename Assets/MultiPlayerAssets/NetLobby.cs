using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public struct PlayerStateSync {
    public int connId;          // connId of the player
    public string player_name;  // the nickname that player inputs before enterring lobby
    public bool owning_robot;   // whether taking a robot
    public string robo_name;   // name of robot taken by the player, must be one of mainmenu.robo_names;
    public bool ready;          // whether ready to start after taking robot
}
/// <summary>
/// Orientation: define visual state and sync them; also do RPC
/// </summary>
public class NetLobby : NetworkBehaviour {
    public readonly SyncList<PlayerStateSync> player_sync_all = new SyncList<PlayerStateSync>();
    /* lobby owner */
    [SyncVar]
    public int owner_connId;
    [SyncVar]
    public bool allow_join;

    public MainMenu mainmenu;

    [Command]
    public void CmdTakeRobot(string robot_s, int connId, string player_name) {
        bool is_robot_taken = (-1 == player_sync_all.FindIndex(i => i.owning_robot && i.robo_name==robot_s));
        if (is_robot_taken)
            Debug.Log("server: the robot is taken!");
        else {
            /* make a new PlayerStateSync */
            PlayerStateSync tmp = new PlayerStateSync();
            tmp.connId = connId;
            tmp.player_name = player_name;
            tmp.owning_robot = true;
            tmp.ready = true;
            tmp.robo_name = robot_s;
            /* find player info */
            int idx = player_sync_all.FindIndex(i => i.connId == connId);
            if (idx == -1)
                player_sync_all.Add(tmp);
            else
                player_sync_all[idx] = tmp;
        }
    }

    void Update() {
        if (isClient) {
            /* go over player_sync_all, get every robot that has an owner and set mainmenu.robo_tag  */
            for (int i = 0; i < player_sync_all.Count; i++) {
                if (player_sync_all[i].owning_robot) {
                    int idx = mainmenu.tags.FindIndex(name => name==player_sync_all[i].robo_name);
                    mainmenu.avatars[i].SetAvatar(player_sync_all[i]);
                }
            }
        }
    }
}
