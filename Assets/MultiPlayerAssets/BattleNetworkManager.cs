using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BattleNetworkManager : NetworkManager {
    /* called on server when a client is connected to server */
    /// <summary>
    /// here is to verify
    /// </summary>
    public override void OnServerConnect(NetworkConnectionToClient conn) {
        base.OnServerConnect(conn);
        Debug.Log("Hey, there! Another client connects. ConnId: " + conn.connectionId);
    }
}