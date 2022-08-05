using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* use abstract class and function to provide general calling form */
public abstract class Buff {
    /* make sure Update() is called in FixedUpdate() */
    public abstract void Update();
    public virtual void Enable() {
        timer = 2;
        en = true;
    }
    public abstract void Disable();
    public void reset() {
        timer = 2;
    }
    public virtual void init(RobotState robot_state) {
        robot = robot_state;
    }
    public float timer;
    public bool en;
    public RobotState robot;
}

/* Buff of Revive */
public class B_Revive : Buff {
    float rate;
    public void Enable(float revive_rate) {
        base.Enable();
        /* add buff - Revive */
        rate = revive_rate;
    }

    public override void Disable() {
        en = false;
    }

    public override void Update() {
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
    public override void Enable() {
        base.Enable();
        /* add buff - Base */
        robot.li_B_dfc.Add(0.5f);
        robot.li_B_cd.Add(3f);
        robot.UpdateBuff();
    }

    public override void Disable() {
        /* remove buff - Base */
        robot.li_B_dfc.Remove(0.5f);
        robot.li_B_cd.Remove(3f);
        robot.UpdateBuff();
        en = false;
    }

    public override void Update() {
        /* no buff */
        if (!en)
            return;
        /* deal with timer */
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
            Disable();
    }
}

/* Buff of High Ground */
public class B_HighGnd : Buff {
    public override void Enable() {
        base.Enable();
        /* add buff - HighGnd */
        robot.li_B_cd.Add(5f);
        robot.UpdateBuff();
    }

    public override void Disable() {
        robot.li_B_cd.Remove(5f);
        robot.UpdateBuff();
        en = false;
    }

    public override void Update() {
        /* no buff */
        if (!en)
            return;
        /* deal with timer */
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
            Disable();
    }
}

/* Buff of Rune Activation Spot */
public class B_RuneActiv : Buff {
    RuneState rune_state;
    public override void init(RobotState robot_state) {
        robot = robot_state;
        Rune rune = BattleField.singleton.rune;
        rune_state = (robot.armor_color == ArmorColor.Red) ? rune.rune_state_red : rune.rune_state_blue;
    }

    public override void Enable() {
        base.Enable();
        /* add buff - HighGnd */
        robot.li_B_cd.Add(5f);
        robot.UpdateBuff();
        rune_state.SetActiveState(Activation.Hitting);
    }

    public override void Disable() {
        robot.li_B_cd.Remove(5f);
        robot.UpdateBuff();
        en = false;
        rune_state.SetActiveState(Activation.Idle);
    }

    public override void Update() {
        /* no buff */
        if (!en)
            return;
        /* deal with timer */
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
            Disable();
    }
}

/* Buff of Outpost */
public class B_Outpost : Buff {
    public override void Enable() {
        base.Enable();
        /* add buff - outpost */
        robot.li_B_cd.Add(5f);
        robot.UpdateBuff();
    }

    public override void Disable() {
        /* remove buff - outpost */
        robot.li_B_cd.Remove(5f);
        robot.UpdateBuff();
        en = false;
    }

    public override void Update() {
        /* no buff */
        if (!en)
            return;
        /* deal with timer */
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
            Disable();
    }
}

/* Buff of Island */
public class B_Island : Buff {
    public override void Enable() {
        base.Enable();
        /* add buff - island */
        robot.li_B_dfc.Add(0.5f);
        robot.UpdateBuff();
    }

    public override void Disable() {
        /* remove buff - island */
        robot.li_B_cd.Remove(0.5f);
        robot.UpdateBuff();
        en = false;
    }

    public override void Update() {
        /* no buff */
        if (!en)
            return;
        /* deal with timer */
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
            Disable();
    }
}

/* Buff of Snipe */
/* TODO: Add HeroState.cs */
public class B_Snipe : Buff {
    public override void Enable() {
        base.Enable();
        /* add buff - Snipe */
        /* TODO */
    }

    public override void Disable() {
        /* remove buff - Snipe */
        /* TODO */
        en = false;
    }

    public override void Update() {
        /* no buff */
        if (!en)
            return;
        /* deal with timer */
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
            Disable();
    }
}

/* Buff of Leap */
public class B_Leap : Buff {
    public override void Enable() {
        base.Enable();
        /* add buff - Leap */
        robot.li_B_dfc.Add(0.5f);
        robot.li_B_cd.Add(3f);
        robot.B_pow = 250f;
        robot.UpdateBuff();
    }

    public override void Disable() {
        /* remove buff - Leap */
        robot.li_B_dfc.Remove(0.5f);
        robot.li_B_cd.Remove(3f);
        robot.UpdateBuff();
        en = false;
    }

    public override void Update() {
        /* no buff */
        if (!en)
            return;
        /* deal with timer */
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
            Disable();
    }
}

public class BuffManager : MonoBehaviour {
    private RobotState robot;
    private Dictionary<string, Buff> buffs;
    private string color_s;

    const string rev = "B_Revive";
    const string bas = "B_Base";
    const string gnd = "B_HighGnd";
    const string run = "B_RuneActiv";
    const string pst = "B_Outpost";
    const string lnd = "B_Island";
    const string snp = "B_Snipe";
    const string lea = "B_Leap";

    void Start() {
        robot = GetComponentInParent<RobotState>();
        buffs = new Dictionary<string, Buff> {
            { rev, new B_Revive() },
            { bas, new B_Base() },
            { gnd, new B_HighGnd() },
            { run, new B_RuneActiv() },
            { pst, new B_Outpost() },
            { lnd, new B_Island() },
            { snp, new B_Snipe() },
            { lea, new B_Leap() }
        };
        foreach (Buff tmp in buffs.Values) {
            tmp.init(robot);
        }
        color_s = robot.armor_color == ArmorColor.Red ? "red" : "blue";
    }

    void FixedUpdate()
    {
        foreach (Buff tmp in buffs.Values) {
            tmp.Update();
        }
    }

    void OnTriggerEnter(Collider col) {
        char sep = ' ';
        string prefix = col.name.Split(sep)[0];
        Debug.Log("enter buff_gnd: " + col.name);
        switch (prefix) {
            case rev:
                if (col.name.Contains(color_s))
                    buffs[rev].Enable();
                break;
            case bas:
                if (col.name.Contains(color_s))
                    buffs[bas].Enable();
                break;
            case gnd:
                if (col.name.Contains(color_s)){
                    buffs[gnd].Enable();
                    /* take over the high ground => change its name so that enemy can't share the buff */
                    col.name = gnd + sep + color_s;
                }
                break;
            case run:
                break;
            case pst:
                break;
            case lnd:
                break;
            case snp:
                break;
            case lea:
                break;
            default:
                Debug.Log(col.name);
                break;
        }
    }

    void OnTriggerStay(Collider col) {

    }

    void OnTriggerExit(Collider col) {
        char sep = ' ';
        string prefix = col.name.Split(sep)[0];
        Debug.Log("leave buff_gnd: " + col.name);
        switch (prefix) {
            case rev:
                if (col.name.Contains(color_s))
                    buffs[rev].Disable();
                break;
            case bas:
                if (col.name.Contains(color_s))
                    buffs[bas].Disable();
                break;
            case gnd:
                if (col.name.Contains(color_s)){
                    buffs[gnd].Disable();
                    /* take over the high ground => change its name so that enemy can't share the buff */
                    col.name = gnd + sep + color_s;
                }
                break;
            case run:
                break;
            case pst:
                break;
            case lnd:
                break;
            case snp:
                break;
            case lea:
                break;
            default:
                Debug.Log(col.name);
                break;
        }
    }
}
