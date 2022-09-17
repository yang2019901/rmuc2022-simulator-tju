using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outpost : MonoBehaviour
{
    public Transform armors_outpost;
    
    private OutpostState _state;

    void Start()
    {
        _state = GetComponent<OutpostState>();
    }

    void FixedUpdate()
    {
        if (!_state.invul && _state.survival)
        {
            OutpostSpin();
        }
    }

    void OutpostSpin()
    {
        float spd = 120;
        Vector3 d_euler_ang =  new Vector3(0, spd*Time.fixedDeltaTime, 0);
        armors_outpost.localEulerAngles += d_euler_ang;
    }
}
