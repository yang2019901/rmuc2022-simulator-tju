using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BasicController : NetworkBehaviour {
    protected float sum = 0;
    protected float last_err = 0;
    protected float Kp = 1f;
    protected float Ki = 0f;
    protected float Kd = 0f;
    /* use PID controller (Kp > 0) to calc MV */
    protected float PID(float err) {
        sum += err;
        float d_err = err - last_err;
        last_err = err;
        float output = Kp * (err + Ki * sum + Kd * d_err);
        // Debug.Log("pid output: " + output);
        return output;
    }
}
