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

        /* Settings UI */
        /* supply UI */
        public GameObject supp_ui;

        /* My UI */
        public HeatRing hr;
        [HideInInspector] public float ratio = 0;
        public Image overheat_bg;
        public TMP_Text txt_bullspd;
        public TMP_Text txt_bullnum;

        private RMUC_UI.RoboTab my_robotab;
        // [HideInInspector] public int my_roboidx = -1;

        RoboState myrobot => BattleField.singleton.robo_local;



        public void Push(UISync uisync) {
            SetNotePad(uisync.bat_sync);

            for (int i = 0; i < robotabs.Length; i++) {
                robotabs[i].Push(uisync.robots[i]);
            }
            SetBase(baseStat_red, uisync.bs_r);
            SetBase(baseStat_blue, uisync.bs_b);

            otptStat_red.Push(uisync.os_r);
            otptStat_blue.Push(uisync.os_b);

            SetMyUI();
        }

        void SetNotePad(BatSync bs) {
            notepad.SetTime(7 * 60 - bs.time_bat);
            // todo: set money and score
        }

        void SetBase(BloodBar baseStat, BaseSync bs) {
            baseStat.SetInvulState(bs.invul);
            baseStat.SetBlood(bs.currblood / 5000f);
            baseStat.SetShield(bs.shield / 500f);
        }

        // called every frame
        bool init = false;
        // void SetMyUI(RoboSync my_robosync) {
        void SetMyUI() {
            if (myrobot == null)
                return;
            if (!init) {
                string color = myrobot.armor_color == ArmorColor.Red ? "red" : "blue";
                // string color = BattleField.singleton.robo_all[my_roboidx].armor_color == ArmorColor.Red ? "red" : "blue";
                foreach (RoboTab rt in GetComponentsInChildren<RoboTab>(includeInactive: true)) {
                    if (rt.name.ToLower().Contains("my")) {
                        // Debug.Log("rt.name: " + rt.name);
                        if (rt.name.ToLower().Contains(color)) {
                            rt.gameObject.SetActive(true);
                            my_robotab = rt;    // store reference

                            // set robotab's img_ava
                            int idx = 0; var tp = myrobot.GetType();
                            if (tp == typeof(HeroState)) idx = 0;
                            else if (tp == typeof(EngineerState)) idx = 1;
                            else if (tp == typeof(InfantryState)) idx = 2;
                            else Debug.Log("wrong type of myrobot: " + tp);
                            my_robotab.img_ava.sprite = my_robotab.imgs_team[idx];
                        } else
                            rt.gameObject.SetActive(false);
                    }
                }
                int my_roboidx = BattleField.singleton.robo_all.FindIndex(i=>i==myrobot);
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

            Weapon weap;
            if (myrobot.TryGetComponent<Weapon>(out weap)) {
                txt_bullnum.text = weap.bullnum.ToString();
                txt_bullspd.text = weap.bullspd.ToString();
            }
            my_robotab.Push(myrobot.Pull());
            my_robotab.bld_bar.DispBldTxt(myrobot.currblood, myrobot.maxblood);
        }


        int bull_num;
        public void SetBullSupply(int num) {
            bull_num = num;
        }

        public void CallBullSupply() {
            if (myrobot == null)
                return;
            RoboController rc = myrobot.GetComponent<RoboController>();
            rc.CmdSupply(rc.gameObject.name, bull_num);
            Debug.Log(rc.gameObject.name + " calls supply: " + bull_num);
        }
    }
}