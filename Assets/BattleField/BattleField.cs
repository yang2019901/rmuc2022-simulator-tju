using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BattleField : MonoBehaviour {
    public static BattleField singleton { get; private set; }

    /// <summary>
    /// Game Params
    /// </summary>
    public int money_red;
    public int money_red_max;
    public int money_blue;
    public int money_blue_max;
    public int score_red;
    public int score_blue;

    public const int length = 30;
    public const int width = 16;
    public const int height = 4;
    public bool had_first_blood;
    public bool started_game { get; private set; }
    public bool ended_game { get; private set; }

    /// <summary>
    /// External reference
    /// </summary>
    public RMUC_UI.BattleUI bat_ui;
    public OutpostState outpost_blue;
    public OutpostState outpost_red;
    public BaseState base_blue;
    public BaseState base_red;
    public GuardState guard_red;
    public GuardState guard_blue;
    public Rune rune;
    /* hero engineer infantry1 infantry2 */
    public RoboState[] robo_red;
    public RoboState[] robo_blue;
    [HideInInspector] public List<RoboState> robo_all = new List<RoboState>(); // automatically set
    [HideInInspector] public RoboState robo_local;                             // automatically set
    [HideInInspector] public List<BasicState> team_all = new List<BasicState>();

    public SyncNode sync_node;

    /* priority (with NetworkIdentity): Instantiate > Awake() > OnStartServer() (obviously, iff in server PC) 
        ----Spawn----> OnStartClient() (obviously, iff in client PC) > Start()    
    */
    void Awake() {
        if (singleton == null) {
            singleton = this;
        } else
            Destroy(this.gameObject);
        robo_all.AddRange(robo_red);
        robo_all.AddRange(robo_blue);

        team_all.AddRange(robo_all);
        team_all.Add(outpost_red);
        team_all.Add(outpost_blue);
    }


    void Start() {
        StartCoroutine(StartGame());
    }


    void Update() {
        t_bat += Time.deltaTime;
        if (this.GetBattleTime() > 1f || !base_blue.survival || !base_red.survival)
            this.EndGame();
    }


    // reset game params, such as money, started_game, etc; score won't be reset
    void ResetParam() {
        this.money_red = 0;
        this.money_red_max = 0;
        this.money_blue = 0;
        this.money_blue_max = 0;
        this.had_first_blood = false;
        this.started_game = false;
        this.ended_game = false;
    }


    IEnumerator StartGame() {
        // a short time for both team to prepare for the game. (change position, look around, etc.)
        ResetParam();
        t_bat = -GameSetting.singleton.prepare_sec - 6;

        yield return new WaitForSeconds(GameSetting.singleton.prepare_sec);

        AssetManager.singleton.StopClip(AssetManager.singleton.prepare);
        AssetManager.singleton.PlayClipAround(AssetManager.singleton.cntdown);

        yield return new WaitForSeconds(6);

        rune.Init();
        if (robo_local != null) {
            var rc = robo_local.GetComponent<RoboController>();
            if (rc != null) {
                /* when game starts, dropdowns become non-interactable and captions of dropdowns in pref_ui submit */
                bat_ui.SetRoboPrefDrop(false);
                rc.CmdUserPref(rc.gameObject.name, bat_ui.drop_chas.captionText.text, bat_ui.drop_turr.captionText.text);
            }
        }
        AssetManager.singleton.PlayClipAround(AssetManager.singleton.gamebg, true, 0.3f);
        AllAddMoney(200);
        StartCoroutine(DistribMoney());
        this.started_game = true;
    }


    [Header("draw-redwin-bluewin")]
    [SerializeField] Animator[] anims_win;
    public void EndGame() {
        if (this.ended_game)
            return;

        int rlt = 0; // 0: draw; 1: red win; 2: blue win
        int[] blood_diff = new int[3] {outpost_red.currblood - outpost_blue.currblood,
            guard_red.currblood - guard_blue.currblood, // TODO: add guard state and put guard blood difference here
            base_red.currblood - base_blue.currblood};
        for (int i = 0; i < blood_diff.Length; i++) {
            if (blood_diff[i] != 0) {
                rlt = blood_diff[i] > 0 ? 1 : 2;
                break;
            }
        }

        if (anims_win != null && rlt < anims_win.Length) {
            anims_win[rlt].gameObject.SetActive(true);
        }

        net_man.StartCoroutine(EndGameTrans());
        this.ended_game = true;
    }


    BattleNetworkManager net_man => BattleNetworkManager.singleton;
    IEnumerator EndGameTrans() {
        // wait for a while such that `anims_win` ends playing
        yield return new WaitForSeconds(1);
        SceneTransit.singleton.StartTransit();
        // wait for a while such that SceneTransit.singleton ends playing
        yield return new WaitForSeconds(1);
        // server PC change scene to `scn_lobby`
        net_man.ServerChangeScene(net_man.scn_lobby);
    }


    float t_bat = 0;
    public float GetBattleTime() => t_bat;


    public RoboState GetRobot(string robot_s) {
        RoboState tmp;
#if UNITY_EDITOR
        Debug.Log("GetRobot() in BattleField: in unity editor");
        tmp = GameObject.Find(robot_s).GetComponent<RoboState>();
#elif UNITY_STANDALONE
            Debug.Log("GetRobot() in BattleField: in standalone");
            tmp = this.robo_all.Find(i => i.name == robot_s);
#endif
        return tmp;
    }


    public bool OnField(GameObject obj) {
        const int x_half_length = 16;
        const int y_half_length = 10;
        const int z_half_length = 10;
        Vector3 rel_pos = obj.transform.position - this.transform.position;
        return Mathf.Abs(rel_pos.x) < x_half_length && Mathf.Abs(rel_pos.y) < y_half_length
            && Mathf.Abs(rel_pos.z) < z_half_length;
    }


    Dictionary<RoboState, int> killnum = new Dictionary<RoboState, int>();
    public void Kill(GameObject hitter, GameObject hittee) {
        Debug.Log(hitter.name + " slays " + hittee.name);
        if (NetworkServer.active)
            sync_node.RpcKill(hitter.name, hittee.name);
        if (!NetworkClient.active)
            return;

        /* a team's base is lost and game ends */
        if (hittee.GetComponent<BaseState>() != null)
            EndGame();

        BaseState bs;
        GuardState gs;
        /* a team's outpost is lost and its base becomes vulnerable */
        if (hittee.GetComponent<OutpostState>() != null) {
            bs = hittee == outpost_blue.gameObject ? base_blue : base_red;
            gs = hittee == outpost_blue.gameObject ? guard_blue : guard_red;
            bs.GetComponent<Base>().OpenShells(true);
            bs.invul = false;
            bs.SetInvulLight(false);
            bs.shield = 500;
            gs.invul = false;
            gs.SetInvulLight(false);
        }

        gs = hittee.GetComponent<GuardState>();
        if (gs != null) {
            bs = gs.armor_color == ArmorColor.Red ? base_red : base_blue;
            bs.shield = 0;
        }

        RoboState rs1 = hitter.GetComponent<RoboState>();
        BasicState rs2;
        AudioClip ac = null;
        if (hittee.TryGetComponent<BasicState>(out rs2)) {
            // teammate's killed
            if (rs2.armor_color == robo_local.armor_color) {
                if (rs2 == robo_local) {
                    ac = AssetManager.singleton.self_die;
                    AssetManager.singleton.PlayClipAtPoint(
                        AssetManager.singleton.robo_die, robo_local.transform.position);
                } else
                    ac = AssetManager.singleton.ally_die;
            }
            // enemy's killed but not by teammate
            else if (rs1.armor_color != robo_local.armor_color)
                ac = AssetManager.singleton.kill[0];
            // enemy's killed by teammate
            else {
                if (!killnum.ContainsKey(rs1))
                    killnum.Add(rs1, -1);
                int cnt = killnum[rs1];
                cnt = (cnt + 1) % AssetManager.singleton.kill.Length;
                killnum[rs1] = cnt;
                ac = AssetManager.singleton.kill[cnt];
            }
        } else
            Debug.Log("cannot get basicstate from hitter");
        AssetManager.singleton.PlayClipAround(ac);
        bat_ui.brdcst.EnqueueKill(hitter, hittee);
    }


    public IEnumerator ActivateRune(ArmorColor armor_color, RuneBuff rune_buff) {
        if (NetworkServer.active) {
            sync_node.RpcActivateRune(armor_color, rune_buff);
        }
        AssetManager.singleton.PlayClipAround(AssetManager.singleton.rune_activ);
        if (rune_buff == RuneBuff.None)
            Debug.LogError("Error: activate RuneBuff.None");
        rune.activ = Activation.Activated;
        rune.rune_color = armor_color;
        AddRuneBuff(armor_color, rune_buff);
        yield return new WaitForSeconds(45);

        RmRuneBuff(armor_color, rune_buff);
        rune.Reset();
        /* reset rune.activ and motion params */
        rune.disabled = true;
        yield return new WaitForSeconds(30);
        rune.disabled = false;
    }


    public void XchgMine(ArmorColor armor_color, bool is_gold) {
        Debug.Log("team " + armor_color + " xchg mine");
        int d_mon = is_gold ? 300 : 100;
        if (armor_color == ArmorColor.Red) {
            money_red_max += d_mon;
            money_red += d_mon;
        } else {
            money_blue_max += d_mon;
            money_blue += d_mon;
        }

        bat_ui.notepad.DispXchgMine(armor_color, is_gold);
    }


    public BatSync Pull() {
        BatSync tmp = new BatSync();
        tmp.time_bat = GetBattleTime();
        tmp.money_red = this.money_red;
        tmp.money_red_max = this.money_red_max;
        tmp.money_blue = this.money_blue;
        tmp.money_blue_max = this.money_blue_max;
        tmp.score_red = this.score_red;
        tmp.score_blue = this.score_blue;
        return tmp;
    }


    public void Push(BatSync tmp) {
        this.t_bat = tmp.time_bat;
        this.money_red = tmp.money_red;
        this.money_red_max = tmp.money_red_max;
        this.money_blue = tmp.money_blue;
        this.money_blue_max = tmp.money_blue_max;
        this.score_red = tmp.score_red;
        this.score_blue = tmp.score_blue;
    }


    /// <summary>
    /// non-API 
    /// </summary>
    void RmRuneBuff(ArmorColor armor_color, RuneBuff rune_buff) {
        float atk_up = rune_buff == RuneBuff.Junior ? 0.5f : 1f;
        float dfc_up = rune_buff == RuneBuff.Junior ? 0 : 0.5f;
        RoboState[] targets = armor_color == ArmorColor.Red ? robo_red : robo_blue;
        foreach (RoboState robot in targets) {
            robot.li_B_atk.Remove(atk_up);
            robot.li_B_dfc.Remove(dfc_up);
            robot.UpdateBuff();
        }
    }


    void AddRuneBuff(ArmorColor armor_color, RuneBuff rune_buff) {
        AssetManager.singleton.PlayClipAround(AssetManager.singleton.rune_activ);
        float atk_up = rune_buff == RuneBuff.Junior ? 0.5f : 1f;
        float dfc_up = rune_buff == RuneBuff.Junior ? 0 : 0.5f;
        RoboState[] targets = armor_color == ArmorColor.Red ? robo_red : robo_blue;
        foreach (RoboState robot in targets) {
            robot.li_B_atk.Add(atk_up);
            robot.li_B_dfc.Add(dfc_up);
            robot.UpdateBuff();
        }
    }


    IEnumerator DistribMoney() {
        for (int i = 0; i < 5; i++) {
            yield return new WaitForSeconds(60);
            AllAddMoney(100);
        }
        yield return new WaitForSeconds(60);
        AllAddMoney(200);
        yield break;
    }


    void AllAddMoney(int number) {
        money_red_max += number;
        money_red += number;
        money_blue_max += number;
        money_blue += number;
    }
}
