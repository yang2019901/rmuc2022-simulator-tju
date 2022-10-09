using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroState : RoboState {
    /// <summary>
    /// Key state variable
    /// </summary>
    public bool sniping = false;
    private string chassis_pref; // chassis preference: "power++", "maxblood++", "init_mode"
    private string weapon_pref; // weapon preference: "bullspd++" "maxheat++"
    public int level = 0;
    
    /// <summary>
    /// Game Params
    /// </summary>
    public int maxexp;
    int expgrow = 4;

    /// <summary>
    /// External reference
    /// </summary>
    Weapon wpn;


    /// <summary>
    /// API
    /// </summary>
    public override RoboSync Pull() {
        RoboSync rs = base.Pull();
        rs.has_blood = true;
        rs.has_wpn = true;
        rs.has_level = true;
        rs.level = this.level;
        rs.bull_num = wpn.bull_num;
        rs.heat_ratio = wpn.heat_ratio;
        return rs;
    }


    public override void Push(RoboSync robo_sync) {
        base.Push(robo_sync);
        /* for visual effects */
        wpn.bull_num = robo_sync.bull_num;
        wpn.heat_ratio = robo_sync.heat_ratio;
    }




    /// <summary>
    /// non-API
    /// </summary>
    public override void Start() {
        base.Start();
        wpn = GetComponent<Weapon>();
        wpn.ResetHeat();
    }


    public override void Update() {
        base.Update();
        GetExp();
        LevelUp();
    }


    float exptimer = 0;
    void GetExp() {
        if (this.survival && Time.time - exptimer > 12) {
            this.currexp += expgrow;
            exptimer = Time.time;
        }
    }


    void LevelUp() {
        while (this.currexp >= this.maxexp) {
            this.currexp -= this.maxexp;
            this.level++;
            Configure();
        }
    }


    /* get user's preference of chassis and weapon from GUI */
    public override void GetUserPref() {
        // TODO
        this.chassis_pref = "power++";
        this.weapon_pref = "bullspd++";
    }


    string level_s; // level info: "level1", "level2", "level3"
    /* Refresh infantry state when born, reborn or level up */
    public override void Configure() {
        /* configure chassis params */
        level_s = string.Format("level{0}", this.level+1);
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

        var jobj = AssetManager.singleton.exp["infantry"][level_s];
        this.expval = jobj["have"].ToObject<int>();
        this.maxexp = level<2 ? jobj["need"].ToObject<int>() : int.MaxValue;
    }

}
