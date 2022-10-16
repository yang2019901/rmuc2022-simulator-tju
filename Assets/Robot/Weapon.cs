using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum Caliber { _17mm, _42mm }

public class Weapon : MonoBehaviour {
    /* turret params */
    public int bullspd => robot.bullspd;
    public float cooldown => robot.cooldown * robot.B_cd;
    public int maxheat => robot.maxheat;
    public int bull_num;
    public int currheat;

    public float heat_ratio = 0;
    RoboState robot;
    Caliber caliber;    // 17mm or 42mm

    /// <summary>
    /// API
    /// </summary>
    public void ResetHeat() {
        this.currheat = 0;
        return;
    }


    public GameObject GetBullet() {
        if (bull_num > 0) {
            bull_num--;
            GainHeat();
            if (this.caliber == Caliber._17mm) {
                AssetManager.singleton.PlayClipAtPoint(AssetManager.singleton._17mm, transform.position);
                return BulletPool.singleton.GetSmallBullet();
            } else {
                AssetManager.singleton.PlayClipAtPoint(AssetManager.singleton._42mm, transform.position);
                return BulletPool.singleton.GetBigBullet();
            }
        } else return null;
    }




    /// <summary>
    /// non-API
    /// </summary>
    void Start() {
        if (this.name.ToLower().Contains("infantry"))
            caliber = Caliber._17mm;
        else if (this.name.ToLower().Contains("hero"))
            caliber = Caliber._42mm;
        else
            Debug.LogError("wrong car name receive by Weapon.cs: " + this.name);

        this.robot = GetComponent<RoboState>();
        this.bull_num = 0;
        ResetHeat();
    }


    float timer;
    void Update() {
        if (!NetworkServer.active)
            return;
        if (!robot.survival)
            ResetHeat();
        else {
            CalcHeat();
            if (robot.currblood < 0)
                robot.Die();
        }
    }


    int Q1, Q0;
    void CalcHeat() {
        Q1 = this.currheat;
        Q0 = this.maxheat;
        if (Q1 > 2 * Q0) {
            robot.currblood -= Mathf.RoundToInt((Q1 - 2 * Q0) / 250f * robot.maxblood);
            robot.SetBloodBars();
            this.currheat = 2 * Q0;
            Q1 = this.currheat;
        }

        if (Time.time - timer > 0.1f) {
            timer = Time.time;
            if (Q1 <= 2 * Q0 && Q1 > Q0) {
                robot.currblood -= Mathf.RoundToInt((Q1 - Q0) / 250f / 10f * robot.maxblood);
                robot.SetBloodBars();
            }
            this.currheat -= Mathf.RoundToInt(this.cooldown / 10f);
            if (this.currheat < 0)
                this.currheat = 0;
        }
        heat_ratio = currheat / (float)maxheat;
    }


    void GainHeat() {
        this.currheat += this.caliber == Caliber._17mm ? 10 : 100;
        return;
    }

}
