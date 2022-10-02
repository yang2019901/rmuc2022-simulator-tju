using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/******************************************************************
 * NOTE: only visual-effect-related variables needs synchronizing *
 ******************************************************************/
public struct RuneSync {
    public RuneLight[] blades_light;
    public bool center_light;
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
    public bool has_wpn;
    public int bull_num;
    public float heat_ratio;
}

public struct BatSync {
    public int money_red;
    public int money_blue;
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
    public BatSync bat_sync;

    public UISync(SyncList<RoboSync> roboSyncs, BaseSync bs_r, BaseSync bs_b, OutpostSync os_r, 
        OutpostSync os_b, BatSync bat_sync) {
        this.robots = roboSyncs;
        this.bs_r = bs_r;
        this.bs_b = bs_b;
        this.os_r = os_r;
        this.os_b = os_b;
        this.bat_sync = bat_sync;
    }
}

public class SyncNode : NetworkBehaviour {
    [SyncVar] private bool ready_push = false;

    [SyncVar] private BatSync bat_sync; 
   
    /* rune */
    [SyncVar] private Vector3 rune_rot = new Vector3();
    [SyncVar] private RuneSync rune_sync_red = new RuneSync();
    [SyncVar] private RuneSync rune_sync_blue = new RuneSync();

    /* outpost */
    [SyncVar] private OutpostSync otpt_sync_red = new OutpostSync();
    [SyncVar] private OutpostSync otpt_sync_blue = new OutpostSync();

    /* base */
    [SyncVar] private BaseSync base_sync_red = new BaseSync();
    [SyncVar] private BaseSync base_sync_blue = new BaseSync();

    /* robots */
    /* Note: SyncList can and only can be modify in Server */
    private readonly SyncList<RoboSync> robo_sync_all = new SyncList<RoboSync>();


    /****************** alias ****************/
    Rune rune;
    OutpostState outpost_red;
    OutpostState outpost_blue;
    BaseState base_red;
    BaseState base_blue;
    /* when sync, robo_red has no difference with robo_blue */
    List<RoboState> robo_all;

    void Start() {
        /* battlefield is initialized in Awake period, which justify following assignment */
        rune = BattleField.singleton.rune;
        outpost_blue = BattleField.singleton.outpost_blue;
        outpost_red = BattleField.singleton.outpost_red;
        base_red = BattleField.singleton.base_red;
        base_blue = BattleField.singleton.base_blue;
        robo_all = BattleField.singleton.robo_all;
        if (isServer)
            for (int i = 0; i < robo_all.Count; i++)
                robo_sync_all.Add(new RoboSync());
    }

    /* use LateUpdate() to ensure users see these */
    void LateUpdate() {
        if (isServer) {
            bat_sync = BattleField.singleton.Pull();
            /* server pushes rune appearence to sync info */
            rune_rot = rune.rotator_rune.localEulerAngles;
            rune_sync_red = rune.rune_state_red.Pull();
            rune_sync_blue = rune.rune_state_blue.Pull();
            /* server pushes outpost appearence to sync info */
            otpt_sync_red = outpost_red.Pull();
            otpt_sync_blue = outpost_blue.Pull();
            /* server pushes base appearence to sync info */
            base_sync_red = base_red.Pull();
            base_sync_blue = base_blue.Pull();
            /* server pushes robots appearence to sync info */
            for (int i = 0; i < robo_sync_all.Count; i++) {
                robo_sync_all[i] = robo_all[i].Pull();
            }
            ready_push = true;
        }
 
        // Note: When battlefield is first loaded, push() may be called in client PC earlier 
        //       than pull() in server PC
        //       which will raise error of null reference in client PC
        if (isClient && ready_push) {
            /* client pulls rune appearence from sync info */
            rune.rotator_rune.localEulerAngles = rune_rot;
            rune.rune_state_red.Push(rune_sync_red);
            rune.rune_state_blue.Push(rune_sync_blue);
            /* client pulls outpost appearence from sync info */
            outpost_red.Push(otpt_sync_red);
            outpost_blue.Push(otpt_sync_blue);
            /* client pulls base appearence from sync info */
            base_red.Push(base_sync_red);
            base_blue.Push(base_sync_blue);
            /* client pulls robots appearence from sync info */
            for (int i = 0; i < robo_all.Count; i++) {
                robo_all[i].Push(robo_sync_all[i]);
            }
            /* host needs that to update UI as well */
            BattleField.singleton.bat_ui.Push(new UISync(robo_sync_all, base_sync_red, base_sync_blue, 
                otpt_sync_red, otpt_sync_blue, bat_sync));
        }
   }
}
