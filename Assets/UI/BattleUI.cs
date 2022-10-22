using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour {
    public RMUC_UI.Notepad notepad;
    public RMUC_UI.AvaBatStat[] avaBatStats;

    public RMUC_UI.BloodBar baseStat_red;
    public RMUC_UI.BloodBar baseStat_blue;

    public RMUC_UI.AvaBatStat otptStat_red;
    public RMUC_UI.AvaBatStat otptStat_blue;

    /* My UI */
    public RMUC_UI.HeatRing hr;
    public float ratio = 0;
    public Image overheat_bg;

    public void Push(UISync uisync) {
        SetNotePad(uisync.bat_sync);

        for (int i = 0; i < avaBatStats.Length; i++) {
            avaBatStats[i].Push(uisync.robots[i]);
        }
        SetBase(baseStat_red, uisync.bs_r);
        SetBase(baseStat_blue, uisync.bs_b);

        otptStat_red.Push(uisync.os_r);
        otptStat_blue.Push(uisync.os_b);

        SetMyUI();
    }

    void SetNotePad(BatSync bs) {
        notepad.SetTime(420 - bs.time_bat);
    }

    void SetBase(RMUC_UI.BloodBar baseStat, BaseSync bs) {
        baseStat.SetInvulState(bs.invul);
        baseStat.SetBlood(bs.currblood / 5000f);
        baseStat.SetShield(bs.shield / 500f);
    }

    void SetMyUI() {
        hr.SetHeat(ratio);
        if (ratio > 1) {
            overheat_bg.gameObject.SetActive(true);
        } else {
            overheat_bg.gameObject.SetActive(false);
        }

    }
}
