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
    public bool invincible;
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
    public bool has_bull;
    public int bull_num;
}

public class SyncNode : NetworkBehaviour {
    [SyncVar]
    private Vector3 rune_rot = new Vector3();
    [SyncVar]
    private RuneSync rune_sync_red = new RuneSync();
    [SyncVar]
    private RuneSync rune_sync_blue = new RuneSync();

    [SyncVar]
    private OutpostSync otpt_sync_red = new OutpostSync();
    [SyncVar]
    private OutpostSync otpt_sync_blue = new OutpostSync();

    [SyncVar]
    private BaseSync base_sync_red = new BaseSync();
    [SyncVar] 
    private BaseSync base_sync_blue = new BaseSync();

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
        }
 
        if (isClient) {
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
            BattleField.singleton.bat_ui.Push(robo_sync_all);
        }
   }
}
