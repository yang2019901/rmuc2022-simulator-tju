using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardState : RoboState {
    Weapon wpn;


    public override void Awake() {
        base.Awake();
        wpn = GetComponent<Weapon>();
    }


    public override void Start()
    {
        base.Start();
        wpn.bullnum = 500;   
    }


    public override RoboSync Pull() {
        RoboSync rs = base.Pull();
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

    }


    public override void Configure() {
        this.maxblood = 600;
        this.maxheat = 320;
        this.cooldown = 100;
        this.currblood = 600;
        this.bullspd = 30;
    }
}
