using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XchgSpot : MonoBehaviour {
    public ArmorColor armor_color;
    public GameObject det_light;
    public GameObject coll_light;

    readonly Vector3 detCent = new Vector3(0, 0, 0.02f);            // offset from this.position to center of detection region 
    readonly Vector3 detSize = new Vector3(0.2f, 0.2f, 0.03f);      // detection region's (a box area) extent 
    readonly Vector3 collCent = new Vector3(-0.6f, 0, -0.3f);      // offset from this.position to center of collection region 
    readonly Vector3 collSize = new Vector3(0.8f, 1f, 0.5f);        // detection region's (a box area) extent 


    const string used_s = "used";
    float t_last = -3f;
    void Update() {
        // calling Physics.Overlapxxx is efficient to detect collide
        Collider[] cols = Physics.OverlapBox(this.transform.TransformPoint(detCent), 0.5f * detSize, this.transform.rotation);
        // Debug.DrawRay(this.transform.TransformPoint(detCent), Vector3.up, Color.red);
        foreach (Collider col in cols) {
            if (col.name.Contains(CatchMine.mine_s) && !col.name.Contains(used_s)) {
                col.name += used_s;
                t_last = BattleField.singleton.GetBattleTime();
                Debug.Log("detect a mine");
                StartCoroutine(DetLightBlink());
            }
        }

        cols = Physics.OverlapBox(this.transform.TransformPoint(collCent), 0.5f * collSize, this.transform.rotation);
        // Debug.DrawRay(this.transform.TransformPoint(collCent), Vector3.up, Color.green);
        foreach (Collider col in cols) {
            if (col.name.Contains(CatchMine.mine_s)) {
                if (Time.time - t_last < 3f) {
                    BattleField.singleton.XchgMine(armor_color, col.name.Contains("gold"));
                    Debug.Log("xchg a mine successfully");
                    StartCoroutine(CollLightBlink());
                } else
                    Debug.Log("xchg too late");
                Destroy(col);
            }
        }
    }


    IEnumerator DetLightBlink() {
        for (int i = 0; i < 15; i++) {
            det_light.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            det_light.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }
    }


    IEnumerator CollLightBlink() {
        for (int i = 0; i < 10; i++) {
            coll_light.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            coll_light.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }
    }
}