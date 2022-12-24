using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace RMUC_UI {
    public class Minimap : MonoBehaviour {
        public const int length = 860;
        public const int width = 440;

        [Header("prefab of roboicon")]
        public GameObject robo_icon;
        [Header("parent of four image objects")]
        public GameObject rune;
        [Header("red-blue")]
        public Image[] imgs_otpt;
        public Image[] imgs_base;


        bool otpt_r, otpt_b;
        public void SetOtpt() {
            if (BattleField.singleton.outpost_red.survival ^ otpt_r)
                imgs_otpt[0].gameObject.SetActive(BattleField.singleton.outpost_red.survival);
            if (BattleField.singleton.outpost_blue.survival ^ otpt_b)
                imgs_otpt[1].gameObject.SetActive(BattleField.singleton.outpost_blue.survival);
            otpt_r = BattleField.singleton.outpost_red.survival;
            otpt_b = BattleField.singleton.outpost_blue.survival;
        }


        bool base_r, base_b;
        public void SetBase() {
            if (BattleField.singleton.base_red.survival ^ base_r)
                imgs_base[0].gameObject.SetActive(BattleField.singleton.base_red.survival);
            if (BattleField.singleton.base_blue.survival ^ base_b)
                imgs_base[1].gameObject.SetActive(BattleField.singleton.base_blue.survival);
            base_r = BattleField.singleton.base_red.survival;
            base_b = BattleField.singleton.base_blue.survival;
        }


        Activation last_activ;
        Image[] imgs_rune;
        Activation activ => BattleField.singleton.rune.activ;
        ArmorColor rune_color => BattleField.singleton.rune.rune_color;
        public void SetRune() {
            if (imgs_rune == null) {
                imgs_rune = new Image[4];
                foreach (Image child in rune.GetComponentsInChildren<Image>(includeInactive: true)) {
                    if (child.name.Contains("idle")) {
                        imgs_rune[0] = child;
                        child.gameObject.SetActive(true);
                    } else {
                        if (child.name.Contains("ready"))
                            imgs_rune[1] = child;
                        else if (child.name.Contains("activ"))
                            if (child.name.Contains("red"))
                                imgs_rune[2] = child;
                            else
                                imgs_rune[3] = child;
                        child.gameObject.SetActive(false);
                    }
                }
            }

            switch (activ) {
                case Activation.Idle:
                    if (last_activ == activ)
                        break;
                    foreach (Image tmp in imgs_rune)
                        tmp.gameObject.SetActive(false);
                    imgs_rune[0].gameObject.SetActive(true);
                    break;
                case Activation.Ready: // same as Activation.Hitting
                case Activation.Hitting:
                    if (last_activ != activ) {
                        foreach (Image tmp in imgs_rune)
                            tmp.gameObject.SetActive(false);
                        imgs_rune[1].gameObject.SetActive(true);
                    }
                    imgs_rune[1].rectTransform.Rotate(Vector3.forward, Time.deltaTime * 60); // spin by 60 deg/s
                    break;
                case Activation.Activated:
                    if (last_activ == activ) // according to rule, armor_color won't change suddenly
                        break;
                    foreach (Image tmp in imgs_rune)
                        tmp.gameObject.SetActive(false);
                    imgs_rune[2 + (int)rune_color].gameObject.SetActive(true);
                    break;
            }
            last_activ = activ;
        }


        public void SetGuard(ArmorColor armor_color, bool survival) {

        }


        public void SetDrone(ArmorColor armor_color, bool survival) {

        }


        List<RoboIcon> rbicns = new List<RoboIcon>();
        public void SetRoboIcons() {
            if (BattleField.singleton.robo_local == null)
                return ;
            foreach (RoboIcon ri in rbicns) {
                if (ri.armor_color != BattleField.singleton.robo_local.armor_color)
                    ri.gameObject.SetActive(GameSetting.singleton.show_enemy);
            }
        }


        void Update() {
            SetRune();
            SetBase();
            SetOtpt();
            SetRoboIcons();
        }


        void Start() {
            foreach (RoboState rs in BattleField.singleton.robo_all) {
                if (rs.name.Contains("drone"))
                    continue;
                GameObject tmp = GameObject.Instantiate(this.robo_icon);
                tmp.transform.SetParent(this.transform);
                tmp.transform.localScale = 0.6f * Vector3.one;
                RoboIcon ri = tmp.GetComponent<RoboIcon>();
                ri.Reset(rs);
                rbicns.Add(ri);
            }
        }

    }
}