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
    public int money_blue;
    public int score_red;
    public int score_blue;

    public bool had_first_blood = false;

    /// <summary>
    /// External reference
    /// </summary>
    public RMUC_UI.BattleUI bat_ui;
    public OutpostState outpost_blue;
    public OutpostState outpost_red;
    public BaseState base_blue;
    public BaseState base_red;
    public Rune rune;
    /* hero engineer infantry1 infantry2 */
    public RoboState[] robo_red;
    public RoboState[] robo_blue;
    public List<RoboState> robo_all = new List<RoboState>();
    public RoboState robo_local;

    /* priority (with NetworkIdentity): Instantiate > Awake() > OnStartServer() (obviously, iff in server PC) 
        ----Spawn----> OnStartClient() (obviously, iff in client PC) > Start()    
    */
    void Awake() {
        if (singleton == null) {
            singleton = this;
        } else
            Destroy(this);
        robo_all.AddRange(robo_red);
        robo_all.AddRange(robo_blue);
    }


    void Start() {
        t_start = Time.time;

        rune.Init();
        Debug.Log("rune init");

        AssetManager.singleton.StopClip(AssetManager.singleton.prepare);
        AssetManager.singleton.PlayClipAround(AssetManager.singleton.gamebg, true, 0.3f);
    }


    int x_half_length = 16;
    int y_half_length = 10;
    int z_half_length = 10;
    public bool OnField(GameObject obj) {
        Vector3 rel_pos = obj.transform.position - this.transform.position;
        return Mathf.Abs(rel_pos.x) < x_half_length && Mathf.Abs(rel_pos.y) < y_half_length
            && Mathf.Abs(rel_pos.z) < z_half_length;
    }

    Dictionary<RoboState, int> killnum = new Dictionary<RoboState, int>();
    public void Kill(GameObject hitter, GameObject hittee) {
        Debug.Log(hitter.name + " slays " + hittee.name);
        if (hittee == outpost_blue.gameObject) {
            base_blue.GetComponent<Base>().OpenShells(true);
            base_blue.invul = false;
            base_blue.SetInvulLight(false);
            // base_blue.shield = 500;
        } else if (hittee == outpost_red.gameObject) {
            base_red.GetComponent<Base>().OpenShells(true);
            base_red.invul = false;
            base_blue.SetInvulLight(false);
            // base_blue.shield = 500;
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
        }
        else
            Debug.Log("cannot get basicstate from hitter");
        AssetManager.singleton.PlayClipAround(ac);
    }


    void AddRuneBuff(ArmorColor armor_color, RuneBuff rune_buff) {
        AssetManager.singleton.PlayClipAround(AssetManager.singleton.rune_activ);
        float atk_up = rune_buff == RuneBuff.Junior ? 0.5f : 1f;
        if (armor_color == ArmorColor.Red) {
            Debug.Log("Team Red adds rune buff");
            foreach (RoboState robot in robo_red) {
                robot.li_B_atk.Add(atk_up);
                robot.UpdateBuff();
            }
        } else {
            Debug.Log("Team Blue adds rune buff");
            foreach (RoboState robot in robo_blue) {
                robot.li_B_atk.Add(atk_up);
                robot.UpdateBuff();
            }
        }
    }
    void RemoveRuneBuff(ArmorColor armor_color, RuneBuff rune_buff) {
        float atk_up = rune_buff == RuneBuff.Junior ? 0.5f : 1f;
        if (armor_color == ArmorColor.Red) {
            Debug.Log("Team Red removes rune buff");
            foreach (RoboState robot in robo_red) {
                robot.li_B_atk.Remove(atk_up);
                robot.UpdateBuff();
            }
        } else {
            Debug.Log("Team Blue removes rune buff");
            foreach (RoboState robot in robo_blue) {
                robot.li_B_atk.Remove(atk_up);
                robot.UpdateBuff();
            }
        }
    }
    public IEnumerator ActivateRune(ArmorColor armor_color, RuneBuff rune_buff) {
        AssetManager.singleton.PlayClipAround(AssetManager.singleton.rune_activ);
        if (rune_buff == RuneBuff.None)
            Debug.LogError("Error: activate RuneBuff.None");
        AddRuneBuff(armor_color, rune_buff);
        yield return new WaitForSeconds(45);

        rune.rune_state_blue.SetActiveState(Activation.Idle);
        rune.rune_state_red.SetActiveState(Activation.Idle);
        RemoveRuneBuff(armor_color, rune_buff);
        /* right now, rune.activated is true => no spinning, no light */
        yield return new WaitForSeconds(30);

        /* reset rune.activated and motion params */
        rune.Reset();
    }


    float t_start;
    public float GetBattleTime() {
        return Time.time - t_start;
    }


    BatSync tmp = new BatSync();
    public BatSync Pull() {
        tmp.time_bat = GetBattleTime();
        tmp.money_red = this.money_red;
        tmp.money_blue = this.money_blue;
        tmp.score_red = this.score_red;
        tmp.score_blue = this.score_blue;
        return tmp;
    }
}
