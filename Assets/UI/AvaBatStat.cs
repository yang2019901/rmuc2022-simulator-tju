using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RMUC_UI {
    public enum AvaStat { Dead = 0, Survival = 1, Defensive = 2, Invulnerable = 3 };

    public class AvaBatStat : MonoBehaviour {
        public Image img_level;
        public BloodBar bld_bar;
        
        public Sprite[] bld_masks;
        public Sprite[] img_lv;
        public Sprite[] img_lv_dead;

        bool survival;
        int currblood;
        int maxblood;
        int level;

        int MaxbldToIdx(int maxblood) {
            return maxblood <= 500 ? maxblood / 50 - 1 : 10; 
        }

        public void Push(RoboSync robo_sync) {
            if (this.currblood != robo_sync.currblood) {
                bld_bar.SetBlood(((float)robo_sync.currblood) / robo_sync.maxblood);
            }
            if (this.maxblood != robo_sync.maxblood) {
                int idx = MaxbldToIdx(robo_sync.maxblood);
                bld_bar.GetComponent<Image>().sprite = bld_masks[idx];
            }
            if (this.survival != robo_sync.survival || this.level != robo_sync.level) {
                this.img_level.sprite = robo_sync.survival ? img_lv[robo_sync.level] 
                    : img_lv_dead[robo_sync.level];
            }
            this.maxblood = robo_sync.maxblood;
            this.currblood = robo_sync.currblood;
            this.survival = robo_sync.survival;
            this.level = robo_sync.level;
        }
    }

}