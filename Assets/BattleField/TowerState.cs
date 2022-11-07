using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerState : BasicState {
    /// <summary>
    /// External Reference
    /// </summary>
    public GameObject[] blood_bars;
    protected ArmorController[] acs;

    /// <summary>
    /// Game Params
    /// </summary>
    public int maxblood;
    public bool survival = true;
    /* for sync */
    public int currblood;


    /// <summary>
    /// API
    /// </summary>
    protected IEnumerator ArmorsBlink(float interval) {
        foreach (ArmorController ac in acs)
            ac.SetLight(false);
        yield return new WaitForSeconds(interval);
        foreach (ArmorController ac in acs)
            ac.SetLight(true);
    }


    protected void SetBloodBars() {
        Vector3 scale = new Vector3(1, 1, (float)currblood / maxblood);
        foreach (GameObject bb in blood_bars) {
            bb.transform.localScale = scale;
        }
    }



    /// <summary>
    /// non-API
    /// </summary>
    public virtual void Start() {
        currblood = maxblood;
        acs = GetComponentsInChildren<ArmorController>();
    }


    public override void TakeDamage(GameObject hitter, GameObject armor_hit, GameObject bullet) {
        /* Requirement: make sure that small bullet's name contains "17mm" && big bullet's contains "42mm" */
        int damage;
        if (armor_hit.name.ToLower().Contains("triangle")) {
            damage = bullet.name.Contains("17mm") ? 0 : 300;
            Debug.LogWarning("hit triangle armor");
        }
        else
            damage = bullet.name.Contains("17mm") ? 5 : 200;
        damage = Mathf.RoundToInt(damage * (hitter.GetComponent<RoboState>().B_atk + 1));
        currblood -= damage;

        Debug.Log("current blood: " + currblood);

        if (this.currblood <= 0) {
            // killed by ally doesnt count and ally will not get exp
            this.killed = hitter.GetComponent<BasicState>().armor_color != armor_color;
            this.killer = hitter;
            Die();
            BattleField.singleton.Kill(hitter, this.gameObject);
        } else
            StartCoroutine(this.ArmorsBlink(0.1f));

        SetBloodBars();
    }


    void Die() {
        this.currblood = 0;
        this.SetBloodBars();

        this.survival = false;
        foreach (ArmorController ac in acs)
            ac.Disable();
        
        DistribExp();
    }
}
