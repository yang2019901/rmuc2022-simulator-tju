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

    void Update()
    {
        
    }
}
