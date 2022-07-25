using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfantryState : RobotState {
    private string chassis_pref; // chassis preference: "power++", "blood++", "init_mode"
    private string weapon_pref; // weapon preference: "shoot_speed++" "heat_limit++" "cool_down++"
    private string level; // level info: "level1", "level2", "level3"
    /* cache */
    private ArmorController[] acs;

    void Start() {
        GetUserPref();
        Configure();
        this.acs = GetComponentsInChildren<ArmorController>();
        this.blood_left = this.blood;
    }


    /* get user's preference of chassis and weapon from GUI */
    private void GetUserPref() {
        // TODO
        this.chassis_pref = "power++";
        this.weapon_pref = "shoot_speed++";
        this.level = "level1";
    }


    public override void TakeDamage(GameObject hitter, GameObject armor_hit, GameObject bullet) {
        /* Requirement: make sure that small bullet's name contains "17mm" && big bullet's contains "42mm" */
        int damage = bullet.name.Contains("17mm") ? 10 : 100;

        /* take account of hitter rune buff */
        RuneBuff hitter_buff = BattleField.singleton.GetRuneBuff(hitter.GetComponent<BasicState>().armor_color);
        if (hitter_buff == RuneBuff.Junior)
            damage = Mathf.RoundToInt(1.5f * damage);
        else if (hitter_buff == RuneBuff.Senior)
            damage = 2 * damage;

        /* take account of hittee rune buff */
        if (BattleField.singleton.GetRuneBuff(this.armor_color) == RuneBuff.Senior)
            damage /= 2;

        blood_left -= damage;

        Debug.Log("current blood: " + blood_left);

        if (blood_left <= 0) {
            blood_left = 0;
            foreach (ArmorController ac in acs)
                ac.Disable();
            BattleField.singleton.Kill(hitter, this.gameObject);
        } else
            StartCoroutine("ArmorsBlink", 0.1f);
    }


    private void Configure() {
        /* configure chassis params */
        var tmp = AssetManager.singleton.infa_chs[this.chassis_pref];
        if (this.chassis_pref != "init_mode")
            tmp = tmp[this.level];
        this.blood = tmp["blood"].ToObject<int>();
        this.power = tmp["power"].ToObject<int>();
        /* configure weapon */
        tmp = AssetManager.singleton.weapon["17mm"][this.weapon_pref];
        if (this.weapon_pref != "init_mode")
            tmp = tmp[this.level];
        this.heat_limit = tmp["heat_limit"].ToObject<int>();
        this.cool_down = tmp["cool_down"].ToObject<int>();
        this.shoot_speed = tmp["shoot_speed"].ToObject<int>();
    }


    private IEnumerator ArmorsBlink(float interval) {
        foreach (ArmorController ac in acs)
            ac.SetLight(false);
        yield return new WaitForSeconds(interval);
        foreach (ArmorController ac in acs)
            ac.SetLight(true);
    }
}
