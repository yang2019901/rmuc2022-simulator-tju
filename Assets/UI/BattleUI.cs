using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RMUC_UI {
    public class BattleUI : MonoBehaviour {
        public Notepad notepad;
        public RoboTab[] avaBatStats;

        public BloodBar baseStat_red;
        public BloodBar baseStat_blue;

        public RoboTab otptStat_red;
        public RoboTab otptStat_blue;

        /* My UI */
        public HeatRing hr;
        [HideInInspector] public float ratio = 0;
        public Image overheat_bg;

        [HideInInspector] public int currblood;
        [HideInInspector] public int maxblood;

        public void Push(UISync uisync) {
            SetNotePad(uisync.bat_sync);

            for (int i = 0; i < avaBatStats.Length; i++) {
                avaBatStats[i].Push(uisync.robots[i]);
            }
            SetBase(baseStat_red, uisync.bs_r);
            SetBase(baseStat_blue, uisync.bs_b);

            otptStat_red.Push(uisync.os_r);
            otptStat_blue.Push(uisync.os_b);

            SetMyUI();
        }

        void SetNotePad(BatSync bs) {
            notepad.SetTime(420 - bs.time_bat);
        }

        void SetBase(BloodBar baseStat, BaseSync bs) {
            baseStat.SetInvulState(bs.invul);
            baseStat.SetBlood(bs.currblood / 5000f);
            baseStat.SetShield(bs.shield / 500f);
        }

        void SetMyUI() {
            hr.SetHeat(ratio);
            if (ratio > 1) {
                overheat_bg.gameObject.SetActive(true);
            } else {
                overheat_bg.gameObject.SetActive(false);
            }

            
        }
    }
}
