using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/* record which player/client owns which robot; survive scene switching and provide mes to battlefield (new scene) */
public class DataManager : NetworkBehaviour
{
    public static DataManager singleton;
    /* data will be sync between server and client, for UI effect */
    public readonly SyncDictionary<string, int> connId_robot;

    void Awake() {
        if (singleton == null) {
            singleton = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(this);
    }

    public void TakeRobot(string robot_s, int conn_id) {
        if (connId_robot.ContainsKey(robot_s))
            Debug.Log(robot_s + " applied by conn_id: " + conn_id 
                + " has been taken by conn_id: " + connId_robot[robot_s]);
        else {
            connId_robot.Add(robot_s, conn_id);
            Debug.Log(robot_s + " applied by conn_id: " + conn_id + " has been taken");
        }
    }
}
