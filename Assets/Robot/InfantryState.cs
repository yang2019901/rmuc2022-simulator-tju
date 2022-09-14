using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfantryState : RoboState {
    private string chassis_pref; // chassis preference: "power++", "maxblood++", "init_mode"
    private string weapon_pref; // weapon preference: "shoot_speed++" "heat_limit++" "cool_down++"
    private string level_s; // level info: "level1", "level2", "level3"
    private int level;
    private int bull_num;

    /* get user's preference of chassis and weapon from GUI */
    public override void GetUserPref() {
        // TODO
        this.chassis_pref = "power++";
        this.weapon_pref = "shoot_speed++";
        this.level_s = "level1";
    }

    public override void Configure() {
        /* configure chassis params */
        var tmp = AssetManager.singleton.infa_chs[this.chassis_pref];
        if (this.chassis_pref != "init_mode")
            tmp = tmp[this.level_s];
        this.maxblood = tmp["maxblood"].ToObject<int>();
        this.power = tmp["power"].ToObject<int>();
        /* configure weapon */
        tmp = AssetManager.singleton.weapon["17mm"][this.weapon_pref];
        if (this.weapon_pref != "init_mode")
            tmp = tmp[this.level_s];
        this.heat_limit = tmp["heat_limit"].ToObject<int>();
        this.cool_down = tmp["cool_down"].ToObject<int>();
        this.shoot_speed = tmp["shoot_speed"].ToObject<int>();
    }

    public override RoboSync Pull() {
        RoboSync rs = base.Pull();
        rs.level = this.level;
        rs.bull_num = this.bull_num;
        return rs;
    }

    public override void Push(RoboSync robo_sync) {
        base.Push(robo_sync);
        this.level = robo_sync.level;
        this.bull_num = robo_sync.bull_num;
    }
}
