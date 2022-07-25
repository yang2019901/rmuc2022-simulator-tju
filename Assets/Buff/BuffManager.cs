using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* use abstract class and function to provide general calling form */
public abstract class Buff : MonoBehaviour {
    /* called in FixedUpdate() */
    public abstract void update();
    public void reset() {
        timer = 2;
    }
    public virtual void init() {
        robot = GetComponent<RobotState>();
    }
    public float timer;
    public bool en;
    public RobotState robot;
}

/* Buff of Revive */
public class B_Revive : Buff {
    float rate;
    public void enable(float revive_rate) {
        rate = revive_rate;
        en = true;
        timer = 2;
    }

    public void disable() {
        en = false;
    }

    public override void update() {
        /* no buff */
        if (!en)
            return;
        /* have buff - revive */
        robot.blood_left += Mathf.CeilToInt(robot.blood * rate);
        robot.blood_left = robot.blood_left < robot.blood ? robot.blood_left : robot.blood;
        /* deal with timer */
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
            disable();
    }
}

/* Buff of Base */
public class B_Base : Buff {
    public void enable() {
        /* add buff - Base */
        robot.B_dfc += 0.5f;
        robot.B_cd += 3f;
        en = true;
        timer = 2;
    }

    public void disable() {
        /* remove buff - Base */
        robot.B_dfc -= 0.5f;
        robot.B_cd -= 3f;
        en = false;
    }

    public override void update() {
        /* no buff */
        if (!en)
            return;
        /* deal with timer */
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
            disable();
    }
}

/* Buff of High Ground */
public class B_HighGnd : Buff {
    public void enable() {
        /* add buff - HighGnd */
        robot.B_cd += 5f;
        en = true;
        timer = 2;
    }

    public void disable() {
        robot.B_cd -= 5f;
        en = false;
    }

    public override void update() {
        /* no buff */
        if (!en)
            return;
        /* deal with timer */
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
            disable();
    }
}

/* Buff of Rune Activation Spot */
public class B_RuneActiv : Buff {
    RuneState rune_state;
    public override void init() {
        robot = GetComponent<RobotState>();
        Rune rune = BattleField.singleton.rune;
        rune_state = (robot.armor_color == ArmorColor.Red) ? rune.rune_state_red : rune.rune_state_blue;
    }

    public void enable() {
        /* add buff - HighGnd */
        robot.B_cd += 5f;
        en = true;
        timer = 2;
        rune_state.SetActiveState(Activation.Hitting);
    }

    public void disable() {
        robot.B_cd -= 5f;
        en = false;
        rune_state.SetActiveState(Activation.Idle);
    }

    public override void update() {
        /* no buff */
        if (!en)
            return;
        /* deal with timer */
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
            disable();
    }
}

/* Buff of Outpost */
public class B_Outpost : Buff {
    public void enable() {
        /* add buff - outpost */
        robot.B_cd += 5f;
        en = true;
        timer = 2;
    }

    public void disable() {
        /* remove buff - outpost */
        robot.B_cd -= 5f;
        en = false;
    }

    public override void update() {
        /* no buff */
        if (!en)
            return;
        /* deal with timer */
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
            disable();
    }
}

/* Buff of Island */
public class B_Island : Buff {
    public void enable() {
        /* add buff - island */
        robot.B_dfc += 0.5f;
        en = true;
        timer = 2;
    }

    public void disable() {
        /* remove buff - island */
        robot.B_cd -= 0.5f;
        en = false;
    }

    public override void update() {
        /* no buff */
        if (!en)
            return;
        /* deal with timer */
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
            disable();
    }
}

/* Buff of Snipe */
/* TODO: Add HeroState.cs */
public class B_Snipe : Buff {
    public void enable() {
        /* add buff - Snipe */
        /* TODO */
        en = true;
        timer = 2;
    }

    public void disable() {
        /* remove buff - Snipe */
        /* TODO */
        en = false;
    }

    public override void update() {
        /* no buff */
        if (!en)
            return;
        /* deal with timer */
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
            disable();
    }
}

/* Buff of Leap */
public class B_Leap : Buff {
    public void enable() {
        /* add buff - Leap */
        robot.B_dfc += 0.5f;
        robot.B_cd += 3f;
        robot.B_pow = 250f;
        en = true;
        timer = 20;
    }

    public void disable() {
        /* remove buff - Leap */
        robot.B_dfc -= 0.5f;
        robot.B_cd -= 3f;
        en = false;
    }

    public override void update() {
        /* no buff */
        if (!en)
            return;
        /* deal with timer */
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
            disable();
    }
}


public class BuffManager : MonoBehaviour {
    void OnTriggerEnter() {

    }

    void OnTriggerStay() {

    }

    void OnTriggerExit() {

    }
}
