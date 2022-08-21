using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicState : MonoBehaviour {
    public abstract void TakeDamage(GameObject hitter, GameObject armor_hit, GameObject bullet);
    public void SetArmorColor(ArmorColor armor_color) {
        this.armor_color = armor_color;
        string other_color = armor_color == ArmorColor.Red ? "blue" : "red";
        Material new_mat = armor_color == ArmorColor.Red ? AssetManager.singleton.light_red 
            : AssetManager.singleton.light_blue;
        Renderer[] rens = GetComponentsInChildren<Renderer>();
        foreach (Renderer ren in rens) {
            if (ren.sharedMaterial.name.ToLower().Contains(other_color)) {
                ren.sharedMaterial = new_mat;
                Debug.Log("replace");
            }
        }
    }
    public ArmorColor armor_color;
}


public class RobotState : BasicState {
    public bool survival = true;  // whether this robot survives

    /** calculated by algorithm automatically
     *  they are initialized by RobotState.Configure() and updated by UpdateBuff() or BuffManager.cs
     */

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
        blood_left += Mathf.RoundToInt(blood * B_rev);
        blood_left = blood_left < blood ? blood_left : blood;
        // Debug.Log("blood: " + blood_left + "/" + blood);
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
        this.blood_left = this.blood;
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
        Vector3 scale = new Vector3(1, 1, (float)blood_left / blood);
        foreach (GameObject bb in blood_bars) {
            bb.transform.localScale = scale;
        }
    }

    public RobotSync Pull() {
        RobotSync tmp = new RobotSync();
        tmp.blood_left = this.blood_left;
        tmp.survival = this.survival;
        return tmp;
    }

    public void Push(RobotSync robot_sync) {
        this.blood_left = robot_sync.blood_left;
        this.SetBloodBars();
        this.survival = robot_sync.survival;
    }

    public virtual void GetUserPref() {}
    public virtual void Configure() {}
}
