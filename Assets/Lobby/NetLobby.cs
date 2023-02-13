using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace LobbyUI {
    [System.Serializable]
    public struct PlayerSync {
        public int connId;          // connId of the player, but only can be used by server because client don't know its connid on server;
        public string player_name;  // the nickname that player inputs before enterring lobby
        public bool owning_ava;   // whether taking a robot
        public string ava_tag;   // name of robot taken by the player, must be one of mainmenu.robo_names;
        public bool ready;          // whether ready to start after taking robot
        PlayerSync(int connId, string player_name, bool owning_ava, string ava_tag, bool ready) {
            this.connId = connId;
            this.player_name = player_name;
            this.owning_ava = owning_ava;
            this.ava_tag = ava_tag;
            this.ready = ready;
        }
    }


    /// <summary>
    /// @job: manage network lobby, especially about events and their synchronization
    /// @Style: Event/Call style
    /// </summary>
    public class NetLobby : NetworkBehaviour {
        public const string NULLAVA = null;
        BattleNetworkManager net_man => BattleNetworkManager.singleton;
        [HideInInspector]
        public int uid;     // unique identifier for each client


        /** Tip: nested struct declaration => Netlobby.AvatarMessage instead of AvatarMessage.
            direction: client -> server
         */
        // tell server PC that local client PC wants the robot
        public struct AvaOwnMessage : NetworkMessage {
            public string robot_s;
            public string player_name;
            /* construct function with params */
            public AvaOwnMessage(string robot_s, string player_name) {
                this.robot_s = robot_s;
                this.player_name = player_name;
            }
        }
        // tell server PC that local client PC is ready
        public struct AvaReadyMessage : NetworkMessage {
            public bool ready;
        }
        // tell client about its connId in server PC's scene
        public struct ClientIdMessage : NetworkMessage {
            public int connId_onserver; // client PC's id on server scene
            public ClientIdMessage(int connId_onserver) {
                this.connId_onserver = connId_onserver;
            }
        }
        // sent by lobby owner to tell server to start the game
        public struct StartGameMessage : NetworkMessage {
            public bool start;
            public StartGameMessage(bool start) { this.start = start; }
        }
        /* tell client PC to play transition animation */
        public struct SceneTransMessage : NetworkMessage {
            public bool trans;     // playing scene transition anim or not. if true, anim will be played 
            public SceneTransMessage(bool trans) { this.trans = trans; }
        }


        /// <summary>
        /// Network Variables:
        public readonly SyncList<PlayerSync> playerSyncs = new SyncList<PlayerSync>();
        [SyncVar]
        [HideInInspector]
        public int owner_uid; // lobby owner
        [SyncVar]
        [HideInInspector]
        public bool allow_join;
        /// </summary> 

        /// <summary>
        /// Local Variables
        public MainMenu mainmenu;
        /// </summary>


        [Server]
        public void OnPlayerLeave(NetworkConnectionToClient conn) {
            int connId = conn.connectionId;
            int syncIdx = playerSyncs.FindIndex(i => i.connId == connId);
            if (syncIdx == -1)
                return;
            else
                playerSyncs.RemoveAt(syncIdx);
        }

        /* OnApplyAvatar():
            1. registers the PlayerSync when a client PC first applys avatar
            2. ensures that any registered client only has one corresponding PlayerSync
           Therefore, Inspite of real take-avatar mes, you can send fake mes:
            1. to register in playerSyncs
            2. to give up owning avatar
        */
        [Server]
        public void OnRecAvaOwnMes(NetworkConnectionToClient conn, AvaOwnMessage mes) {
            bool is_avatar_taken = (-1 != playerSyncs.FindIndex(i => i.owning_ava && i.ava_tag == mes.robot_s));
            if (is_avatar_taken)
                Debug.Log("server: the robot is taken!");
            else {
                /* make a new PlayerStateSync */
                PlayerSync tmp = new PlayerSync();
                tmp.connId = conn.connectionId;
                tmp.player_name = mes.player_name;
                bool fake_mes = !mainmenu.ava_tags.Contains(mes.robot_s);
                if (fake_mes) {
                    tmp.owning_ava = false;
                    tmp.ready = false;
                    tmp.ava_tag = NULLAVA;
                    int idx = playerSyncs.FindIndex(i => i.connId == conn.connectionId);
                    if (idx == -1)
                        playerSyncs.Add(tmp);
                    else
                        playerSyncs[idx] = tmp;
                } else {
                    tmp.owning_ava = true;
                    tmp.ready = true;
                    tmp.ava_tag = mes.robot_s;
                    int idx = playerSyncs.FindIndex(i => i.connId == conn.connectionId);
                    playerSyncs[idx] = tmp;
                }
            }
        }

        [Server]
        public void OnRecAvaReadyMes(NetworkConnectionToClient conn, AvaReadyMessage mes) {
            int id_cli = conn.connectionId;
            int syncIdx = playerSyncs.FindIndex(i => i.connId == id_cli);
            if (syncIdx == -1) {
                Debug.Log("A client PC owning no avatar trys to change avatar state");
                return;
            } else {
                PlayerSync ps = new PlayerSync();
                ps = playerSyncs[syncIdx];
                ps.ready = mes.ready;
                playerSyncs[syncIdx] = ps;
            }
            return;
        }

        [Server]
        public void OnRecStartGameMes(NetworkConnectionToClient conn, StartGameMessage mes) {
            if (mes.start) {
                net_man.playerSyncs = new List<PlayerSync>(this.playerSyncs);
                net_man.StartCoroutine(MyServerChangeScene());
            }
        }

        [Client]
        public void OnPlayerSyncChanged(SyncList<PlayerSync>.Operation op, int index, PlayerSync oldval, PlayerSync newval) {
            /* if a new value is added to synclist, then 
                @oldval: new PlayerSync()
                @newval: newly-added object (PlayerSync) 
                @index: newval's index in synclist
            */
            /* step 1: reset old avatar */
            if (oldval.ava_tag != null) {
                int avaIdx = mainmenu.ava_tags.FindIndex(tag => tag == oldval.ava_tag);
                mainmenu.avatars[avaIdx].RstRoboTab();
            }
            /* step 2: set corresponding AvatarTab according to newval  */
            if (newval.ava_tag != null) {
                int avaIdx = mainmenu.ava_tags.FindIndex(tag => tag == newval.ava_tag);
                mainmenu.avatars[avaIdx].SetRoboTab(newval);
            }
            int syncIdx = this.playerSyncs.FindIndex(p => p.connId == this.uid);
            if (syncIdx != -1) {
                mainmenu.owning_ava = playerSyncs[syncIdx].owning_ava;
                mainmenu.ava_ready = playerSyncs[syncIdx].ready;
                mainmenu.SetButtonReady();
            }
            // /* log in every client */
            // Debug.Log("playerSyncs changed!");
        }

        [Client]
        public void OnRecCliIdMes(ClientIdMessage mes) {
            // Debug.Log("receive server-assign connId: " + mes.connId_onserver);
            this.uid = mes.connId_onserver;
        }

        // call all clients to play SceneTrans Anim 
        // and then call net_man.ServerChangeScene
        [Server]
        IEnumerator MyServerChangeScene() {
            NetworkServer.SendToAll<SceneTransMessage>(new SceneTransMessage(true));
            yield return new WaitForSeconds(5);
            Debug.Log("net_man starts to change scene");
            net_man.ServerChangeScene(net_man.scn_field);
            yield break;
        }

        [Client]
        public void OnRecScnTransMes(SceneTransMessage mes) {
            // mainmenu is not an network obj, so every client 
            // has to call mainmenu disable it instead of calling it once in server PC
            mainmenu.DisableAllMenus();
            if (mes.trans) {
                Debug.Log("OnRecScnTransMes");
                SceneTransit.singleton.StartTransit();
                Debug.Log("StartTransit");
            }
        }

        public override void OnStartClient() {
            base.OnStartClient();

            /* when first joining, 1. send fake AvaMes to register
                2. init AvaTabs as playerSyncs */
            AvaOwnMessage fake_ava_mes = new AvaOwnMessage(NULLAVA, mainmenu.input_info.text);
            NetworkClient.Send<AvaOwnMessage>(fake_ava_mes);
            foreach (PlayerSync tmp in playerSyncs) {
                /* all clients has a corresponding PlayerSync, 
                    yet it's possible that not every client owns avatar */
                if (tmp.owning_ava) {
                    int avaIdx = mainmenu.ava_tags.FindIndex(tag => tag == tmp.ava_tag);
                    mainmenu.avatars[avaIdx].SetRoboTab(tmp);
                }
            }
            /* first client is owner */
            this.owner_uid = this.playerSyncs[0].connId;
        }

        public override void OnStopClient() {
            base.OnStopClient();

            /* update owner id */
            if (this.playerSyncs.Count > 0)
                this.owner_uid = this.playerSyncs[0].connId;
        }

    }
}