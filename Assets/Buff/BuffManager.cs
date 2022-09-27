/// <summary>
/// BuffManager.cs belongs to Game Logic; should only run in server PC
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/* use abstract class and function to provide general calling form */
public abstract class Buff {
    public string tag;
    /* make sure Update() is called in FixedUpdate() */
    public abstract void Update();
    /* Note: Enable() may be called every frame. Make sure list.Add() won't be called multiple times */
    public virtual void Enable(Collider collider) {
        if (!en) {
            robot.robo_buff.Add(this);
        }
        col = collider;
        timer = 2;
        en = true;
    }
    public virtual void init(RoboState robo_state, string my_color, string enemy_color) {
        robot = robo_state;
        my_color_s = my_color;
        enemy_color_s = enemy_color;
    }
    public virtual void Disable() {
        if (!en)
            return ;
        robot.robo_buff.Remove(this);
    }
    public float timer;
    public bool en;
    protected RoboState robot;
    protected Collider col;
    protected string my_color_s;
    protected string enemy_color_s;
}

/* Buff of Revive 
    Note: engineer's self-reviving buff will not be considered here 
    because it's designed to deal with ground buff
*/
public class B_Revive : Buff {
    public override void Enable(Collider col) {
        /* add buff - Revive */
        if (!en) {
            robot.li_B_rev.Add(0.05f);
            robot.li_B_rbn.Add(2);
            robot.UpdateBuff();
        }
        base.Enable(col);
    }

    public override void Disable() {
        base.Disable();
        if (!en)
            return;
        robot.li_B_rev.Remove(0.05f);
        robot.li_B_rbn.Remove(2);
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
    public override void init(RoboState robot_state, string my_color, string enemy_color) {
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

    public override void Disable() {
        base.Disable();
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

/* Buff of Upland */
public class B_Upland : Buff {
    public override void Enable(Collider col) {
        if (!en) {
            /* add buff - Upland */
            robot.li_B_cd.Add(5f);
            robot.UpdateBuff();
        }
        base.Enable(col);
    }

    public override void Disable() {
        base.Disable();
        if (!en)
            return;
        /* release Upland control */
        col.name += enemy_color_s;
        /* remove buff - Upland */
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
    public override void init(RoboState robot_state, string my_color, string enemy_color) {
        base.init(robot_state, my_color, enemy_color);
        Rune rune = BattleField.singleton.rune;
        rune_state = (robot.armor_color == ArmorColor.Red) ? rune.rune_state_red : rune.rune_state_blue;
    }

    public override void Enable(Collider col) {
        if (!en) {
            /* add buff - Upland */
            robot.li_B_cd.Add(5f);
            robot.UpdateBuff();
        }
        rune_state.SetActiveState(Activation.Hitting);
        base.Enable(col);
    }

    public override void Disable() {
        base.Disable();
        if (!en)
            return;
        robot.li_B_cd.Remove(5f);
        robot.UpdateBuff();
        en = false;
        if (!BattleField.singleton.rune.activated)
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
    public override void init(RoboState robo_state, string my_color, string enemy_color) {
        base.init(robo_state, my_color, enemy_color);
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

    public override void Disable() {
        base.Disable();
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

    public override void Disable() {
        base.Disable();
        if (!en)
            return;
        /* release Upland control */
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

    public override void Disable() {
        base.Disable();
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

    public override void Disable() {
        base.Disable();
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

public class BuffType {
    public const string rev = "B_Revive";
    public const string bas = "B_Base";
    public const string uld = "B_Upland";
    public const string run = "B_RuneActiv";
    public const string pst = "B_Outpost";
    public const string ild = "B_Island";
    public const string snp = "B_Snipe";
    public const string lea = "B_Leap";
}

public class BuffManager : MonoBehaviour {
    private RoboState robot;
    private Dictionary<string, Buff> buffs;

    float timer_rune;
    bool leaping;
    float timer_leap;
    string tag_leap;
    string my_color_s;
    string enemy_color_s;

    void Start() {
        robot = GetComponentInParent<RoboState>();
        buffs = new Dictionary<string, Buff> {
            { BuffType.rev, new B_Revive() },
            { BuffType.bas, new B_Base() },
            { BuffType.uld, new B_Upland() },
            { BuffType.run, new B_RuneActiv() },
            { BuffType.pst, new B_Outpost() },
            { BuffType.ild, new B_Island() },
            { BuffType.snp, new B_Snipe() },
            { BuffType.lea, new B_Leap() }
        };
        my_color_s = robot.armor_color == ArmorColor.Red ? "red" : "blue";
        enemy_color_s = robot.armor_color == ArmorColor.Blue ? "red" : "blue";
        foreach (string key in buffs.Keys) {
            buffs[key].tag = key;
            buffs[key].init(robot, my_color_s, enemy_color_s);
        }
        leaping = false;
    }

    void FixedUpdate() {
        if (!NetworkServer.active)
            return ;
        foreach (Buff tmp in buffs.Values) {
            tmp.Update();
        }
    }

    void OnTriggerEnter(Collider col) {
        // Debug.Log("enter buff_uld: " + col.name);
        if (col.name.Contains(BuffType.run) && !buffs[BuffType.run].en) {
            timer_rune = Time.time;
        } else if (col.name.Contains(BuffType.lea)) {
            if (!leaping || Time.time - timer_leap >= 10 || Time.time - timer_leap < 0)
                return;
            if (col.name.Contains("end") && col.name.Contains(tag_leap)) {
                buffs[BuffType.lea].Enable(col);
                leaping = false;
            }
        }
    }

    void OnTriggerStay(Collider col) {
        char sep = ' ';
        string prefix = col.name.Split(sep)[0];
        switch (prefix) {
            case BuffType.rev:
                if (col.name.Contains(my_color_s))
                    buffs[BuffType.rev].Enable(col);
                break;
            case BuffType.bas:
                if (col.name.Contains(my_color_s)) {
                    BaseState tmp = ((B_Base)buffs[BuffType.bas]).base_state;
                    if (tmp.survival && tmp.buff_active)
                        buffs[BuffType.bas].Enable(col);
                }
                break;
            case BuffType.uld:
                if (col.name.Contains(my_color_s)) {
                    buffs[BuffType.uld].Enable(col);
                    /* take over the Upland => change its name so that enemy can't share the buff */
                    col.name = col.name.Replace(enemy_color_s, "");
                }
                break;
            case BuffType.run:
                if (Time.time - timer_rune > 3 && col.name.Contains(my_color_s))
                    buffs[BuffType.run].Enable(col);
                break;
            case BuffType.pst:
                if (col.name.Contains(my_color_s)) {
                    OutpostState tmp = ((B_Outpost)buffs[BuffType.pst]).outpost_state;
                    if (tmp.survival && tmp.buff_active)
                        buffs[BuffType.pst].Enable(col);
                }
                break;
            case BuffType.ild:
                if (col.name.Contains(my_color_s) && robot.gameObject.name.Contains("engineer")) {
                    buffs[BuffType.ild].Enable(col);
                    /* take over the Upland => change its name so that enemy can't share the buff */
                    col.name = col.name.Replace(enemy_color_s, "");
                }
                break;
            case BuffType.snp:
                if (col.name.Contains(my_color_s) && robot.gameObject.name.Contains("hero")) {
                    buffs[BuffType.snp].Enable(col);
                }
                break;
            case BuffType.lea:
                break;
            default:
                Debug.Log(col.name);
                break;
        }
    }

    void OnTriggerExit(Collider col) {
        char sep = ' ';
        string prefix = col.name.Split(sep)[0];
        // Debug.Log("leave buff_uld: " + col.name);

        if (prefix == BuffType.lea && col.name.Contains("start")) {
            leaping = true;
            timer_leap = Time.time;
            tag_leap = col.name.Split(sep)[1];
        }
    }
}
