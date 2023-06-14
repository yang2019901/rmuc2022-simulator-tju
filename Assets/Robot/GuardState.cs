using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardState : RoboState {
    Weapon wpn;
    public bool invul;


    public override void Awake() {
        base.Awake();
        wpn = GetComponent<Weapon>();
    }


    public override void Start() {
        base.Start();
        wpn.bullnum = 500;
        this.invul = true;
        SetInvulLight(true);
    }


    public override RoboSync Pull() {
        RoboSync rs = base.Pull();
        if (this.invul)
            rs.bat_stat = RMUC_UI.BatStat.Invulnerable;
        rs.has_blood = true;
        rs.has_level = false;
        rs.has_wpn = true;
        rs.bull_num = wpn.bullnum;
        rs.heat_ratio = wpn.heat_ratio;
        return rs;
    }


    public override void Push(RoboSync robo_sync) {
        base.Push(robo_sync);
        wpn.bullnum = robo_sync.bull_num;
        wpn.heat_ratio = robo_sync.heat_ratio;
        return;
    }


    public override void Configure() {
        this.maxblood = 600;
        this.maxheat = 320;
        this.cooldown = 100;
        this.bullspd = 30;
        this.expval = AssetManager.singleton.exp["guard"]["have"].ToObject<int>();
    }


    public void SetInvulLight(bool on) {
        if (on)
            foreach (ArmorController ac in this.acs)
                ac.SetLight(AssetManager.singleton.light_purple);
        else
            foreach (ArmorController ac in this.acs)
                ac.SetLight(true);
    }

    public override void TakeDamage(GameObject hitter, GameObject armor_hit, GameObject bullet) {
        if (!this.invul)
            base.TakeDamage(hitter, armor_hit, bullet);
    }
}
