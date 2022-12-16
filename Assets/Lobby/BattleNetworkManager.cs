using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

using LobbyUI;

public class BattleNetworkManager : NetworkManager {
    public new static BattleNetworkManager singleton;
    public NetLobby net_lob;
    public MainMenu mainmenu;
    [Scene]
    public string scn_field;
    [Scene]
    public string scn_lobby;
    /* used to transfer data when scene loads */
    [HideInInspector]
    public List<PlayerSync> playerSyncs = new List<PlayerSync>();


    public override void Awake() {
        if (singleton == null) {
            singleton = this;
        } else
            Destroy(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    bool ScnCmp(string scn_runtime, string scn_asset) {
        return scn_asset.Contains(scn_runtime);
    }

    public bool isScnLobby() => ScnCmp(SceneManager.GetActiveScene().name, scn_lobby);
    public bool isScnField() => ScnCmp(SceneManager.GetActiveScene().name, scn_field);

    public override void OnStartServer() {
        base.OnStartServer();
        if (net_lob == null)
            return;
        NetworkServer.RegisterHandler<NetLobby.AvaOwnMessage>(net_lob.OnRecAvaOwnMes);
        NetworkServer.RegisterHandler<NetLobby.AvaReadyMessage>(net_lob.OnRecAvaReadyMes);
        NetworkServer.RegisterHandler<NetLobby.StartGameMessage>(net_lob.OnRecStartGameMes);
        /* clear playerSyncs, otherwise, previous items are there */
        net_lob.playerSyncs.Reset();
    }


    public override void OnStopServer() {
        base.OnStopServer();
        Debug.Log("server: stop server");
    }


    /* called on server when a client is connected to server */
    public override void OnServerConnect(NetworkConnectionToClient conn) {
        base.OnServerConnect(conn);
        Debug.Log("Hey, there! A client connects. ConnId: " + conn.connectionId);
        if (isScnLobby()) {
            conn.Send<NetLobby.ClientIdMessage>(new NetLobby.ClientIdMessage(conn.connectionId));
        }
    }


    /* called on server when a client is disconnected */
    public override void OnServerDisconnect(NetworkConnectionToClient conn) {
        base.OnServerDisconnect(conn);
        Debug.Log("Hey, there! A client disconnects. ConnId: " + conn.connectionId);
        if (isScnLobby())
            net_lob.OnPlayerLeave(conn);
    }


    public override void OnServerSceneChanged(string sceneName) {
        base.OnServerSceneChanged(sceneName);
        // Debug.Log(sceneName + " has been loaded");
        if (isScnField()) {
            string log = "";
            /* BattleField having been loaded, assign robot instance to avatar owner */
            foreach (RoboState robot in BattleField.singleton.robo_all) {
                int syncIdx = playerSyncs.FindIndex(i => i.ava_tag == robot.name);
                if (syncIdx == -1)
                    log += "no player takes " + robot.name + "\n";
                else {
                    NetworkConnectionToClient connToClient = NetworkServer.connections[playerSyncs[syncIdx].connId];
                    robot.GetComponent<NetworkIdentity>().AssignClientAuthority(connToClient);
                    log += playerSyncs[syncIdx].player_name + " takes " + robot.name + "\n";
                }
            }
            Debug.Log(log);
        }
    }



    public override void OnClientConnect() {
        base.OnClientConnect();
        if (mainmenu == null || net_lob == null)
            return;
        mainmenu.SetPlayerLobby();
        net_lob.playerSyncs.Callback += net_lob.OnPlayerSyncChanged;
        NetworkClient.RegisterHandler<NetLobby.ClientIdMessage>(net_lob.OnRecCliIdMes);
        NetworkClient.RegisterHandler<NetLobby.SceneTransMessage>(net_lob.OnRecScnTransMes);
        // Debug.Log("register handler in net_man");
    }


    /* called on that client when a client is stopped (disconnected included) */
    public override void OnStopClient() {
        base.OnStopClient();
        Destroy(NetworkClient.localPlayer.gameObject);
        Debug.Log("client PC: stop client; connected: " + NetworkClient.isConnected);
        if (NetworkClient.isConnected) {
            // clear what OnClientConnect() has done:
            if (isScnLobby()) {
                net_lob.playerSyncs.Callback -= net_lob.OnPlayerSyncChanged;
                mainmenu.SetPlayerMode();
            } else if (isScnField()) {
                SceneManager.LoadScene(scn_lobby);
            }
        } else {
            // mainmenu's attempting to connect but fail
            // call mainmenu to do finishing touches 
            mainmenu.OnCancelJoin();
        }
    }


    // since battlenetworkmanager is singleton that won't be destroyed when scene changes 
    // but this.net_lob and this.mainmenu will be destroyed
    // so every time new scene is loaded, singleton will find the new net_lob and mainmenu
    void OnSceneLoaded(Scene scn, LoadSceneMode mode) {
        this.net_lob = FindObjectOfType<NetLobby>(includeInactive: true);
        this.mainmenu = FindObjectOfType<MainMenu>(includeInactive: true);
    }

}