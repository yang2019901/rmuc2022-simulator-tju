using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfantryState : RoboState {
    private string chassis_pref; // chassis preference: "power++", "maxblood++", "init_mode"
    private string weapon_pref; // weapon preference: "bullspd++" "maxheat++" "cooldown++"
    private string level_s; // level info: "level1", "level2", "level3"
    private int level;
    Weapon wpn;

    public override void Start() {
        base.Start();
        wpn = GetComponent<Weapon>();
        wpn.Reset();
    }


    /* get user's preference of chassis and weapon from GUI */
    public override void GetUserPref() {
        // TODO
        this.chassis_pref = "power++";
        this.weapon_pref = "bullspd++";
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
        this.maxheat = tmp["maxheat"].ToObject<int>();
        this.cooldown = tmp["cooldown"].ToObject<int>();
        this.bullspd = tmp["bullspd"].ToObject<int>();
    }

    public override RoboSync Pull() {
        RoboSync rs = base.Pull();
        rs.has_blood = true;
        rs.has_wpn = true;
        rs.has_level = true;
        rs.level = this.level;
        rs.bull_num = wpn.bull_num;
        return rs;
    }
}
