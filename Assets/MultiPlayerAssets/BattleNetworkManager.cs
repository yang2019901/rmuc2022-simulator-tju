using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BattleNetworkManager : NetworkManager {
    public RMUC_UI.NetLobby net_lob;
    public RMUC_UI.MainMenu mainmenu;

    /* called on server when a client is connected to server */
    /// <summary>
    /// here is to verify
    /// </summary>
    public override void OnServerConnect(NetworkConnectionToClient conn) {
        base.OnServerConnect(conn);
        Debug.Log("Hey, there! A client connects. ConnId: " + conn.connectionId);
        conn.Send<RMUC_UI.NetLobby.ClientIdMessage>(new RMUC_UI.NetLobby.ClientIdMessage(conn.connectionId));
    }

    /* called on server when a client is disconnected */
    public override void OnServerDisconnect(NetworkConnectionToClient conn) {
        base.OnServerDisconnect(conn);
        Debug.Log("Hey, there! A client disconnects. ConnId: " + conn.connectionId);
        net_lob.OnPlayerLeave(conn);
    }

    public override void OnStartServer() {
        base.OnStartServer();
        NetworkServer.RegisterHandler<RMUC_UI.NetLobby.AvatarMessage>(net_lob.OnApplyAvatar);
        NetworkServer.RegisterHandler<RMUC_UI.NetLobby.AvaStateMessage>(net_lob.OnInvAvaReady);
        /* clear playerSyncs, otherwise, previous items are there */
        net_lob.playerSyncs.Reset();
    }
    
    public override void OnClientConnect() {
        base.OnClientConnect();
        net_lob.playerSyncs.Callback += net_lob.OnPlayerSyncChanged;
        NetworkClient.RegisterHandler<RMUC_UI.NetLobby.ClientIdMessage>(net_lob.OnReceiveConnId);
        Debug.Log("register handler in net_man");
    }

    public override void OnClientDisconnect() {
        base.OnClientDisconnect();
        net_lob.playerSyncs.Callback -= net_lob.OnPlayerSyncChanged;
        mainmenu.SetPlayerOpt();
    }


}