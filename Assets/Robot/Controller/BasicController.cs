using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BasicController : NetworkBehaviour {
    [HideInInspector] public RoboState robo_state;
    [Header("AutoAim params")]
    public float maxDist = 15f;        // max distance of auto aim
    public float dynCoeff = 0.4f;    // a 0~1 number describes how fast turret chases the target; 0: no chasing    1: perfectly chasing

    protected Rigidbody _rigid => robo_state.rigid;


    // project 'from' and 'to' to the plane defined by norm and calc signed angle of their projections
    public static float SignedAngleOnPlane(Vector3 from, Vector3 to, Vector3 norm) {
        Vector3 tmp1 = Vector3.ProjectOnPlane(from, norm);
        Vector3 tmp2 = Vector3.ProjectOnPlane(to, norm);
        return Vector3.SignedAngle(tmp1, tmp2, norm);
    }


    Vector3 vel_targ;
    Vector3 last_pos;
    // Note: MUST called in FixedUpdate(), otherwise, the vel_targ has a random error with inaccuracy of about 30%
    void CalcTargVel() {
        if (target == null)
            return;
        vel_targ = (target.transform.position - last_pos) / Time.fixedDeltaTime;
        if (vel_targ.magnitude > 10)    // too fast => illegal velocity when target switching
            vel_targ = Vector3.zero;
        // Debug.Log("target velocity: " + vel_targ.magnitude);
        last_pos = target.transform.position;
    }


    public virtual void FixedUpdate() {
        CalcTargVel();
    }



    List<ArmorController> target_armors;
    Camera robo_cam => Camera.main;
    ArmorController target;
    protected ArmorController last_target;
    public bool AutoAim(Transform bull_start, bool runeMode) {
        // light cone
        float minang_th = 45;
        float minang = 360;
        if (runeMode) {
            RuneState rs = robo_state.armor_color == ArmorColor.Red ? BattleField.singleton.rune.rune_state_red
                : BattleField.singleton.rune.rune_state_blue;
            if (rs.idx_target == -1)
                return false;
            target_armors = new List<ArmorController> { rs.blades[rs.idx_target].armor };
            minang_th = 60;
        } else
            target_armors = (robo_state.armor_color == ArmorColor.Blue) ^ runeMode ? ArmorController.vis_armors_red : ArmorController.vis_armors_blue;
        foreach (ArmorController ac in target_armors) {
            // judge whether armor's enabled
            if (!ac.en)
                continue;

            // judge whether armor's facing turret
            Vector3 v1 = ac.transform.position - bull_start.position;
            Vector3 v2 = ac.transform.TransformVector(ac.norm_in);
            if (Vector3.Angle(v1, v2) >= 75)
                continue;

            float ang = Vector3.Angle(ac.transform.position - bull_start.position, bull_start.forward);
            if (ang < minang || ac == last_target) {
                RaycastHit hitinfo;
                Ray ray = new Ray(bull_start.position, ac.transform.position - bull_start.position);
                // judge whether armor's under cover, bullet is in "Ignore Raycast" layer
                if (!Physics.Raycast(ray, out hitinfo, maxDist, ~LayerMask.GetMask("Ignore Raycast")))
                    continue;
                // Debug.DrawLine(ac.transform.position, bull_start.position, Color.blue);
                if (hitinfo.collider.gameObject == ac.gameObject) {
                    minang = ang;
                    target = ac;
                    if (ac == last_target)  // preferably aiming at last target
                        break;
                }
            }
        }
        if (minang >= minang_th) {
            // Debug.Log("no target");
            return false;
        }
        // Debug.Log("last_target id: " + last_target.GetInstanceID());
        Vector3 pos = target.transform.position;
        float interval = (pos - bull_start.position).magnitude / robo_state.bullspd;
        if (runeMode)
            pos = BattleField.singleton.rune.PredPos(pos, interval);
        else if (target == last_target) {
            // hit robot and outpost and base
            pos = target.transform.position + (vel_targ - _rigid.velocity) * interval;
            // Debug.Log(this.name + ": " + _rigid.velocity.magnitude);
        }
        last_target = target;
        if (CalcFall(ref pos, bull_start.position))
            AimAt(pos);
        else {
            // Debug.Log("too far to reach");
            return false;
        }

        return true;
    }


    float spd => robo_state.bullspd;
    const float g = 9.8f;
    /** calc gravity effect and correct trajectory
        return false if target is beyond range for given bullet speed and bull_start */
    bool CalcFall(ref Vector3 target, Vector3 bull_start) {
        Vector3 r = target - bull_start;
        float y = Vector3.Dot(r, Vector3.up);
        float d = r.magnitude;
        float A = g * g / 4;
        float B = g * y - spd * spd;
        float C = d * d;
        float delta = B * B - 4 * A * C;
        if (delta < 0)
            return false;
        float t = Mathf.Sqrt((-B - Mathf.Sqrt(delta)) / (2 * A));
        float theta = Mathf.Asin((y + g * t * t / 2) / (spd * t));
        float y_new = Mathf.Sqrt(d * d - y * y) * Mathf.Tan(theta);
        Debug.DrawLine(bull_start, target, Color.yellow);
        target[1] += y_new - y;
        // Debug.DrawLine(bull_start, target, Color.green);
        return true;
    }


    /* implement it to rotate turret to aim at target */
    protected virtual void AimAt(Vector3 target) {
        Debug.Log("if you wanna use auto-aim, implement AimAt(Vector3) first.");
    }
}


public class PIDController {
    float sum = 0;
    float last_err = 0;
    public float Kp = 1f;
    public float Ki = 0f;
    public float Kd = 0f;


    public PIDController(float Kp, float Ki, float Kd) {
        this.Kp = Kp;
        this.Ki = Ki;
        this.Kd = Kd;
    }


    public float PID(float err) {
        sum += err;
        float d_err = err - last_err;
        last_err = err;
        float output = Kp * (err + Ki * sum + Kd * d_err);
        // Debug.Log("pid output: " + output);
        return output;
    }


    // reset pid controller params (and integral as well)
    public void Reset(float Kp, float Ki, float Kd) {
        this.sum = 0;
        this.last_err = 0;
        this.Kp = Kp;
        this.Ki = Ki;
        this.Kd = Kd;
    }
}