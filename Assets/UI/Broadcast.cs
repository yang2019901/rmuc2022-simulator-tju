using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace RMUC_UI {
    /* broadcast kill message */
    public class Broadcast : MonoBehaviour {
        public Image img_killer;
        public Image img_killer_idx;
        public TMP_Text txt_killer_idx;
        public Image img_killee;
        public Image img_killee_idx;
        public TMP_Text txt_killee_idx;
        public Image img_bg;

        public Sprite[] spr_ava_red;
        public Sprite[] spr_ava_dead_red;

        public Sprite[] spr_ava_blue;
        public Sprite[] spr_ava_dead_blue;

        public Sprite[] spr_bg;
        public Sprite[] spr_idx_bg;


        Animator anim;
        List<BasicState> killers = new List<BasicState>();
        List<BasicState> killees = new List<BasicState>();
        void Awake() {
            anim = GetComponent<Animator>();
        }


        List<BasicState> team_red = new List<BasicState>();     // robots and towers with red armor
        List<BasicState> team_blue = new List<BasicState>();    // robots and towers with blue armor
        void Start() {
            team_red.AddRange(BattleField.singleton.robo_red);
            team_red.Add(BattleField.singleton.guard_red);
            team_red.Add(BattleField.singleton.outpost_red);
            team_blue.AddRange(BattleField.singleton.robo_blue);
            team_blue.Add(BattleField.singleton.guard_blue);
            team_blue.Add(BattleField.singleton.outpost_blue);
        }


        bool broadcasting = false;
        void Update() {
            // // for test
            // if (Input.GetKeyDown(KeyCode.T))
            //     EnqueueKill(BattleField.singleton.robo_blue[1].gameObject, BattleField.singleton.outpost_red.gameObject);
            
            if (!broadcasting) {
                BrdcstKill();
            }
        }


        public void EnqueueKill(GameObject hitter, GameObject hittee) {
            BasicState killer = hitter.GetComponent<BasicState>();
            BasicState killee = hittee.GetComponent<BasicState>();
            if (killer == null || killee == null)
                return;
            killers.Add(killer);
            killees.Add(killee);
            // Debug.Log("current mes in queue: " + killers.Count);
        }


        /* choose the right image and index from alternatives */
        void SetMesInfo(BasicState bs, bool killed, Image ava, Image idx_bg, TMP_Text idx) {
            if (bs.armor_color == ArmorColor.Red) {
                int tmp = team_red.FindIndex(i => i == bs);
                ava.sprite = killed ? spr_ava_dead_red[tmp] : spr_ava_red[tmp];
                idx_bg.sprite = spr_idx_bg[0];
            } else {
                int tmp = team_blue.FindIndex(i => i == bs);
                ava.sprite = killed ? spr_ava_dead_blue[tmp] : spr_ava_blue[tmp];
                idx_bg.sprite = spr_idx_bg[1];
            }
            // guard has no index
            if (bs.GetComponent<RoboState>() != null && bs.GetComponent<GuardState>() == null) {
                idx_bg.gameObject.SetActive(true);
                idx.gameObject.SetActive(true);
                idx.text = bs.name.Split('_')[2];
            } else {
                idx_bg.gameObject.SetActive(false);
                idx.gameObject.SetActive(false);
            }
        }


        void OnFinishBrdcst() {
            broadcasting = false;
        }


        /* animator event callback, judge if there's kill message not broadcast and broadcast it if any */
        void BrdcstKill() {
            if (killers.Count == 0)
                return;

            BasicState killer = killers[0];
            BasicState killee = killees[0];
            killers.RemoveAt(0);
            killees.RemoveAt(0);

            img_bg.sprite = killer.armor_color == ArmorColor.Red ? spr_bg[0] : spr_bg[1];
            SetMesInfo(killer, false, img_killer, img_killer_idx, txt_killer_idx);
            SetMesInfo(killee, true, img_killee, img_killee_idx, txt_killee_idx);

            anim.SetTrigger("Start");
            broadcasting = true;
        }

    }
}
