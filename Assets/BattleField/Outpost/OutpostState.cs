using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutpostState : TowerState
{
    public bool buff_active;
    // outpost invulnerable: purple lightbars, stop rotating, no blinking
    public bool invul;

    public override void Start()
    {
        base.Start();
        buff_active = true;
        foreach (ArmorController ac in acs)
            ac.SetLight(AssetManager.singleton.light_purple);
    }

    public void Update() {
        bool tmp = BattleField.singleton.GetBattleTime() < 10;
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

    public OutpostSync Pull() {
        OutpostSync tmp = new OutpostSync();
        tmp.currblood = this.currblood;
        tmp.survival = this.survival;
        tmp.invul = this.invul;
        tmp.rot = GetComponent<Outpost>().armors_outpost.localEulerAngles;
        return tmp;
    }

    public void Push(OutpostSync outpost_sync) {
        if (this.invul)
            foreach (ArmorController ac in acs)
                ac.SetLight(AssetManager.singleton.light_purple);
        else if (this.currblood > outpost_sync.currblood) {
            this.currblood = outpost_sync.currblood;
            this.SetBloodBars();
            StartCoroutine(this.ArmorsBlink(0.1f));
        }
        this.survival = outpost_sync.survival;
        this.GetComponent<Outpost>().armors_outpost.localEulerAngles = outpost_sync.rot;
    }

}
