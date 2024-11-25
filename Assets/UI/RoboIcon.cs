using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace RMUC_UI {
    public class RoboIcon : MonoBehaviour {
        RoboState robo_conn;
        public GameObject arrow;
        public TMP_Text txt_idx;
        public Image img_bg;
        [Header("red-blue")]
        public Sprite[] spr_bg;
        public Sprite[] spr_arrow;
        public ArmorColor armor_color;

        Image img_arrow;


        public void Reset(RoboState robo_conn) {
            if (robo_conn == null) {
                Destroy(this.gameObject);
                return;
            }
            this.robo_conn = robo_conn;
            SetColor(robo_conn.armor_color);
            string[] kw = robo_conn.name.Split('_');  // keywords contained in robot's name. Ex. infantry_red_4
            if (kw.Length != 3) {
                Destroy(this.gameObject);
                return;
            }
            txt_idx.text = kw[2];
        }


        void Awake() {
            img_arrow = arrow.GetComponentInChildren<Image>();
        }


        void Update() {
            if (this.robo_conn == null || this.robo_conn.rigid == null)
                return;

            SetArrow();
            SetIconPos();
        }

        float x => robo_conn.transform.position.x;
        float z => robo_conn.transform.position.z;
        List<float> X = new List<float>();
        List<float> Z = new List<float>();
        Vector2 vel;
        void FixedUpdate() {
            X.Add(x); Z.Add(z);
            if (X.Count < 5)
                return;
            vel = new Vector2(X[4] - X[0], Z[4] - Z[0]) / (4 * Time.fixedDeltaTime);
            X.RemoveAt(0); Z.RemoveAt(0);
        }


        void SetColor(ArmorColor armor_color) {
            this.armor_color = armor_color;
            int tmp = (int)armor_color;
            img_arrow.sprite = spr_arrow[tmp];
            img_bg.sprite = spr_bg[tmp];
        }


        void SetArrow() {
            if (vel.magnitude < 0.1f) {
                arrow.SetActive(false);
                return;
            } else
                arrow.SetActive(true);
            float ang = Vector2.SignedAngle(Vector2.up, vel);
            arrow.transform.localEulerAngles = new Vector3(0, 0, ang + 180);
        }


        void SetIconPos() {
            // pos_map = (pos_real - origin_real) * map_scale + origin_map
            // here, origin_real and origin_map are both zero
            float pos_x = robo_conn.rigid.transform.position.x / BattleField.length * Minimap.length;
            float pos_y = robo_conn.rigid.transform.position.z / BattleField.width * Minimap.width;
            this.transform.localPosition = new Vector3(-pos_x, -pos_y, 0);
        }
    }
} 