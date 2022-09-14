using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RoboState : BasicState {
    public bool survival = true;  // whether this robot survives

    /** calculated by algorithm automatically
     *  they are initialized by RoboState.Configure() and updated by UpdateBuff() or BuffManager.cs
     */

    /* weapon params */
    public int heat_limit;
    public int cool_down;
    public int shoot_speed;
    /* chassis params */
    public int power;
    public int maxblood;
    public int currblood;


    /* Buff */
    /* damage = damage * (1 + hitter.B_atk) * max(1-hittee.B_dfc, 0) */
    public float B_atk;
    public float B_dfc;
    /* heat -= cool_down * B_cd */
    public float B_cd;
    public float B_pow;
    public float B_rev;
    public void UpdateBuff() {
        B_atk = Mathf.Max(li_B_atk.ToArray());
        B_dfc = Mathf.Max(li_B_dfc.ToArray());
        B_cd = Mathf.Max(li_B_cd.ToArray());
        B_rev = Mathf.Max(li_B_rev.ToArray());
    }

    float timer_rev = 0f;
    private void Revive() {
        currblood += Mathf.RoundToInt(maxblood * B_rev);
        currblood = currblood < maxblood ? currblood : maxblood;
        // Debug.Log("blood: " + currblood + "/" + maxblood);
    }

    public List<float> li_B_atk; // attack buff. Ex: rune_junior => B_atk.Add(0.5); rune_senior => B_atd.Add(1)
    public List<float> li_B_dfc; // defence buff. Ex: rune_senior => B_dfc.Add(0.5)
    public List<float> li_B_cd;
    public List<float> li_B_rev;


    /* for visual effects */
    public virtual void Start() {
        li_B_atk = new List<float> { 0 };
        li_B_dfc = new List<float> { 0 };
        li_B_cd  = new List<float> { 1 };
        li_B_rev = new List<float> { 0 };
        UpdateBuff();
        GetUserPref();
        Configure();
        this.acs = GetComponentsInChildren<ArmorController>();
        this.currblood = this.maxblood;
    }
    public virtual void Update() {
        if (Time.time - timer_rev >= 1f) {
            timer_rev = Time.time;
            Revive();
        }
    }

    ArmorController[] acs;
    public override void TakeDamage(GameObject hitter, GameObject armor_hit, GameObject bullet) {
        /* Requirement: make sure that small bullet's name contains "17mm" && big bullet's contains "42mm" */
        int damage = bullet.name.Contains("17mm") ? 10 : 100;
        damage = Mathf.RoundToInt(damage * (hitter.GetComponent<RoboState>().B_atk + 1)
            * Mathf.Max(1 - this.GetComponent<RoboState>().B_dfc, 0));
        currblood -= damage;

        Debug.Log("current blood: " + currblood);

        if (this.currblood <= 0) {
            this.currblood = 0;
            this.survival = false;
            foreach (ArmorController ac in acs)
                ac.Disable();
            BattleField.singleton.Kill(hitter, this.gameObject);
        } else
            StartCoroutine("ArmorsBlink", 0.1f);

        SetBloodBars();
    }

    private IEnumerator ArmorsBlink(float interval) {
        foreach (ArmorController ac in acs)
            ac.SetLight(false);
        yield return new WaitForSeconds(interval);
        foreach (ArmorController ac in acs)
            ac.SetLight(true);
    }

    public GameObject[] blood_bars;
    private void SetBloodBars() {
        Vector3 scale = new Vector3(1, 1, (float)currblood / maxblood);
        foreach (GameObject bb in blood_bars) {
            bb.transform.localScale = scale;
        }
    }

    public virtual RoboSync Pull() {
        RoboSync tmp = new RoboSync();
        tmp.currblood = this.currblood;
        tmp.maxblood = this.maxblood;
        if (!this.survival)
            tmp.ava_stat = RMUC_UI.AvaStat.Dead;
        else if (Mathf.Approximately(this.B_dfc, 1))
            tmp.ava_stat = RMUC_UI.AvaStat.Invulnerable;
        else if (!Mathf.Approximately(this.B_dfc, 0))
            tmp.ava_stat = RMUC_UI.AvaStat.Defensive;
        else
            tmp.ava_stat = RMUC_UI.AvaStat.Survival;
        return tmp;
    }

    public virtual void Push(RoboSync robo_sync) {
        this.survival = robo_sync.ava_stat != RMUC_UI.AvaStat.Dead;
        if (this.currblood > robo_sync.currblood) {
            this.currblood = robo_sync.currblood;
            this.SetBloodBars();
            if (!this.survival)
                foreach (ArmorController ac in acs)
                    ac.Disable();
            else
                StartCoroutine(this.ArmorsBlink(0.1f));
        }
        return;
    }

    public virtual void GetUserPref() {}
    public virtual void Configure() {}
}
