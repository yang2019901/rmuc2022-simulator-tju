using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// for robot, "Defensive" means B_dfc > 0; for base, "Defensive" means shield > 0;
// for outpost, it is illegal
public enum BatStat { Dead = 0, Survival = 1, Defensive = 2, Invulnerable = 3 };
namespace RMUC_UI {

    public class AvaBatStat : MonoBehaviour {
        public BloodBar bld_bar;
        public Image img_lv;
        public Image img_ava;
        public TMP_Text txt_bullnum;
        
        public Sprite[] bld_masks;
        public Sprite[] imgs_lv;
        public Sprite[] imgs_lv_dead;
        [Header("dead-survival-defensive-invulnerable")]
        public Sprite[] imgs_ava;

        BatStat bat_stat;
        int currblood = -1;
        int maxblood = -1;
        int level = -1;
        int bull_num = -1;

        void Start() {
            if (img_lv != null)
                img_lv.gameObject.SetActive(true);
            if (img_ava != null)
                img_ava.gameObject.SetActive(true);
            if (txt_bullnum != null)
                txt_bullnum.gameObject.SetActive(true);
            if (bld_bar != null) {
                bld_bar.gameObject.SetActive(true);
                bld_bar.Reset();
            }
        }


        public void Push(OutpostSync otpt_sync) {
            /* no need to set survival, because it won't make difference to outpost UI */
            if (otpt_sync.currblood != this.currblood) {
                bld_bar.SetBlood(otpt_sync.currblood / 1500f);
                this.currblood = otpt_sync.currblood;
                Debug.Log("outpost currblood: " + this.currblood);
            }
            bld_bar.SetInvulState(otpt_sync.invul);
        }

        public void Push(RoboSync robo_sync) {
            if (this.bat_stat != robo_sync.bat_stat)
                img_ava.sprite = imgs_ava[(int)robo_sync.bat_stat];
            if (robo_sync.has_blood && this.bld_bar != null) {
                if (this.currblood != robo_sync.currblood) {
                    bld_bar.SetBlood(((float)robo_sync.currblood) / robo_sync.maxblood);
                }
                if (this.maxblood != robo_sync.maxblood) {
                    int idx = MaxbldToIdx(robo_sync.maxblood);
                    if (bld_masks.Length > idx)
                        bld_bar.SetMask(bld_masks[idx]);
                    else
                        Debug.Log("No alternative mask");
                }
            }
            if (robo_sync.has_level && img_lv != null) {
                if (this.bat_stat != robo_sync.bat_stat || this.level != robo_sync.level) {
                    this.img_lv.sprite = robo_sync.bat_stat != BatStat.Dead ? imgs_lv[robo_sync.level] 
                        : imgs_lv_dead[robo_sync.level];
                }
            }
            if (robo_sync.has_bull && this.bull_num != robo_sync.bull_num) {
                if (robo_sync.bull_num == 0)
                    txt_bullnum.text = "<color=#FF0707>0</color>";
                else
                    txt_bullnum.text = robo_sync.bull_num.ToString();
            }
            this.maxblood = robo_sync.maxblood;
            this.currblood = robo_sync.currblood;
            this.bat_stat = robo_sync.bat_stat;
            this.level = robo_sync.level;
            this.bull_num = robo_sync.bull_num;
        }
        int MaxbldToIdx(int maxblood) {
            return maxblood <= 500 ? maxblood/50 - 1 : 10; 
        }

    }

}