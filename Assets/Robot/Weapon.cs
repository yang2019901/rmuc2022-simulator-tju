using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Caliber {_17mm, _42mm}

public class Weapon : MonoBehaviour {
    /* turret params */
    public int bullspd => robot.bullspd;
    public int cooldown => robot.cooldown;
    public int maxheat => robot.maxheat;
    public int bull_num;
    public int currheat;

    RoboState robot;
    Caliber caliber;    // 17mm or 42mm

    void Start() {
        if (this.name.ToLower().Contains("infantry"))
            caliber = Caliber._17mm;
        else if (this.name.ToLower().Contains("hero"))
            caliber = Caliber._42mm;
        else
            Debug.LogError("wrong car name receive by Weapon.cs: " + this.name);

        this.robot = GetComponent<RoboState>();
        this.bull_num = 0;
        Reset();
    }


    public void Reset() {
        this.currheat = 0;
        return;
    }


    float timer;
    void Update() {
        if (!robot.survival) {
            Reset();
            return;
        }
        int Q1 = this.currheat;
        int Q0 = this.maxheat;
        if (Q1 > 2*Q0) {
            robot.currblood -= Mathf.RoundToInt((Q1 - 2*Q0) / 250f * robot.maxblood);
            this.currheat = 2 * Q0;
            Q1 = this.currheat;
        }

        if (Time.time - timer > 0.1f) {
            timer = Time.time;
            if (Q1 <= 2*Q0 && Q1 > Q0)
                robot.currblood -= Mathf.RoundToInt((Q1-Q0) / 250f / 10f * robot.maxblood);
            this.currheat -= this.cooldown;
            if (this.currheat < 0)
                this.currheat = 0;
        }
        if (robot.currblood < 0) {
            robot.currblood = 0;
            robot.survival = false;
        }
    }

    public GameObject GetBullet() {
        if (bull_num > 0) {
            bull_num--;
            if (caliber == Caliber._17mm) {
                // small bullet
                this.currheat += 10;
                return BulletPool.singleton.GetSmallBullet();
            }
            else {
                // big bullet
                this.currheat += 100;
                return BulletPool.singleton.GetBigBullet();
            }
        }
        else return null;
    }
}
