using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarTab : MonoBehaviour {
    [SerializeField] private GameObject img_ready;
    [SerializeField] private GameObject img_index;
    [SerializeField] private Text text_owner;

    public void SetReady(bool ready) {
        img_ready.SetActive(ready);
        img_index.SetActive(ready);
    }

    public void SetOwner(string owner) {
        text_owner.text = owner;
    }

    public void SetAvatar(PlayerStateSync player_state) {
        if (!player_state.owning_robot)
            Debug.LogError("Damn! RoboTag receive player_state that owns no robot. Program goes wrong");
        SetOwner(player_state.player_name);
        SetReady(player_state.ready);
    }
}
