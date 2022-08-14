using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutpostState : TowerState
{
    public bool buff_active;
    public override void Start()
    {
        base.Start();
        buff_active = true;
    }

    public OutpostSync Pull() {
        OutpostSync tmp = new OutpostSync();
        tmp.blood_left = this.blood_left;
        tmp.survival = this.survival;
        tmp.rot = GetComponent<Outpost>().armors_outpost.localEulerAngles;
        return tmp;
    }

    public void Push(OutpostSync outpost_sync) {
        if (this.blood_left != outpost_sync.blood_left) {
            this.blood_left = outpost_sync.blood_left;
            this.SetBloodBars();
            StartCoroutine(this.ArmorsBlink(0.1f));
        }
        this.survival = outpost_sync.survival;
        this.GetComponent<Outpost>().armors_outpost.localEulerAngles = outpost_sync.rot;
    }

    void Update()
    {
        
    }
}
