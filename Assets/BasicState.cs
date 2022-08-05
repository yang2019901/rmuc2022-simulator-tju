using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicState : MonoBehaviour {
    public abstract void TakeDamage(GameObject hitter, GameObject armor_hit, GameObject bullet);
    public ArmorColor armor_color;
}


public abstract class RobotState : BasicState {
    public bool survival = true;  // whether this robot survives
    /* weapon params */
    public int heat_limit;
    public int cool_down;
    public int shoot_speed;
    /* chassis params */
    public int power;
    public int blood;
    public int blood_left;

    /* Buff */
    /* damage = damage * (1 + hitter.B_atk) * max(1-hittee.B_dfc, 0) */
    public float B_atk = 0;
    public float B_dfc = 0;
    /* heat -= cool_down * B_cd */
    public float B_cd = 0;
    public float B_pow = 0;
    public float B_rev = 0;
    public void UpdateBuff() {
        B_atk = Mathf.Max(li_B_atk.ToArray());
        B_dfc = Mathf.Max(li_B_dfc.ToArray());
        B_cd = Mathf.Max(li_B_cd.ToArray());
        B_rev = Mathf.Max(li_B_rev.ToArray());
    }
    public void Revive() {
        blood_left += Mathf.RoundToInt(blood * B_rev);
        blood_left = blood_left < blood ? blood_left : blood;
    }
    public List<float> li_B_atk = new List<float> { 0 }; // attack buff. Ex: rune_junior => B_atk.Add(0.5); rune_senior => B_atd.Add(1)
    public List<float> li_B_dfc = new List<float> { 0 }; // defence buff. Ex: rune_senior => B_dfc.Add(0.5)
    public List<float> li_B_cd = new List<float> { 0 };
    public List<float> li_B_rev = new List<float> { 0 };

    ArmorController[] acs;
    public override void TakeDamage(GameObject hitter, GameObject armor_hit, GameObject bullet) {
        /* Requirement: make sure that small bullet's name contains "17mm" && big bullet's contains "42mm" */
        int damage = bullet.name.Contains("17mm") ? 10 : 100;
        damage = Mathf.RoundToInt(damage * (hitter.GetComponent<RobotState>().B_atk + 1)
            * Mathf.Max(1 - this.GetComponent<RobotState>().B_dfc, 0));
        blood_left -= damage;

        Debug.Log("current blood: " + blood_left);

        if (blood_left <= 0) {
            blood_left = 0;
            foreach (ArmorController ac in acs)
                ac.Disable();
            BattleField.singleton.Kill(hitter, this.gameObject);
        } else
            StartCoroutine("ArmorsBlink", 0.1f);
    }

    public virtual void Start() {
        GetUserPref();
        Configure();
        this.acs = GetComponentsInChildren<ArmorController>();
        this.blood_left = this.blood;
    }

    private IEnumerator ArmorsBlink(float interval) {
        foreach (ArmorController ac in acs)
            ac.SetLight(false);
        yield return new WaitForSeconds(interval);
        foreach (ArmorController ac in acs)
            ac.SetLight(true);
    }

    public abstract void GetUserPref();
    public abstract void Configure();
}
