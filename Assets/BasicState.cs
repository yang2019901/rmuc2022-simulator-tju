using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicState : MonoBehaviour {
    public abstract void TakeDamage(GameObject hitter, GameObject armor_hit, GameObject bullet);
    public ArmorColor armor_color;
}


public abstract class RobotState : BasicState {
    public bool survival = true;  // whether this robot survives
    /* weapon params */
    public int heat_limit;
    public int cool_down;
    public int shoot_speed;
    /* chassis params */
    public int power;
    public int blood;
    public int blood_left;

    /* Buff */
    /* damage = damage * (1 + hitter.B_atk) * max(1-hittee.B_dfc, 0) */
    public float B_atk = 0; // attack buff. Ex: rune_junior => B_atk += 0.5; rune_senior => B_atd += 1
    public float B_dfc = 0; // defence buff. Ex: rune_senior => B_dfc += 0.5
    /* heat -= cool_down * B_cd */
    public float B_cd = 0;
    public float B_pow = 0;
}
