using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneState : RoboState {
    public override RoboSync Pull() {
        RoboSync rs = base.Pull();
        rs.has_blood = false;
        rs.has_level = false;
        rs.has_wpn = true;
        return rs;
    }

    public override void Configure() {
        this.maxblood = 0;
        this.currblood = 0;
        this.bullspd = 30;
    }
}
