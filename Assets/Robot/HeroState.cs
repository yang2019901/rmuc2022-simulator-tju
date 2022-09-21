using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroState : RoboState {
    public bool sniping = false;
    private string chassis_pref; // chassis preference: "power++", "maxblood++", "init_mode"
    private string weapon_pref; // weapon preference: "bullspd++" "maxheat++"
    private string level_s; // level info: "level1", "level2", "level3"
    private int level;
    private int bull_num;

    /* get user's preference of chassis and weapon from GUI */
    public override void GetUserPref() {
        // TODO
        this.chassis_pref = "power++";
        this.weapon_pref = "bullspd++";
        this.level_s = "level1";
    }

    public override void Configure() {
        /* configure chassis params */
        var tmp = AssetManager.singleton.hero_chs[this.chassis_pref];
        if (this.chassis_pref != "init_mode")
            tmp = tmp[this.level_s];
        this.maxblood = tmp["maxblood"].ToObject<int>();
        this.power = tmp["power"].ToObject<int>();
        /* configure weapon */
        tmp = AssetManager.singleton.weapon["42mm"][this.weapon_pref];
        if (this.weapon_pref != "init_mode")
            tmp = tmp[this.level_s];
        this.maxheat = tmp["maxheat"].ToObject<int>();
        this.cooldown = tmp["cooldown"].ToObject<int>();
        this.bullspd = tmp["bullspd"].ToObject<int>();
    }

    public override RoboSync Pull() {
        RoboSync rs = base.Pull();
        rs.has_blood = true;
        rs.has_bull = true;
        rs.has_level = true;
        rs.level = this.level;
        rs.bull_num = this.bull_num;
        return rs;
    }

    public override void Push(RoboSync robo_sync) {
        base.Push(robo_sync);
        this.level = robo_sync.level;
        this.bull_num = robo_sync.bull_num;
    }

    public override void Update() {
        base.Update();
    }
}
