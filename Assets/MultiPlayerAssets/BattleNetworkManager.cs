using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BattleNetworkManager : NetworkManager {
    public RMUC_UI.NetLobby net_lob;

    /* called on server when a client is connected to server */
    /// <summary>
    /// here is to verify
    /// </summary>
    public override void OnServerConnect(NetworkConnectionToClient conn) {
        base.OnServerConnect(conn);
        Debug.Log("Hey, there! A client connects. ConnId: " + conn.connectionId);
    }

    /* called on server when a client is disconnected */
    public override void OnServerDisconnect(NetworkConnectionToClient conn) {
        base.OnServerDisconnect(conn);
        Debug.Log("Hey, there! A client disconnects. ConnId: " + conn.connectionId);
        net_lob.OnPlayerLeave(conn);
    }
}