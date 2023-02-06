using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RMUC_UI {
    public class HeatRing : MonoBehaviour {
        [Header("heat ring: white-yellow-red")]
        public Sprite[] hr;

        Image img_hr;

        void Start() {
            img_hr = GetComponent<Image>();
        }

        public void SetHeat(float ratio) {
            ratio = Mathf.Clamp(ratio, 0, 1);
            if (ratio >= 0 && ratio <= 1f / 3)
                img_hr.sprite = hr[0];
            else if (ratio <= 2f / 3)
                img_hr.sprite = hr[1];
            else if (ratio <= 1f)
                img_hr.sprite = hr[2];
            img_hr.fillAmount = ratio;
        }
    }
}
