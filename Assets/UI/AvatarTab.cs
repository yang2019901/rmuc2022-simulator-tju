using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RMUC_UI {
    public class AvatarTab : MonoBehaviour {
        [SerializeField] private GameObject img_ready;
        [SerializeField] private GameObject img_index;
        [SerializeField] private Text text_owner;

        private const string Golden = "#ffdf00";
        public void SetReady(bool ready) {
            img_ready.SetActive(ready);
            img_index.SetActive(!ready);
        }

        public void SetOwner(string owner) {
            text_owner.text = owner;
        }

        public void SetAvatarTab(PlayerSync player_state) {
            if (!player_state.owning_robot)
                Debug.LogError("Damn! RoboTag receive player_state that owns no robot. Program goes wrong");
            /* local player takes that avatar */
            if (player_state.connId == Mirror.NetworkClient.connection.connectionId) {
                /* 0 in {0} is necessary and indicates to insert params[0] here */
                player_state.player_name = string.Format("<color={0}>{1}</color>", Golden, player_state.player_name);
            }
            SetOwner(player_state.player_name);
            SetReady(player_state.ready);
        }

        public void ResetAvatarTab() {
            this.SetReady(false);
            SetOwner("");
        }
    }
}
