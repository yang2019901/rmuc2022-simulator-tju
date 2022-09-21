using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Caliber {_17mm, _42mm}

public class Weapon : MonoBehaviour {
    /* turret params */
    public int bullspd;
    public int cooldown;
    public int maxheat;
    private int heat;
    Caliber caliber;

    void Start() {
        if (this.name.ToLower().Contains("infantry"))
            caliber = Caliber._17mm;
        else if (this.name.ToLower().Contains("hero"))
            caliber = Caliber._42mm;
        else
            Debug.LogError("wrong car name receive by Weapon.cs: " + this.name);
    }

    public GameObject GetBullet() {
        return caliber == Caliber._17mm ? BulletPool.singleton.GetSmallBullet()
            : BulletPool.singleton.GetBigBullet();
        
    }
}
