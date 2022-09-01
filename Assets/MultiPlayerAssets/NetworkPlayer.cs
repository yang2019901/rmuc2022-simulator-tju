using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkPlayer : NetworkBehaviour {
    /* data will be sync between server and client, for UI effect */
    public readonly SyncDictionary<string, int> connId_robot = new SyncDictionary<string, int>();

    public bool owning = false;
    public string player_name;

    [Command]
    public void CmdTakeRobot(string robot_s, int conn_id) {
        if (connId_robot.ContainsKey(robot_s))
            Debug.Log(robot_s + " applied by conn_id: " + conn_id
                + " has been taken by conn_id: " + connId_robot[robot_s]);
        else {
            connId_robot.Add(robot_s, conn_id);
            Debug.Log(robot_s + " applied by conn_id: " + conn_id + " has been taken");
        }
    }

    void Update() {
        /* set UI by ownership of robots */
    }
}
