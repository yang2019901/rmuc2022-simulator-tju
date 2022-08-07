using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroState : RobotState {
    public bool sniping = false;
    private string chassis_pref; // chassis preference: "power++", "blood++", "init_mode"
    private string weapon_pref; // weapon preference: "shoot_speed++" "heat_limit++"
    private string level; // level info: "level1", "level2", "level3"

    /* get user's preference of chassis and weapon from GUI */
    public override void GetUserPref() {
        // TODO
        this.chassis_pref = "power++";
        this.weapon_pref = "shoot_speed++";
        this.level = "level1";
    }

    public override void Configure() {
        /* configure chassis params */
        var tmp = AssetManager.singleton.hero_chs[this.chassis_pref];
        if (this.chassis_pref != "init_mode")
            tmp = tmp[this.level];
        this.blood = tmp["blood"].ToObject<int>();
        this.power = tmp["power"].ToObject<int>();
        /* configure weapon */
        tmp = AssetManager.singleton.weapon["42mm"][this.weapon_pref];
        if (this.weapon_pref != "init_mode")
            tmp = tmp[this.level];
        this.heat_limit = tmp["heat_limit"].ToObject<int>();
        this.cool_down = tmp["cool_down"].ToObject<int>();
        this.shoot_speed = tmp["shoot_speed"].ToObject<int>();
    }
}
