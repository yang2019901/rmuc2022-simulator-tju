using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BattleUI : MonoBehaviour
{
    public RMUC_UI.AvaBatStat[] avaBatStats; 
    
    public void Push(SyncList<RoboSync> robo_syncs) {
        for (int i = 0; i < avaBatStats.Length; i++) {
            avaBatStats[i].Push(robo_syncs[i]);
        }
    } 
}
