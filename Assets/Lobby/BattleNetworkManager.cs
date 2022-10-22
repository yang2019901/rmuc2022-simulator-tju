using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

using LobbyUI;

public class BattleNetworkManager : NetworkManager {
    public NetLobby net_lob;
    public MainMenu mainmenu;
    [Scene]
    public string scn_field;
    [Scene]
    public string scn_lobby;
    /* used to transfer data when scene loads */
    [HideInInspector]
    public List<PlayerSync> playerSyncs = new List<PlayerSync>();

    bool ScnCmp(string scn_runtime, string scn_asset) {
        return scn_asset.Contains(scn_runtime);
    }

    public bool isScnLobby() => ScnCmp(SceneManager.GetActiveScene().name, scn_lobby);
    public bool isScnField() => ScnCmp(SceneManager.GetActiveScene().name, scn_field);

    public override void OnStartServer() {
        base.OnStartServer();
        NetworkServer.RegisterHandler<NetLobby.AvaOwnMessage>(net_lob.OnApplyAvatar);
        NetworkServer.RegisterHandler<NetLobby.AvaReadyMessage>(net_lob.OnInvAvaReady);
        NetworkServer.RegisterHandler<NetLobby.StartGameMessage>(net_lob.OnStartGame);
        /* clear playerSyncs, otherwise, previous items are there */
        net_lob.playerSyncs.Reset();
    }


    /* called on that client when a client is disconnected */
    public override void OnStopClient() {
        base.OnStopClient();
        Debug.Log("client: stop client");
        if (isScnLobby()) {
            net_lob.playerSyncs.Callback -= net_lob.OnPlayerSyncChanged;
            mainmenu.SetPlayerMode();
        } else if (isScnField()) {
            SceneManager.LoadScene(scn_lobby);
            Destroy(this.gameObject);
        }
    }


    public override void OnStopServer() {
        base.OnStopServer();
        Debug.Log("server: stop server");
        if (isScnField()) {
            SceneManager.LoadScene(scn_lobby);
            Destroy(this.gameObject);
        }
    }


    /* called on server when a client is connected to server */
    /// <summary>
    /// here is to verify
    /// </summary>
    public override void OnServerConnect(NetworkConnectionToClient conn) {
        base.OnServerConnect(conn);
        Debug.Log("Hey, there! A client connects. ConnId: " + conn.connectionId);
        conn.Send<NetLobby.ClientIdMessage>(new NetLobby.ClientIdMessage(conn.connectionId));
    }

    /* called on server when a client is disconnected */
    public override void OnServerDisconnect(NetworkConnectionToClient conn) {
        base.OnServerDisconnect(conn);
        Debug.Log("Hey, there! A client disconnects. ConnId: " + conn.connectionId);
        if (isScnLobby())
            net_lob.OnPlayerLeave(conn);
    }

    public override void OnClientConnect() {
        base.OnClientConnect();
        net_lob.playerSyncs.Callback += net_lob.OnPlayerSyncChanged;
        NetworkClient.RegisterHandler<NetLobby.ClientIdMessage>(net_lob.OnReceiveConnId);
        Debug.Log("register handler in net_man");
    }



    public override void OnServerSceneChanged(string sceneName) {
        base.OnServerSceneChanged(sceneName);
        Debug.Log(sceneName + " has been loaded");
        if (isScnField()) {
            /* BattleField having been loaded, assign robot instance to avatar owner */
            foreach (RoboState robot in BattleField.singleton.robo_red) {
                int syncIdx = playerSyncs.FindIndex(i => i.ava_tag == robot.name);
                if (syncIdx == -1)
                    Debug.Log("no player takes " + robot.name);
                else {
                    NetworkConnectionToClient connToClient = NetworkServer.connections[playerSyncs[syncIdx].connId];
                    robot.GetComponent<NetworkIdentity>().AssignClientAuthority(connToClient);
                    Debug.Log(playerSyncs[syncIdx].player_name + " takes " + robot.name);
                }
            }
            foreach (RoboState robot in BattleField.singleton.robo_blue) {
                int syncIdx = playerSyncs.FindIndex(i => i.ava_tag == robot.name);
                if (syncIdx == -1)
                    Debug.Log("no player takes " + robot.name);
                else {
                    NetworkConnectionToClient connToClient = NetworkServer.connections[playerSyncs[syncIdx].connId];
                    robot.GetComponent<NetworkIdentity>().AssignClientAuthority(connToClient);
                    Debug.Log(playerSyncs[syncIdx].player_name + " takes " + robot.name);
                }
            }
        }
    }

}