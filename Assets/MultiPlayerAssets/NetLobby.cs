using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace RMUC_UI {
    public struct PlayerSync {
        public int connId;          // connId of the player
        public string player_name;  // the nickname that player inputs before enterring lobby
        public bool owning_robot;   // whether taking a robot
        public string robo_name;   // name of robot taken by the player, must be one of mainmenu.robo_names;
        public bool ready;          // whether ready to start after taking robot
    }
    /// <summary>
    /// @Orientation: define visual state and sync them; also do RPC
    /// @Style: Event/Call style
    /// </summary>
    public class NetLobby : NetworkBehaviour {
        /* nested struct declaration => Netlobby.AvatarMessage instead of AvatarMessage */
        public struct AvatarMessage : NetworkMessage {
            public string robot_s;
            public int connId;
            public string player_name;
            /* construct function with params */
            public AvatarMessage(string robot_s, int connId, string player_name) {
                this.robot_s = robot_s;
                this.connId = connId;
                this.player_name = player_name;
            }
        }

        /// <summary>
        /// Network Variables:
        public readonly SyncList<PlayerSync> player_sync_all = new SyncList<PlayerSync>();
        [SyncVar]
        public int owner_connId; // lobby owner
        [SyncVar]
        public bool allow_join;
        /// </summary> 


        /// <summary>
        /// Local Variables
        public MainMenu mainmenu;
        /// </summary>
        
        public void ApplyAvatar(AvatarMessage mes) {
            bool is_avatar_taken = (-1 == player_sync_all.FindIndex(i => i.owning_robot && i.robo_name==mes.robot_s));
            if (is_avatar_taken)
                Debug.Log("server: the robot is taken!");
            else {
                /* make a new PlayerStateSync */
                PlayerSync tmp = new PlayerSync();
                tmp.connId = mes.connId;
                tmp.player_name = mes.player_name;
                tmp.owning_robot = true;
                tmp.ready = true;
                tmp.robo_name = mes.robot_s;
                /* find player info */
                int idx = player_sync_all.FindIndex(i => i.connId == mes.connId);
                if (idx == -1)
                    player_sync_all.Add(tmp);
                else
                    player_sync_all[idx] = tmp;
            }
        }

        public override void OnStartClient() {
            base.OnStartClient();
            player_sync_all.Callback += OnPlayerSyncChanged;
            /** ApplyAvatar => Action<AvatarMessage>, 
                which tells me that usage of ApplyAvatar should be declared as ApplyAvatar(AvatarMessage mes) */
            NetworkClient.RegisterHandler<AvatarMessage>(ApplyAvatar);
        }

        void OnPlayerSyncChanged(SyncList<PlayerSync>.Operation op, int index, PlayerSync oldval, PlayerSync newval) {
            Debug.Log(string.Format("index: {}, oldval: {}, newval: {}", index, oldval, newval));
            /* go over player_sync_all, get every robot that has an owner and set mainmenu.robo_tag  */
            for (int i = 0; i < player_sync_all.Count; i++) {
                if (player_sync_all[i].owning_robot) {
                    int idx = mainmenu.ava_tags.FindIndex(name => name==player_sync_all[i].robo_name);
                    mainmenu.avatars[i].SetAvatarTab(player_sync_all[i]);
                }
            }
        }
    }
}