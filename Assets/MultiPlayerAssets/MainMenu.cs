using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

/* handle all UI-related events and call corresponding functions in other scripts */
public class MainMenu : MonoBehaviour {
    public BattleNetworkManager net_man;

    public GameObject Menu_player_mode;
    public GameObject Menu_player_info;
    [Header("option")]
    public GameObject Menu_player_opt;
    public InputField input_addr;
    [Header("lobby")]
    public GameObject Menu_player_lobby;
    public Button[] buttons;

    void Start() {
        /* set first menu to be player info menu */
        SetPlayerInfo();
    }


    /* start host -> go to lobby -> lobby behaviour -> start game */
    public void SinglePlay() {
        net_man.StartHost();
        SetPlayerLobby();
    }

    public void JoinLobby() {
        net_man.networkAddress = input_addr.text;
        Debug.Log(net_man.networkAddress);
        net_man.StartClient();
        SetPlayerLobby();
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
}
