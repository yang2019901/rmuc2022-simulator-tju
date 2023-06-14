using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutpostState : TowerState {
    public bool buff_active;
    // outpost invulnerable: purple lightbars, stop rotating, no blinking
    public bool invul;

    const float time_invul_off = 0;    // outpost switchs off invulnerable state 30 sec after game starts

    /// <summary>
    /// API
    /// </summary>
    public OutpostSync Pull() {
        OutpostSync tmp = new OutpostSync();
        tmp.currblood = this.currblood;
        tmp.survival = this.survival;
        tmp.invul = this.invul;
        tmp.rot = GetComponent<Outpost>().armors_outpost.localEulerAngles;
        return tmp;
    }


    public void Push(OutpostSync outpost_sync) {
        /* in client PC, Start() will set armors to purple */
        if (this.currblood != outpost_sync.currblood) {
            this.currblood = outpost_sync.currblood;
            this.SetBloodBars();
            if (!outpost_sync.survival)
                foreach (ArmorController ac in acs)
                    ac.Disable();
            // else
            //     StartCoroutine(this.ArmorsBlink(0.1f));
        }

        /* update local survival. Otherwise, negedge of survival won't be detected */
        this.survival = outpost_sync.survival;
        this.GetComponent<Outpost>().armors_outpost.localEulerAngles = outpost_sync.rot;
    }




    /// <summary>
    /// non-API
    /// </summary>
    public override void Start() {
        base.Start();
        buff_active = true;
        this.expval = AssetManager.singleton.exp["outpost"]["have"].ToObject<int>();
        foreach (ArmorController ac in acs)
            ac.SetLight(AssetManager.singleton.light_purple);
    }


    public void Update() {
        bool tmp = BattleField.singleton.GetBattleTime() < time_invul_off;
        if (this.invul && !tmp) {
            foreach (ArmorController ac in acs) {
                ac.SetLight(true);
            }
        }
        this.invul = tmp;
    }


    public override void TakeDamage(GameObject hitter, GameObject armor_hit, GameObject bullet) {
        if (!this.invul)
            base.TakeDamage(hitter, armor_hit, bullet);
    }

}
