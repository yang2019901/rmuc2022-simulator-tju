using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Buff {
    /* called in FixedUpdate() */
    public abstract void Update(RobotState robot);
    public float timer;
    public bool en;
}

/* Buff of Revive */
public class B_Revive : Buff {
    float rate;
    public void Enable(float revive_rate) {
        rate = revive_rate;
        en = true;
        timer = 2;
    }

    public void Disable() {
        en = false;
    }

    /* continously called to maintain the buff */
    public void Reset() {
        timer = 2;
    }

    public override void Update(RobotState robot) {
        /* no buff */
        if (!en)
            return;
        /* have buff - revive */
        robot.blood_left += Mathf.CeilToInt(robot.blood * rate);
        robot.blood_left = robot.blood_left < robot.blood ? robot.blood_left : robot.blood;
        /* deal with timer */
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
            Disable();
    }
}

/* Buff of Base */
public class B_Base : Buff {
    public void Enable() {
        en = true;
        timer = 2;
    }

    public void Disable() {
        en = false;
    }

    public override void Update(RobotState robot) {
         /* no buff */
        if (!en)
            return;
        /* have buff - Base */
        robot.
        /* deal with timer */
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
            Disable();
    }
}

public class BuffManager : MonoBehaviour {
    void OnTriggerEnter() {

    }
}
