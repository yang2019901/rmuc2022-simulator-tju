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
        public bool owning_ava = false;
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
        }

        /* start host -> go to lobby -> lobby behaviour -> start game */
        public void SinglePlay() {
            net_man.networkAddress = "localhost";
            Debug.Log(net_man.networkAddress);
            net_man.StartHost();
            SetPlayerLobby();
            /* single player => not allow to join */
            net_lob.allow_join = false;
            /* set owner to be local player */
            net_lob.owner_connId = 0;
        }

        public void JoinLobby() {
            net_man.networkAddress = input_addr.text;
            Debug.Log(net_man.networkAddress);
            net_man.StartClient();
            SetPlayerLobby();
        }

        public void CreateLobby() {
            net_man.networkAddress = "localhost";
            Debug.Log(net_man.networkAddress);
            net_man.StartHost();
            SetPlayerLobby();
            /* muliplayer => allow to join */
            net_lob.allow_join = true;
            /* Host mode => local connId is 0 */
            net_lob.owner_connId = 0;
        }

        public void Quit() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        /** under the hood : button click -> TakeAvatar --mes--> (server PC) OnApplyAvatar
            @player_sync: tells which tab is clicked
         */
        public void TakeAvatar(AvatarTab player_sync) {
            // TODO: Judge double click to give up owning (owning that avatar and click that avatar again)



            int idx = this.avatars.FindIndex(ava => ava == player_sync);
            NetLobby.AvatarMessage mes = new NetLobby.AvatarMessage(
                this.ava_tags[idx], input_info.text);
            NetworkClient.Send<NetLobby.AvatarMessage>(mes);
        }

        public void InvReady() {
            if (!owning_ava)
                return;
            NetLobby.AvaStateMessage mes = new NetLobby.AvaStateMessage();
            mes.ready = !this.ava_ready;
            NetworkClient.Send<NetLobby.AvaStateMessage>(mes);
        }

        /// <summary> switch to another menu
        public void SetPlayerInfo() {
            DisableAllMenu();
            Menu_player_info.SetActive(true);
        }
        public void SetPlayerMode() {
            DisableAllMenu();
            Menu_player_mode.SetActive(true);
        }
        public void SetPlayerOpt() {
            DisableAllMenu();
            Menu_player_opt.SetActive(true);
        }
        public void SetPlayerLobby() {
            DisableAllMenu();
            Menu_player_lobby.SetActive(true);
            owning_ava = false;
            ava_ready = false;
            SetButtonReady();
        }
        /// <summary>
        /// (wrap-up) set all menus to be inactive (obviously, it's invisible, too)
        /// </summary>
        void DisableAllMenu() {
            Menu_player_mode.SetActive(false);
            Menu_player_info.SetActive(false);
            Menu_player_opt.SetActive(false);
            Menu_player_lobby.SetActive(false);
        }
        /// </summary>

        public void LeaveLobby() {
            if (NetworkServer.active && NetworkClient.active)
                net_man.StopHost();
            else if (NetworkClient.active)
                net_man.StopClient();
            else
                net_man.StopServer();
            SetPlayerOpt();
        }

        public void SetButtonReady() {
            if (this.owning_ava)
                btn_ready.SetActive(true);
            else
                btn_ready.SetActive(false);

            if (this.ava_ready) {
                TMP_Text btn_txt = btn_ready.GetComponentInChildren<TMP_Text>();
                btn_txt.text = "<color=#DCDD00>Cancel Ready</color>"; // yellow text
            } else {
                TMP_Text btn_txt = btn_ready.GetComponentInChildren<TMP_Text>();
                btn_txt.text = "<color=#28DD00>Ready</color>"; // green text
            }
        }
    }
}