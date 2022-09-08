using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* engineer:
    1. no limit of chassis power
    2. no level, maxblood is 500
 */
public class EngineerState : RoboState
{
    // Start is called before the first frame update
    public override void Start()
    {
        this.maxblood = 500;
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }
}
