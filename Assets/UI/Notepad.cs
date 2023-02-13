using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RMUC_UI {
    public class Notepad : MonoBehaviour {
        public Image score_red;
        public Image score_blue;
        public Image round;
        public TMP_Text battle_time;
        public Sprite[] digits_left;
        public Sprite[] digits_right;
        public Sprite[] tags_round;
        public TMP_Text money_red;
        public TMP_Text money_blue;
        [Header("gold-silver")]
        public Sprite[] imgs_mine;
        public GameObject xchgmine;


        public void DispScores(int score_red, int score_blue) {
            this.score_red.sprite = digits_left[score_red];
            this.score_blue.sprite = digits_right[score_blue];
            this.round.sprite = tags_round[score_blue + score_red];
        }


        /* 122s => 2:02 */
        public void DispTime(float t) {
            if (t < 0) {
                battle_time.text = "<size=14>比赛结束</size>";
                return;
            }
            int time = Mathf.FloorToInt(t);
            int minute = time / 60;
            int second = time % 60;
            battle_time.text = string.Format("{0:D1}:{1:D2}", minute, second);
        }


        public void DispXchgMine(ArmorColor armor_color, bool is_gold) {
            GameObject tmp = GameObject.Instantiate(this.xchgmine);
            Image img = tmp.GetComponentInChildren<Image>();
            TMP_Text txt = tmp.GetComponentInChildren<TMP_Text>();
            string str, clr;
            if (is_gold) {
                str = "+300";
                clr = "#ffdf00";
                img.sprite = imgs_mine[0];
            } else {
                str = "+100";
                clr = "#ffffff";
                img.sprite = imgs_mine[1];
            }
            txt.text = string.Format("<color={0}>{1}</color>", clr, str);
            tmp.transform.SetParent(this.transform);
            tmp.transform.localScale = 0.8f * Vector3.one;
            tmp.transform.localPosition = armor_color == ArmorColor.Red ? new Vector3(-80, -173, 0) 
                : new Vector3(100, -173, 0);
            StartCoroutine(AnimXchg(tmp));

            DispMoney();
        }


        public void DispMoney() {
            this.money_red.text = string.Format("<size=12>{0}</size><size=10>/{1}</size>",
                BattleField.singleton.money_red, BattleField.singleton.money_red_max);
            this.money_blue.text = string.Format("<size=12>{0}</size><size=10>/{1}</size>",
                BattleField.singleton.money_blue, BattleField.singleton.money_blue_max);
        }


        // blink
        IEnumerator AnimXchg(GameObject xchgmine) {
            for (int i = 0; i < 3; i++) {
                xchgmine.SetActive(true);
                yield return new WaitForSeconds(0.25f);
                xchgmine.SetActive(false);
                yield return new WaitForSeconds(0.15f);
            }
            Destroy(xchgmine);
            yield break;
        }
    }
}
