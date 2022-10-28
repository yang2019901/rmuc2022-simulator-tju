using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RMUC_UI {
    public class BattleUI : MonoBehaviour {
        public Notepad notepad;
        public RoboTab[] robotabs;

        public BloodBar baseStat_red;
        public BloodBar baseStat_blue;

        public RoboTab otptStat_red;
        public RoboTab otptStat_blue;

        /* My UI */
        public HeatRing hr;
        [HideInInspector] public float ratio = 0;
        public Image overheat_bg;
        public TMP_Text txt_bullspd;
        public TMP_Text txt_bullnum;

        public RMUC_UI.RoboTab my_robotab;
        [HideInInspector] public int my_roboidx = -1;


        public void Push(UISync uisync) {
            SetNotePad(uisync.bat_sync);

            for (int i = 0; i < robotabs.Length; i++) {
                robotabs[i].Push(uisync.robots[i]);
            }
            SetBase(baseStat_red, uisync.bs_r);
            SetBase(baseStat_blue, uisync.bs_b);

            otptStat_red.Push(uisync.os_r);
            otptStat_blue.Push(uisync.os_b);

            if (my_roboidx != -1) {
                SetMyUI(uisync.robots[my_roboidx]);
            }
        }

        void SetNotePad(BatSync bs) {
            notepad.SetTime(7 * 60 - bs.time_bat);
        }

        void SetBase(BloodBar baseStat, BaseSync bs) {
            baseStat.SetInvulState(bs.invul);
            baseStat.SetBlood(bs.currblood / 5000f);
            baseStat.SetShield(bs.shield / 500f);
        }

        // called every frame
        bool init = false;
        void SetMyUI(RoboSync my_robosync) {
            if (!init) {
                string color = BattleField.singleton.robo_all[my_roboidx].armor_color == ArmorColor.Red ? "red" : "blue";
                foreach (RoboTab rt in GetComponentsInChildren<RoboTab>(includeInactive: true))
                    if (rt.name.ToLower().Contains("my")) {
                        Debug.Log("rt.name: " + rt.name);
                        if (rt.name.ToLower().Contains(color)) {
                            rt.gameObject.SetActive(true);
                            my_robotab = rt;
                            Debug.Log("my_roboidx: " + my_roboidx);
                            my_robotab.img_ava.sprite = my_robotab.imgs_team[my_roboidx % 5];
                        } else
                            rt.gameObject.SetActive(false);
                    }
                foreach (TMP_Text txt in my_robotab.GetComponentsInChildren<TMP_Text>())
                    if (txt.gameObject.name.ToLower().Contains("idx")) {
                        txt.text = (my_roboidx % 5 + 1).ToString();
                        Debug.Log("set my robot index");
                    }
                init = true;
            }
            hr.SetHeat(ratio);
            if (ratio > 1)
                overheat_bg.gameObject.SetActive(true);
            else
                overheat_bg.gameObject.SetActive(false);

            if (my_robosync.has_wpn) {
                txt_bullnum.text = my_robosync.bull_num.ToString();
                txt_bullspd.text = BattleField.singleton.robo_all[my_roboidx].bullspd.ToString();
            }
            my_robotab.Push(my_robosync);
            my_robotab.bld_bar.DispBldTxt(my_robosync.currblood, my_robosync.maxblood);
        }
    }
}