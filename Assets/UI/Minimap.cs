using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RMUC_UI {
    public class Minimap : MonoBehaviour {
        public const int length = 860;
        public const int width = 440;

        public GameObject robo_icon;

        public void SetOtpt(ArmorColor armor_color, bool survival) {

        }


        public void SetBase(ArmorColor armor_color, bool survival) {

        }


        public void SetRune(Activation activ, ArmorColor armor_color) {

        }


        public void SetGuard(ArmorColor armor_color, bool survival) {

        }


        public void SetDrone(ArmorColor armor_color, bool survival) {

        }


        void Start() {
            foreach (RoboState rs in BattleField.singleton.robo_all) {
                GameObject tmp = GameObject.Instantiate(this.robo_icon);
                tmp.transform.SetParent(this.transform);
                tmp.transform.localScale = 0.6f * Vector3.one;
                tmp.GetComponent<RoboIcon>().Reset(rs.GetComponent<Rigidbody>());
                // Debug.Log("create a roboicon");
            }
        }

    }
}