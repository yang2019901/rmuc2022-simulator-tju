using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

using RMUC_UI;

public class RoboState : BasicState {
    public bool survival = true;  // whether this robot survives
    [HideInInspector] public Rigidbody rigid;

    /** calculated by algorithm automatically
     *  they are initialized by RoboState.Configure() and updated by UpdateBuff() or BuffManager.cs
     */
    public int currexp = 0;
    public int maxexp;
    /* weapon params */
    public int maxheat;
    public int cooldown;
    public int bullspd;
    /* chassis params */
    public int power;
    public int maxblood;
    public int currblood;
    public List<Buff> robo_buff = new List<Buff>();

    /* Buff */
    /* damage = damage * (1 + hitter.B_atk) * max(1-hittee.B_dfc, 0) */
    public float B_atk;
    public float B_dfc;
    /* heat -= cooldown * B_cd */
    public float B_cd;
    public float B_pow;
    public float B_rev;
    public int B_rbn; // B_reborn: 2 if supply_spot; 1 if recover_card or self-reviving of engineer 

    public List<float> li_B_atk; // attack buff. Ex: rune_junior => B_atk.Add(0.5); rune_senior => B_atd.Add(1)
    public List<float> li_B_dfc; // defence buff. Ex: rune_senior => B_dfc.Add(0.5)
    public List<float> li_B_cd;
    public List<float> li_B_rev;
    public List<int> li_B_rbn;



    /// <summary>
    /// API
    /// </summary>
    public virtual RoboSync Pull() {
        RoboSync tmp = new RoboSync();
        tmp.currblood = this.currblood;
        tmp.maxblood = this.maxblood;
        if (!this.survival)
            tmp.bat_stat = BatStat.Dead;
        else if (Mathf.Approximately(this.B_dfc, 1))
            tmp.bat_stat = BatStat.Invulnerable;
        else if (!Mathf.Approximately(this.B_dfc, 0))
            tmp.bat_stat = BatStat.Defensive;
        else
            tmp.bat_stat = BatStat.Survival;
        return tmp;
    }


    public virtual void Push(RoboSync robo_sync) {
        if (!this.survival && robo_sync.bat_stat != BatStat.Dead) {
            Debug.Log(string.Format("{0} reborns", this.gameObject.name));
            foreach (ArmorController ac in acs)
                ac.Enable();
        }
        if (this.currblood != robo_sync.currblood || this.maxblood != robo_sync.maxblood) {
            this.currblood = robo_sync.currblood;
            this.maxblood = robo_sync.maxblood;
            SetBloodBars();
            if (robo_sync.bat_stat == BatStat.Dead)
                Die();
            // else
            //     StartCoroutine(this.ArmorsBlink(0.1f));
        }
        this.survival = robo_sync.bat_stat != BatStat.Dead;
        return;
    }


    public GameObject[] blood_bars;
    public void SetBloodBars() {
        if (maxblood <= 0)
            return;
        Vector3 scale = new Vector3(1, 1, (float)currblood / maxblood);
        foreach (GameObject bb in blood_bars) {
            bb.transform.localScale = scale;
        }
    }


    public virtual void Die() {
        Debug.Log(this.gameObject.name + " dies");
        this.currblood = 0;
        SetBloodBars();
        this.rbn_req += 10;

        this.survival = false;
        foreach (ArmorController ac in acs)
            ac.Disable();

        foreach (Buff tmp in this.robo_buff.ToArray()) {
            tmp.Disable();
        }
        UpdateBuff();

        DistribExp();
    }


    public void UpdateBuff() {
        B_atk = Mathf.Max(li_B_atk.ToArray());
        B_dfc = Mathf.Max(li_B_dfc.ToArray());
        B_cd = Mathf.Max(li_B_cd.ToArray());
        B_rev = Mathf.Max(li_B_rev.ToArray());
        B_rbn = Mathf.Max(li_B_rbn.ToArray());
    }



    /// <summary>
    /// non-API
    /// </summary>
    public virtual void Awake() {
        this.acs = GetComponentsInChildren<ArmorController>();
        this.rigid = GetComponentInChildren<Rigidbody>();
    }


    /* li_b_xx will be allocated here. hence, make sure base.Start() is called at very beginning */
    public virtual void Start() {
        li_B_atk = new List<float> { 0 };
        li_B_dfc = new List<float> { 0 };
        li_B_cd = new List<float> { 1 };
        li_B_rev = new List<float> { 0 };
        li_B_rbn = new List<int> { 0 };
        UpdateBuff();

        Configure();

        this.currblood = this.maxblood;
        SetBloodBars();
    }


    public virtual void Update() {
        if (!BattleField.singleton.started_game)
            return;

        if (NetworkServer.active)
            Revive();
    }


    protected int rbn_req = 0;  // Every death will add 10 to rbn_req immediately.
                                // When first died, rbn_req will be 10, 
                                //  exactly what's needed for first reborn
    float timer_rev = 0f;
    int rbn = 0;
    // revive per second
    void Revive() {
        if (this.survival) {
            if (B_rev != 0 && Time.time - timer_rev > 1 && currblood < maxblood) {
                currblood += Mathf.RoundToInt(maxblood * B_rev);
                currblood = currblood < maxblood ? currblood : maxblood;
                SetBloodBars();
                timer_rev = Time.time;
            }
        } else {
            if (Time.time - timer_rev >= 1) {
                rbn += B_rbn;
                timer_rev = Time.time;
            }
            if (rbn >= rbn_req)
                Reset();
        }
        // Debug.Log("blood: " + currblood + "/" + maxblood);
    }


    protected virtual void Reset() {
        rbn = 0;
        /* by rule, recover currblood to 20% */
        this.currblood = maxblood / 5;
        SetBloodBars();
        this.survival = true;
        /* set host's visual effect */
        Debug.Log(string.Format("{0} reborns", this.gameObject.name));
        foreach (ArmorController ac in acs)
            ac.Enable();
        SetBloodBars();
        StartCoroutine(this.Reborn());
    }


    IEnumerator Reborn() {
        /* delay 10 sec invincible */
        this.li_B_dfc.Add(1);
        UpdateBuff();
        yield return new WaitForSeconds(10);

        /* cancel invincible buff */
        this.li_B_dfc.Remove(1);
        UpdateBuff();
        yield break;
    }


    protected ArmorController[] acs;
    public override void TakeDamage(GameObject hitter, GameObject armor_hit, GameObject bullet) {
        // if (!NetworkServer.active)
        //     return;
        /* Requirement: make sure that small bullet's name contains "17mm" && big bullet's contains "42mm" */
        int damage = bullet.name.Contains("17mm") ? 10 : 100;
        damage = Mathf.RoundToInt(damage * (hitter.GetComponent<RoboState>().B_atk + 1)
            * Mathf.Max(1 - this.GetComponent<RoboState>().B_dfc, 0));
        currblood -= damage;
        /* if robot is invulnerable, there's no armorsblink or setbloodbar */
        if (damage == 0)
            return;
        /* else, robot record this hit and make visual effect */
        Hit(hitter);

        // Debug.Log("current blood: " + currblood);

        if (this.currblood <= 0) {
            // killed by ally doesnt count and ally will not get exp
            this.killed = hitter.GetComponent<BasicState>().armor_color != armor_color;
            this.killer = hitter;
            Die();
            BattleField.singleton.Kill(hitter, this.gameObject);
        } 
        else
           StartCoroutine(ArmorsBlink(0.1f));

        // SetBloodBars();
    }


    private IEnumerator ArmorsBlink(float interval) {
        foreach (ArmorController ac in acs)
            ac.SetLight(false);
        yield return new WaitForSeconds(interval);
        foreach (ArmorController ac in acs)
            /* set armors light according to robot's survival */
            ac.SetLight(this.survival);
    }



    public virtual void GetUserPref(string pref_chas, string pref_turr) { } // { Debug.Log("RoboState.GetUserPref (virtual)"); }
    public virtual void Configure() { } // { Debug.Log("RoboState.Configure (virtual)"); }
}
