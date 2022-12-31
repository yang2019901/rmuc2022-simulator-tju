using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Outpost : MonoBehaviour {
    public Transform armors_outpost;

    private OutpostState _state;
    private BaseState enemy_bs;

    void Start() {
        _state = GetComponent<OutpostState>();
        enemy_bs = _state.armor_color == ArmorColor.Red ? BattleField.singleton.base_blue : BattleField.singleton.base_red;
    }

    void FixedUpdate() {
        if (!NetworkServer.active)
            return;
        // By rules, outpost's armors will rotate if and only if 
        //     1. outpost is survival and not invulnerable
        //     2. more than 2min to the end of the game
        //     3. enemy's base is survival
        if (!_state.invul && _state.survival && BattleField.singleton.GetBattleTime() < 300 && enemy_bs.survival) {
            OutpostSpin();
        }
    }

    void OutpostSpin() {
        float spd = 120;
        Vector3 d_euler_ang = new Vector3(0, spd * Time.fixedDeltaTime, 0);
        armors_outpost.localEulerAngles += d_euler_ang;
    }
}
