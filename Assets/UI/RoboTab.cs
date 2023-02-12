using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// for robot, "Defensive" means B_dfc > 0; for base, "Defensive" means shield > 0;
// for outpost, it is illegal
namespace RMUC_UI {
    public enum BatStat { Dead = 0, Survival = 1, Defensive = 2, Invulnerable = 3 };

    public class RoboTab : MonoBehaviour {
        public BloodBar bld_bar;
        public Image img_lv;
        public Image img_ava;
        public TMP_Text txt_bullnum;

        public Sprite[] bld_masks;
        public Sprite[] imgs_lv;
        public Sprite[] imgs_lv_dead;
        [Header("dead-survival-defensive-invulnerable")]
        public Sprite[] imgs_ava;
        /* for my robotab */
        [Header("hero engi infa infa")]
        public Sprite[] imgs_team;
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


        public void Push(RoboSync robo_sync) {
            // img_ava
            if (this.bat_stat != robo_sync.bat_stat && img_ava != null && imgs_ava.Length > (int)robo_sync.bat_stat)
                img_ava.sprite = imgs_ava[(int)robo_sync.bat_stat];
            // blood bar
            if (robo_sync.has_blood && this.bld_bar != null) {
                // blood top
                if (this.currblood != robo_sync.currblood || this.maxblood != robo_sync.maxblood) {
                    bld_bar.SetBlood(((float)robo_sync.currblood) / robo_sync.maxblood);
                }
                // blood mask (how many grid in robotab)
                if (this.maxblood != robo_sync.maxblood && bld_masks.Length > 0) {
                    int idx = MaxbldToIdx(robo_sync.maxblood);
                    if (bld_masks.Length > idx)
                        bld_bar.SetMask(bld_masks[idx]);
                    else
                        Debug.Log("No alternative mask");
                }
            }
            // img_lv
            if (robo_sync.has_level && img_lv != null) {
                if (this.bat_stat != robo_sync.bat_stat || this.level != robo_sync.level) {
                    bool surv = robo_sync.bat_stat != BatStat.Dead;
                    if (surv && imgs_lv.Length > robo_sync.level)
                        this.img_lv.sprite = imgs_lv[robo_sync.level];
                    else if (!surv && imgs_lv_dead.Length > robo_sync.level)
                        this.img_lv.sprite = imgs_lv_dead[robo_sync.level];
                }
            }
            // txt_bullnum
            if (robo_sync.has_wpn && txt_bullnum != null) {
                if (this.bull_num != robo_sync.bull_num)
                    if (robo_sync.bull_num == 0)
                        txt_bullnum.text = "<color=#FF0707>0</color>";
                    else
                        txt_bullnum.text = robo_sync.bull_num.ToString();
            }
            // refresh
            this.maxblood = robo_sync.maxblood;
            this.currblood = robo_sync.currblood;
            this.bat_stat = robo_sync.bat_stat;
            this.level = robo_sync.level;
            this.bull_num = robo_sync.bull_num;
        }


        public void Push(OutpostSync otpt_sync) {
            /* no need to set survival, because it won't make difference to outpost UI */
            if (otpt_sync.currblood != this.currblood) {
                bld_bar.SetBlood(otpt_sync.currblood / 1500f);
                this.currblood = otpt_sync.currblood;
            }
            bld_bar.SetInvulState(otpt_sync.invul);
        }


        int MaxbldToIdx(int maxblood) {
            return maxblood <= 500 ? maxblood / 50 - 1 : 10;
        }

    }

}