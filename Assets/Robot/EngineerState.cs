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
    string rev_card_s;



    /// <summary>
    /// API
    /// </summary>
    public override RoboSync Pull() {
        RoboSync rs = base.Pull();
        // Debug.Log("actual survival: " + this.survival + " pull survival: " + rs.bat_stat);
        rs.has_blood = true;
        rs.has_wpn = false;
        rs.has_level = false;
        return rs;
    }       // called by syncnode


    public override void Die() {
        base.Die();
        ec.rev_card.name = "revive_card";
    }       // called by weapon


    public override void TakeDamage(GameObject hitter, GameObject armor_hit, GameObject bullet) {
        base.TakeDamage(hitter, armor_hit, bullet);
        if (reviving) {
            reviving = false;
            li_B_rev.Remove(0.02f);
            UpdateBuff();
        }
        timer_hit = 0; 
    }       // called by armorcontroller


    /// <summary>
    /// non-API
    /// </summary>
    public override void Awake() {
        base.Awake();
        ec = GetComponent<EngineerController>();
    }


    public override void Start() {
        base.Start();

        li_B_rbn.Add(1);
        UpdateBuff();

        rev_card_s = BuffType.rev + BuffManager.sep + (armor_color == ArmorColor.Red ? "red" : "blue") + " card";
        ec.rev_card.name = rev_card_s;
        
        rbn_req = 10;
    }


    bool reviving = true;
    float timer_hit = interv_self_rev;     // how long since last hit
    public override void Update() {
        base.Update();
        if (timer_hit > interv_self_rev && !reviving) {
            reviving = true;
            li_B_rev.Add(0.02f);
            UpdateBuff();
        }
    }


    protected override void Reset() {
        base.Reset();
        ec.rev_card.name = rev_card_s;
    }


    public override void Configure() {
        base.Configure();
        /* Make sure init maxblood first because in Start(), maxblood is assigned to currblood */
        this.maxblood = 500;
        this.expval = AssetManager.singleton.exp["engineer"]["have"].ToObject<int>(); 
        return ;
    }
}
