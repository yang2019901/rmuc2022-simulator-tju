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

        public void SetScores(int score_red, int score_blue) {
            this.score_red.sprite = digits_left[score_red];
            this.score_blue.sprite = digits_right[score_blue];
            this.round.sprite = tags_round[score_blue + score_red];
        }

        /* 122s => 2:02 */
        public void SetTime(float t) {
            int time = Mathf.FloorToInt(t);
            int minute = time / 60;
            int second = time % 60;
            battle_time.text = string.Format("{0:D1}:{0:D2}", minute, second);
        }

        public void SetMoney(int red_money, int red_money_all, int blue_money, int blue_money_all) {
            this.money_red.text = string.Format("<size=12>{}</size><size=10>/{}</size>", red_money, red_money_all);
            this.money_blue.text = string.Format("<size=12>{}</size><size=10>/{}</size>", blue_money, blue_money_all);
        }

        // /* for test */
        // void Start() {
        //     SetScores(0, 2);
        //     SetTime(122);
        // }
    }
}
