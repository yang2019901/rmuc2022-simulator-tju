using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseState : TowerState {
    public bool buff_active;
    private float last_snipe = -10f;
    public override void Start() {
        base.Start();
        buff_active = true;
    }

    public override void TakeDamage(GameObject hitter, GameObject armor_hit, GameObject bullet) {
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
    }

    public BaseSync Pull() {
        BaseSync tmp = new BaseSync();
        tmp.blood_left = this.blood_left;
        tmp.survival = this.survival;
        return tmp;
    }

    public void Push(BaseSync base_sync) {
        if (this.blood_left > base_sync.blood_left) {
            this.blood_left = base_sync.blood_left;
            this.SetBloodBars();
            StartCoroutine(this.ArmorsBlink(0.1f));
        }
        this.survival = base_sync.survival;
    }

    // Update is called once per frame
    void Update() {

    }
}
