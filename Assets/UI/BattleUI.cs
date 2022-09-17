using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BattleUI : MonoBehaviour
{
    public RMUC_UI.AvaBatStat[] avaBatStats;
    public RMUC_UI.BloodBar baseStat_red;
    public RMUC_UI.BloodBar baseStat_blue;
    
    public void Push(UISync uisync) {
        for (int i = 0; i < avaBatStats.Length; i++) {
            avaBatStats[i].Push(uisync.robots[i]);
        }
        baseStat_red.SetInvulState(uisync.basebar_r.invul);
        baseStat_red.SetBlood(uisync.basebar_r.currblood / 5000f);
        baseStat_red.SetShield(uisync.basebar_r.shield / 500f);
        
        baseStat_blue.SetInvulState(uisync.basebar_b.invul);
        baseStat_blue.SetBlood(uisync.basebar_b.currblood / 5000f);
        baseStat_blue.SetShield(uisync.basebar_b.shield / 500f);
    }
}
