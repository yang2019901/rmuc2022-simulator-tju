using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;

namespace RMUC_UI {
    /* handle all UI-related events and call corresponding functions in other scripts */
    public class MainMenu : MonoBehaviour {
        public BattleNetworkManager net_man;
        public NetLobby net_lob;

        [Header("info")]
        public GameObject Menu_player_info;
        public TMP_InputField input_info;
        [Header("mode")]
        public GameObject Menu_player_mode;
        [Header("option")]
        public GameObject Menu_player_opt;
        public TMP_InputField input_addr;
        [Header("lobby")]
        public GameObject Menu_player_lobby;
        public GameObject btn_ready;
        [HideInInspector]
        public bool owning_ava = false;
        [HideInInspector]
        public bool ava_ready = false;
        [Header("robot names")]
        [Tooltip("set in Inspector and corresponding to avatar")]
        public List<string> ava_tags;
        public List<AvatarTab> avatars;

        void Start() {
            /* set first menu to be player info menu */
            SetPlayerInfo();
            /* set default player name as computer name */
            input_info.text = System.Environment.GetEnvironmentVariable("ComputerName");
            input_addr.text = "localhost";
        }

        /* start host -> go to lobby -> lobby behaviour -> start game */
        public void SinglePlay() {
            net_man.networkAddress = "localhost";
            net_man.StartHost();
            SetPlayerLobby();
            /* single player => not allow to join */
            net_lob.allow_join = false;
            /* set owner to be local player */
            net_lob.owner_uid = 0;
        }

        public void JoinLobby() {
            net_man.networkAddress = input_addr.text;
            DisableAllMenus();
            Debug.Log(net_man.networkAddress);
            net_man.StartClient();
        }

        public void CreateLobby() {
            net_man.networkAddress = "localhost";
            Debug.Log(net_man.networkAddress);
            net_man.StartHost();
            SetPlayerLobby();
            /* muliplayer => allow to join */
            net_lob.allow_join = true;
            /* Host mode => local connId is 0 */
            net_lob.owner_uid = 0;
        }

        public void Quit() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        /// <summary> switch to another menu
        public void SetPlayerInfo() {
            DisableAllMenus();
            Menu_player_info.SetActive(true);
        }
        public void SetPlayerMode() {
            DisableAllMenus();
            Menu_player_mode.SetActive(true);
        }
        public void SetPlayerOpt() {
            DisableAllMenus();
            Menu_player_opt.SetActive(true);
        }
        public void SetPlayerLobby() {
            DisableAllMenus();
            foreach (AvatarTab avaTab in this.avatars)
                avaTab.ResetAvatarTab();
            /* Don't use NI_obj.SetActive() in client PC. Otherwise, NI_obj won't be spawned properly */
            if (NetworkServer.active)
                Menu_player_lobby.SetActive(true);
            owning_ava = false;
            ava_ready = false;
            SetButtonReady();
        }
        /// <summary>
        /// (wrap-up) set all menus to be inactive (obviously, it's invisible, too)
        /// </summary>
        private void DisableAllMenus() {
            Menu_player_mode.SetActive(false);
            Menu_player_info.SetActive(false);
            Menu_player_opt.SetActive(false);
            Menu_player_lobby.SetActive(false);
        }
        /// </summary>
         
        /// <summary> Lobby Behaviour
        /** under the hood : button click -> TakeAvatar --mes--> (server PC) OnApplyAvatar
            @player_sync: tells which tab is clicked
         */
        public void TakeAvatar(AvatarTab player_sync) {
            /* get to know which AvatarTab player clicked */
            int idx = this.avatars.FindIndex(ava => ava == player_sync);
            NetLobby.AvatarMessage mes = new NetLobby.AvatarMessage(
                this.ava_tags[idx], this.input_info.text);
            /* judge whether user click his avatar. If so, give up taking that avatar */
            PlayerSync ps = net_lob.playerSyncs.Find(i => i.connId == net_lob.uid);
            if (ps.owning_ava && ps.ava_tag == this.ava_tags[idx])
                mes.robot_s = NetLobby.NULLAVA;
            NetworkClient.Send<NetLobby.AvatarMessage>(mes);
        }

        public void InvReady() {
            if (!owning_ava)
                return;
            if (net_lob.owner_uid == net_lob.uid) {
                // start the game
                NetworkClient.Send<NetLobby.StartGameMessage>(new NetLobby.StartGameMessage(true));
                Menu_player_lobby.SetActive(false);
            } else {
                NetLobby.AvaStateMessage mes = new NetLobby.AvaStateMessage();
                mes.ready = !this.ava_ready;
                NetworkClient.Send<NetLobby.AvaStateMessage>(mes);
            }
        }

        public void LeaveLobby() {
            Debug.Log("networkclient.connection.connectionId: " + NetworkClient.connection.connectionId);
            if (NetworkServer.active && NetworkClient.active)
                net_man.StopHost();
            else if (NetworkClient.active)
                net_man.StopClient();
            else
                net_man.StopServer();
            // OnClientDisconnect will take care of loading scene
            SetPlayerOpt();
        }

        public void SetButtonReady() {
            if (this.owning_ava)
                btn_ready.SetActive(true);
            else
                btn_ready.SetActive(false);
            
            TMP_Text btn_txt = btn_ready.GetComponentInChildren<TMP_Text>();
            /* Note: owner can start game any time he wants 
                Therefore, he has button text of <start game> instead of <ready>*/
            if (net_lob.owner_uid == net_lob.uid) {
                btn_txt.text = "<color=#28DD00><Start Game></color>";
            } else {
                btn_txt.text = this.ava_ready ? "<color=#DCDD00><Cancel Ready></color>" // yellow text
                    : "<color=#28DD00><Ready></color>"; // green text
            }

        }
        /// </summary>

    }
}