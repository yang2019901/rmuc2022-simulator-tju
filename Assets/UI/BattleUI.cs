using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BattleUI : MonoBehaviour
{
    public RMUC_UI.AvaBatStat[] avaBatStats;
    public RMUC_UI.BloodBar baseStat_red;
    public RMUC_UI.BloodBar baseStat_blue;

    public RMUC_UI.AvaBatStat otptStat_red;
    public RMUC_UI.AvaBatStat otptStat_blue;
    
    public RMUC_UI.HeatRing hr;
    public float ratio = 0;

    public void Push(UISync uisync) {
        for (int i = 0; i < avaBatStats.Length; i++) {
            avaBatStats[i].Push(uisync.robots[i]);
        }
        baseStat_red.SetInvulState(uisync.bs_r.invul);
        baseStat_red.SetBlood(uisync.bs_r.currblood / 5000f);
        baseStat_red.SetShield(uisync.bs_r.shield / 500f);
        
        baseStat_blue.SetInvulState(uisync.bs_b.invul);
        baseStat_blue.SetBlood(uisync.bs_b.currblood / 5000f);
        baseStat_blue.SetShield(uisync.bs_b.shield / 500f);

        otptStat_red.Push(uisync.os_r);
        otptStat_blue.Push(uisync.os_b);

        hr.SetHeat(ratio);
    }
}
