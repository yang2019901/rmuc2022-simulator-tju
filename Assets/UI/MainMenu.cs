using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject UI_player_info;
    public GameObject UI_player_mode;

    public void SetPlayerInfo() {
        UI_player_info.SetActive(true);        
        UI_player_mode.SetActive(false);
    }

    public void SetPlayerMode() {
        UI_player_info.SetActive(false);
        UI_player_mode.SetActive(true);
    }
}
