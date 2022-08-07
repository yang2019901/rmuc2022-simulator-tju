using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* use abstract class and function to provide general calling form */
public abstract class Buff {
    /* make sure Update() is called in FixedUpdate() */
    public abstract void Update();
    public virtual void Enable(Collider collider) {
        timer = 2;
        en = true;
        col = collider;
    }
    public virtual void init(RobotState robot_state, string my_color, string enemy_color) {
        robot = robot_state;
        my_color_s = my_color;
        enemy_color_s = enemy_color;
    }
    public float timer;
    public bool en;
    public RobotState robot;
    protected Collider col;
    protected string my_color_s;
    protected string enemy_color_s;
}

/* Buff of Revive */
public class B_Revive : Buff {
    public override void Enable(Collider col) {
        /* add buff - Revive */
        if (!en) {
            robot.li_B_rev.Add(0.05f);
            robot.UpdateBuff();
        }
        base.Enable(col);
    }

    public void Disable() {
        if (!en)
            return;
        robot.li_B_rev.Remove(0.05f);
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

/* Buff of Base */
public class B_Base : Buff {
    public BaseState base_state;
    public override void init(RobotState robot_state, string my_color, string enemy_color) {
        base.init(robot_state, my_color, enemy_color);
        base_state = (robot.armor_color == ArmorColor.Red) ? BattleField.singleton.base_red
            : BattleField.singleton.base_blue;
    }

    public override void Enable(Collider col) {
        if (!en) {
            /* add buff - Base */
            robot.li_B_dfc.Add(0.5f);
            robot.li_B_cd.Add(3f);
            robot.UpdateBuff();
        }
        base.Enable(col);
    }

    public void Disable() {
        if (!en)
            return;
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
    public override void Enable(Collider col) {
        if (!en) {
            /* add buff - HighGnd */
            robot.li_B_cd.Add(5f);
            robot.UpdateBuff();
        }
        base.Enable(col);
    }

    public void Disable() {
        if (!en)
            return;
        /* release high ground control */
        col.name += enemy_color_s;
        /* remove buff - HighGnd */
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
    public override void init(RobotState robot_state, string my_color, string enemy_color) {
        base.init(robot_state, my_color, enemy_color);
        Rune rune = BattleField.singleton.rune;
        rune_state = (robot.armor_color == ArmorColor.Red) ? rune.rune_state_red : rune.rune_state_blue;
    }

    public override void Enable(Collider col) {
        if (!en) {
            /* add buff - HighGnd */
            robot.li_B_cd.Add(5f);
            robot.UpdateBuff();
            rune_state.SetActiveState(Activation.Hitting);
        }
        base.Enable(col);
    }

    public void Disable() {
        if (!en)
            return;
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
    public OutpostState outpost_state;
    public override void init(RobotState robot_state, string my_color, string enemy_color) {
        base.init(robot_state, my_color, enemy_color);
        outpost_state = (robot.armor_color == ArmorColor.Red) ? BattleField.singleton.outpost_red
            : BattleField.singleton.outpost_blue;
    }

    public override void Enable(Collider col) {
        if (!en) {
            /* add buff - outpost */
            robot.li_B_cd.Add(5f);
            robot.UpdateBuff();
        }
        base.Enable(col);
    }

    public void Disable() {
        if (!en)
            return;
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
    public override void Enable(Collider col) {
        if (!en) {
            /* add buff - Island */
            robot.li_B_dfc.Add(0.5f);
            robot.UpdateBuff();
        }
        base.Enable(col);
    }

    public void Disable() {
        if (!en)
            return;
        /* release high ground control */
        col.name += enemy_color_s;
        /* remove buff - Island */
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
    public override void Enable(Collider col) {
        if (!en) {
            /* add buff - Snipe */
            ((HeroState)robot).sniping = true;
        }
        base.Enable(col);
    }

    public void Disable() {
        if (!en)
            return;
        /* remove buff - Snipe */
        ((HeroState)robot).sniping = false;
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
    public override void Enable(Collider col) {
        en = true;
        timer = 20;
        /* add buff - Leap */
        robot.li_B_dfc.Add(0.5f);
        robot.li_B_cd.Add(3f);
        robot.B_pow = 250f;
        robot.UpdateBuff();
    }

    public void Disable() {
        if (!en)
            return;
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

    float timer_rune;
    bool leaping;
    float timer_leap;
    string tag_leap;
    string my_color_s;
    string enemy_color_s;
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
        my_color_s = robot.armor_color == ArmorColor.Red ? "red" : "blue";
        enemy_color_s = robot.armor_color == ArmorColor.Blue ? "red" : "blue";
        foreach (Buff tmp in buffs.Values) {
            tmp.init(robot, my_color_s, enemy_color_s);
        }
        leaping = false;
    }

    void FixedUpdate() {
        foreach (Buff tmp in buffs.Values) {
            tmp.Update();
        }
    }

    void OnTriggerEnter(Collider col) {
        // Debug.Log("enter buff_gnd: " + col.name);
        if (col.name.Contains(run)) {
            timer_rune = Time.time;
        } else if (col.name.Contains(lea)) {
            if (!leaping || Time.time - timer_leap >= 10 || Time.time - timer_leap < 0)
                return;
            if (col.name.Contains("end") && col.name.Contains(tag_leap)) {
                buffs[lea].Enable(col);
                leaping = false;
            }
        }
    }

    void OnTriggerStay(Collider col) {
        char sep = ' ';
        string prefix = col.name.Split(sep)[0];
        switch (prefix) {
            case rev:
                if (col.name.Contains(my_color_s))
                    buffs[rev].Enable(col);
                break;
            case bas:
                if (col.name.Contains(my_color_s)) {
                    BaseState tmp = ((B_Base)buffs[bas]).base_state;
                    if (tmp.active && tmp.buff_active)
                        buffs[bas].Enable(col);
                }
                break;
            case gnd:
                if (col.name.Contains(my_color_s)) {
                    buffs[gnd].Enable(col);
                    /* take over the high ground => change its name so that enemy can't share the buff */
                    col.name = col.name.Replace(enemy_color_s, "");
                }
                break;
            case run:
                if (Time.time - timer_rune > 3 && col.name.Contains(my_color_s))
                    buffs[run].Enable(col);
                break;
            case pst:
                if (col.name.Contains(my_color_s)) {
                    OutpostState tmp = ((B_Outpost)buffs[pst]).outpost_state;
                    if (tmp.active && tmp.buff_active)
                        buffs[pst].Enable(col);
                }
                break;
            case lnd:
                if (col.name.Contains(my_color_s) && robot.gameObject.name.Contains("engineer")) {
                    buffs[lnd].Enable(col);
                    /* take over the high ground => change its name so that enemy can't share the buff */
                    col.name = col.name.Replace(enemy_color_s, "");
                }
                break;
            case snp:
                if (col.name.Contains(my_color_s) && robot.gameObject.name.Contains("hero")) {
                    buffs[snp].Enable(col);
                }
                break;
            case lea:
                break;
            default:
                Debug.Log(col.name);
                break;
        }
    }

    void OnTriggerExit(Collider col) {
        char sep = ' ';
        string prefix = col.name.Split(sep)[0];
        // Debug.Log("leave buff_gnd: " + col.name);

        if (prefix == lea && col.name.Contains("start")) {
            leaping = true;
            timer_leap = Time.time;
            tag_leap = col.name.Split(sep)[1];
        }
    }
}
