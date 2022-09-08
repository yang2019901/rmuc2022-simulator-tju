using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RMUC_UI {
    public enum AvaStat { Dead = 0, Survival = 1, Defensive = 2, Invulnerable = 3 };

    public class AvaBatStat : MonoBehaviour {
        public Image level;
        public BloodBar bld_bar;
        public int currblood;
        bool survival;
        public int maxblood;
        public Sprite[] img_lv;
        public Sprite[] img_lv_dead;

        public void SetMaxBlood(int maxblood) {

        }

        // /* set avatar's current blood and automatically set its survival */
        // public void SetCurrBlood() {
        //     bld_bar.SetBlood((float)currblood / maxblood);
        //     this.survival = currblood > 0;
        // }

        public void SetLevel(int lv) {
            if (survival)
                this.level.sprite = img_lv[lv];
            else
                this.level.sprite = img_lv_dead[lv];
        }

        public void SetState(AvaStat ava_stat) {

        }

        public void Push(RoboSync robo_sync) {
            this.currblood = robo_sync.currblood;
            this.survival = robo_sync.survival;
        }
    }

}