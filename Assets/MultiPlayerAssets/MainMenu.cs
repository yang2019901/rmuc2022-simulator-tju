using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;

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
    [Header("robot names")]
    [Tooltip("set in Inspector and corresponding to avatar")]
    public List<string> tags;
    public List<AvatarTab> avatars;

    void Start() {
        /* set first menu to be player info menu */
        SetPlayerInfo();
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
        net_man.networkAddress = input_addr.text;
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
