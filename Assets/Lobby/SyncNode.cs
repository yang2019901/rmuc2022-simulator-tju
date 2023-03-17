using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

using RMUC_UI;

/******************************************************************
 * NOTE: only visual-effect-related variables needs synchronizing *
 ******************************************************************/
public struct RuneSync {
    public RuneLight[] blades_light;
    public bool center_light;
    public int idx_target;
}

public struct OutpostSync {
    public Vector3 rot;
    /* set base animation */
    public bool survival;
    public bool invul;
    /* set blood bar and armor blinking */
    public int currblood;
}

public struct BaseSync {
    public bool survival;
    public bool invul;
    public int shield;
    /* set blood bar and armor blinking */
    public int currblood;
}

public struct RoboSync {
    /* survival, dead, defence up or invulnerable */
    public BatStat bat_stat;
    /* set blood bar and armor blinking */
    public bool has_blood;
    public int currblood;
    public int maxblood;
    /* set RMUC_UI.AvaBatStat */
    public bool has_level;
    public int level;
    public int currexp;
    public bool has_wpn;
    public int bull_num;
    public float heat_ratio;
}

public struct BatSync {
    public int money_red;
    public int money_red_max;
    public int money_blue;
    public int money_blue_max;
    public int score_red;
    public int score_blue;
    public float time_bat;
}

public struct UISync {
    public SyncList<RoboSync> robots;
    public BaseSync bs_r;
    public BaseSync bs_b;
    public OutpostSync os_r;
    public OutpostSync os_b;
    public RoboSync gs_r;
    public RoboSync gs_b;
    public BatSync bat_sync;

    public UISync(SyncList<RoboSync> roboSyncs, BaseSync bs_r, BaseSync bs_b, OutpostSync os_r,
        OutpostSync os_b, RoboSync gs_r, RoboSync gs_b, BatSync bat_sync) {
        this.robots = roboSyncs;
        this.bs_r = bs_r;
        this.bs_b = bs_b;
        this.os_r = os_r;
        this.os_b = os_b;
        this.gs_r = gs_r;
        this.gs_b = gs_b;
        this.bat_sync = bat_sync;
    }
}

public class SyncNode : NetworkBehaviour {
    [SyncVar] private bool ready_push = false;

    [SyncVar] private BatSync bat_sync;

    /* rune */
    [SyncVar] private Vector3 rune_rot = new Vector3();
    [SyncVar] private float a, w, t;
    [SyncVar] private int sgn;
    [SyncVar] private RuneBuff rune_buff;
    [SyncVar] private RuneSync rune_sync_red = new RuneSync();
    [SyncVar] private RuneSync rune_sync_blue = new RuneSync();

    /* outpost */
    [SyncVar] private OutpostSync otpt_sync_red = new OutpostSync();
    [SyncVar] private OutpostSync otpt_sync_blue = new OutpostSync();

    /* base */
    [SyncVar] private BaseSync base_sync_red = new BaseSync();
    [SyncVar] private BaseSync base_sync_blue = new BaseSync();

    /* guard */
    [SyncVar] private RoboSync guard_sync_red = new RoboSync();
    [SyncVar] private RoboSync guard_sync_blue = new RoboSync();

    /* robots */
    /* Note: SyncList can and only can be modify in Server */
    private readonly SyncList<RoboSync> robo_sync_all = new SyncList<RoboSync>();


    /****************** alias ****************/
    Rune rune;
    OutpostState outpost_red;
    OutpostState outpost_blue;
    BaseState base_red;
    BaseState base_blue;
    GuardState guard_red;
    GuardState guard_blue;
    /* when sync, robo_red has no difference with robo_blue */
    List<RoboState> robo_all;


    [ClientRpc]
    public void RpcActivateRune(ArmorColor armor_color, RuneBuff rune_buff) {
        if (NetworkServer.active)   // already activated in server PC
            return;

        StartCoroutine(BattleField.singleton.ActivateRune(armor_color, rune_buff));
    }


    [ClientRpc]
    public void RpcKill(string hitter_s, string hittee_s) {
        if (NetworkServer.active)
            return;

        BasicState hitter = BattleField.singleton.team_all.Find(i => i.name == hitter_s);
        BasicState hittee = BattleField.singleton.team_all.Find(i => i.name == hittee_s);

        BattleField.singleton.Kill(hitter.gameObject, hittee.gameObject);
    }


    void Start() {
        /* battlefield is initialized in Awake period, which justify following assignment */
        rune = BattleField.singleton.rune;
        outpost_blue = BattleField.singleton.outpost_blue;
        outpost_red = BattleField.singleton.outpost_red;
        base_red = BattleField.singleton.base_red;
        base_blue = BattleField.singleton.base_blue;
        robo_all = BattleField.singleton.robo_all;
        guard_red = BattleField.singleton.guard_red;
        guard_blue = BattleField.singleton.guard_blue;
        if (isServer)
            for (int i = 0; i < robo_all.Count; i++)
                robo_sync_all.Add(new RoboSync());
    }

    /* use LateUpdate() to ensure users see these */
    void LateUpdate() {
        if (isServer) {
            bat_sync = BattleField.singleton.Pull();
            /* pulls rune appearence from server PC */
            rune_rot = rune.rotator_rune.localEulerAngles;
            a = rune.a;
            w = rune.w;
            t = rune.t;
            sgn = rune.sgn;
            rune_buff = rune.rune_buff;
            rune_sync_red = rune.rune_state_red.Pull();
            rune_sync_blue = rune.rune_state_blue.Pull();
            /* pulls outpost appearence from server PC */
            otpt_sync_red = outpost_red.Pull();
            otpt_sync_blue = outpost_blue.Pull();
            /* pulls base appearence from server PC */
            base_sync_red = base_red.Pull();
            base_sync_blue = base_blue.Pull();
            /* pulls robots appearence from server PC */
            for (int i = 0; i < robo_sync_all.Count; i++) {
                robo_sync_all[i] = robo_all[i].Pull();
            }
            /* pulls guard appearence from server PC */
            guard_sync_red = guard_red.Pull();
            guard_sync_blue = guard_blue.Pull();
            ready_push = true;
        }

        // Note: When battlefield is first loaded, push() may be called in client PC earlier 
        //       than pull() in server PC
        //       which will raise error of null reference in client PC
        if (isClient && ready_push) {
            BattleField.singleton.Push(bat_sync);
            /* pushes rune appearence to client PC */
            rune.rotator_rune.localEulerAngles = rune_rot;
            rune.a = a;
            rune.w = w;
            rune.t = t;
            rune.sgn = sgn;
            rune.rune_buff = rune_buff;
            rune.rune_state_red.Push(rune_sync_red);
            rune.rune_state_blue.Push(rune_sync_blue);
            /* pushes outpost appearence to client PC */
            outpost_red.Push(otpt_sync_red);
            outpost_blue.Push(otpt_sync_blue);
            /* pushes base appearence to client PC */
            base_red.Push(base_sync_red);
            base_blue.Push(base_sync_blue);
            /* pushes robots appearence to client PC */
            for (int i = 0; i < robo_all.Count; i++) {
                robo_all[i].Push(robo_sync_all[i]);
            }
            /* pushes guard appearence to client PC */
            guard_red.Push(guard_sync_red);
            guard_blue.Push(guard_sync_blue);
            /* host needs that to update UI as well */
            BattleField.singleton.bat_ui.Push(new UISync(robo_sync_all, base_sync_red, base_sync_blue,
                otpt_sync_red, otpt_sync_blue, guard_sync_red, guard_sync_blue, bat_sync));
        }
    }
}
