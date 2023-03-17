using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RMUC_UI {
    public class BloodBar : MonoBehaviour {
        [Header("blood bar")]
        public GameObject mask;
        public GameObject bar_blood;
        public RectTransform bld_full;   // full blood's pixel position
        public RectTransform bld_empty;     // empty blood's pixel position

        public GameObject bar_golden;

        [Header("For Base Only")]
        public GameObject bar_blood_mid;
        public RectTransform bld_mid_full;
        public RectTransform bld_mid_empty;

        [Header("For Base and Guard")]
        public GameObject bar_shield;
        public RectTransform shd_full;   // full shield's pixel position
        public RectTransform shd_empty;     // empty shield's pixel position

        [Header("For Robot")]
        public GameObject bar_green;

        [Header("For Outpost")]
        public GameObject bar_dead;

        [Header("For Display")]
        public TMP_Text blood_txt;

        float bld_ratio = 1;
        float bld_mid_ratio = 1;


        /// <summary>
        /// API
        /// </summary>

        /* set blood bar's length */
        public void SetBlood(float ratio) {
            this.bld_ratio = ratio;
            bar_blood.transform.position = ratio * (bld_full.position) + (1 - ratio) * (bld_empty.position);
            if (Mathf.Approximately(ratio, 0) && bar_dead != null) {
                bar_dead.SetActive(true);
            }
        }
        

        public void SetShield(float ratio) {
            bar_shield.transform.position = ratio * (shd_full.position) + (1 - ratio) * (shd_empty.position);
        }

        /* Reset blood bar's visual effect to game-start state */
        public void Reset() {
            bar_blood.transform.position = bld_full.position;
            if (bar_shield != null)
                bar_shield.transform.position = shd_full.position;
            if (bar_golden != null)
                bar_golden.SetActive(false);
            if (bar_green != null)
                bar_green.SetActive(false);
            if (bar_dead != null)
                bar_dead.SetActive(false);
        }

        /* for robot only */
        public void SetBloodMask(Sprite newmask) {
            Image tmp = this.mask.GetComponent<Image>();
            tmp.sprite = newmask;
            tmp.SetNativeSize();
        }

        /* for base and outpost only:  add a golden cover */
        public void SetInvulState(bool is_on) => bar_golden.SetActive(is_on);

        public void DispBldTxt(int currblood, int maxblood) {
            blood_txt.text = string.Format("<b>{0, 3} / {1,-3}</b>", currblood, maxblood);
        }

        /// <summary>
        /// non-API
        /// </summary>
        void Update() {
            if (bar_blood_mid != null) {
                if (bld_mid_ratio > bld_ratio) {
                    if (bld_mid_ratio > bld_ratio + 0.2f)
                        bld_mid_ratio = bld_ratio + 0.2f;
                    bld_mid_ratio -= 0.1f * Time.deltaTime;
                    bar_blood_mid.transform.position = bld_mid_ratio * (bld_mid_full.position)
                        + (1 - bld_mid_ratio) * (bld_mid_empty.position);
                }
                else {
                    bld_mid_ratio = bld_ratio;
                    bar_blood_mid.transform.position = bld_mid_ratio * (bld_mid_full.position)
                        + (1 - bld_mid_ratio) * (bld_mid_empty.position);
                }
            }
        }
    }
}
