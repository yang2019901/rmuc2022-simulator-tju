using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* engineer:
    1. no limit of chassis power
    2. no level, maxblood is 500
 */
public class EngineerState : RoboState {
    const int interv_self_rev = 30;     // how long engineer starts to self-revive since last hit
    EngineerController ec;



    /// <summary>
    /// API
    /// </summary>
    public override RoboSync Pull() {
        RoboSync rs = base.Pull();
        // Debug.Log("actual survival: " + this.survival + " pull survival: " + rs.bat_stat);
        rs.has_blood = true;
        rs.has_wpn = false;
        rs.has_level = false;
        rs.maxblood = this.maxblood;
        return rs;
    }


    /// <summary>
    /// non-API
    /// </summary>
    public override void Start() {
        base.Start();
        ec = GetComponent<EngineerController>();
        ec.rev_card.name = BuffType.rev + BuffManager.sep + (armor_color == ArmorColor.Red ? "red" : "blue");

        li_B_rbn.Add(1);
        UpdateBuff();

        tmp1 = interv_self_rev;
        rbn_req = 10;
    }


    float tmp2;
    public override void Update() {
        base.Update();
        tmp2 = tmp1 + Time.deltaTime;
        if (tmp1 < interv_self_rev && tmp2 >= interv_self_rev) {
            li_B_rev.Add(0.02f);
            UpdateBuff();
        }
        tmp1 = tmp2;
    }


    float tmp1 = 0;     // how long since last hit
    public override void TakeDamage(GameObject hitter, GameObject armor_hit, GameObject bullet)
    {
        base.TakeDamage(hitter, armor_hit, bullet);
        if (tmp1 >= interv_self_rev) {
            li_B_rev.Remove(0.02f);
            UpdateBuff();
        }
        tmp1 = 0; 
    }


    public override void Die()
    {
        base.Die();
        ec.rev_card.name = "revive_card";
    }


    protected override void Revive() {
        base.Revive();
        ec.rev_card.name = BuffType.rev + BuffManager.sep + (armor_color == ArmorColor.Red ? "red" : "blue");
    }


    public override void Configure() {
        base.Configure();
        /* Make sure init maxblood first because in Start(), maxblood is assigned to currblood */
        this.maxblood = 500;
        this.expval = AssetManager.singleton.exp["engineer"]["have"].ToObject<int>(); 
        return ;
    }
}
