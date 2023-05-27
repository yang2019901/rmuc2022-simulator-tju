using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.UI;
using TMPro;

namespace LobbyUI {
    public class RoboTab : MonoBehaviour {
        [SerializeField] private GameObject img_ready;
        [SerializeField] private GameObject img_index;
        [SerializeField] private TMP_Text text_owner;

        private const string Golden = "#ffdf00";
        public void SetReady(bool ready) {
            img_ready.SetActive(ready);
            img_index.SetActive(!ready);
        }

        public void SetOwner(string owner) {
            text_owner.text = owner;
        }

        public void SetRoboTab(PlayerSync player_state) {
            if (!player_state.owning_ava)
                Debug.LogError("Damn! RoboTag receive player_state that owns no robot. Program goes wrong");
            /* local player takes that avatar */
            if (player_state.connId == NetLobby.uid) {
                /* 0 in {0} is necessary and indicates to insert params[0] here */
                player_state.player_name = string.Format("<color={0}>{1}</color>", Golden, player_state.player_name);
            }
            SetOwner(player_state.player_name);
            SetReady(player_state.ready);
        }

        public void RstRoboTab() {
            this.SetReady(false);
            SetOwner("");
        }
    }
}
