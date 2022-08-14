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
    /* set blood bar and armor blinking */
    public int blood_left;
}

public struct BaseSync {
    public bool survival;
    /* set blood bar and armor blinking */
    public int blood_left;
}

public struct RobotSync {
    /* whether user input works */
    public bool survival;
    /* set blood bar and armor blinking */
    public int blood_left;
}

public class DataNode : NetworkBehaviour {
    [SyncVar]
    public Vector3 rune_rot = new Vector3();
    [SyncVar]
    public RuneSync rune_sync_red = new RuneSync();
    [SyncVar]
    public RuneSync rune_sync_blue = new RuneSync();

    [SyncVar]
    public OutpostSync otpt_sync_red = new OutpostSync();
    [SyncVar]
    public OutpostSync otpt_sync_blue = new OutpostSync();

    [SyncVar]
    public BaseSync base_sync_red = new BaseSync();
    [SyncVar]
    public BaseSync base_sync_blue = new BaseSync();

    public readonly SyncList<RobotSync> robo_sync_red = new SyncList<RobotSync>();
    public readonly SyncList<RobotSync> robo_sync_blue = new SyncList<RobotSync>();

    /* alias */
    Rune rune;
    OutpostState outpost_red;
    OutpostState outpost_blue;
    BaseState base_red;
    BaseState base_blue;
    RobotState[] robo_red;
    RobotState[] robo_blue;

    void Start() {
        /* battlefield is initialized in Awake period, which justify following assignment */
        rune = BattleField.singleton.rune;
        outpost_blue = BattleField.singleton.outpost_blue;
        outpost_red = BattleField.singleton.outpost_red;
        base_red = BattleField.singleton.base_red;
        base_blue = BattleField.singleton.base_blue;
        robo_red = BattleField.singleton.robo_red;
        robo_blue = BattleField.singleton.robo_blue;
        for (int i = 0; i < robo_red.Length; i++) {
            robo_sync_red.Add(new RobotSync());
            robo_sync_blue.Add(new RobotSync());
        }
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
            for (int i = 0; i < robo_sync_red.Count; i++) {
                robo_sync_red[i] = robo_red[i].Pull();
                robo_sync_blue[i] = robo_blue[i].Pull();
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
            for (int i = 0; i < robo_sync_red.Count; i++) {
                robo_red[i].Push(robo_sync_red[i]);
                robo_blue[i].Push(robo_sync_blue[i]);
            }
        }
   }
}
