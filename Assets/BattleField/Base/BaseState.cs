using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseState : TowerState {
    public bool buff_active;
    public bool invul;  // whether base is invulnerable
    public int shield;
    float last_snipe = -10f;


    public override void Start() {
        base.Start();
        this.buff_active = true;
        this.shield = 0;
        this.invul = true;
        SetInvulLight(true);
    }

    public override void TakeDamage(GameObject hitter, GameObject armor_hit, GameObject bullet) {
        if (this.invul)
            return ;
        if (hitter.name.ToLower().Contains("hero")) {
            HeroState hero = hitter.GetComponent<HeroState>();
            if (hero == null)
                Debug.LogError("error in BaseState.cs: hitter uses 42mm bullet but with no HeroState.cs");

            /* Base gets 100% defence to 42mm bullet after sniping in 10 sec */
            if (Time.time - last_snipe < 10f)
                return ;

            if (hero.sniping) {
                /* temporarily change B_atk */
                hero.B_atk = 1.5f;
                base.TakeDamage(hitter, armor_hit, bullet);
                /* restore B_atk */
                hero.UpdateBuff();
                /* update last snipe time to now */
                last_snipe = Time.time;
                Debug.LogWarning("Extraordinary!!, hero sniping");
            } else 
                base.TakeDamage(hitter, armor_hit, bullet);
        } else
            base.TakeDamage(hitter, armor_hit, bullet);
        if (shield + currblood >= maxblood) {
            shield = shield + currblood - maxblood;
            currblood = maxblood;
        }
    }

    public BaseSync Pull() {
        BaseSync tmp = new BaseSync();
        tmp.currblood = this.currblood;
        tmp.survival = this.survival;
        tmp.shield = this.shield;
        tmp.invul = this.invul;
        return tmp;
    }

    public void Push(BaseSync base_sync) {
        if (this.currblood > base_sync.currblood) {
            this.currblood = base_sync.currblood;
            this.SetBloodBars();
            if (!base_sync.survival)
                foreach (ArmorController ac in acs)
                    ac.Disable();
            else
                StartCoroutine(this.ArmorsBlink(0.1f));
        }
        if (this.invul && !base_sync.invul)
            SetInvulLight(false);
        this.survival = base_sync.survival;
        this.shield = base_sync.shield;
        this.invul = base_sync.invul;
    }

    public void SetInvulLight(bool on) {
        if (on)
            foreach (ArmorController ac in acs)
                ac.SetLight(AssetManager.singleton.light_purple);
        else
            foreach (ArmorController ac in acs)
                ac.SetLight(true);
    } 
}
