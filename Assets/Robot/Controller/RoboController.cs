using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


/* infantry and hero's controller */
public class RoboController : BasicController {
    [Header("Kinematic")]
    public Vector3 centerOfMass;
    [Header("turret")]
    public Transform yaw;
    public Transform pitch;
    [Header("wheels")]
    public Transform[] wheels;
    [Header("order: FL-FR-BR-BL")]
    public WheelCollider[] wheelColliders;

    [Header("Weapon")]
    public Transform bullet_start;
    [Header("View")]
    public Transform view;

    [HideInInspector] public float currcap = 0;
    const int maxcap = 500;

    [Header("AutoAim params")]
    public float maxDist = 15f;        // max distance of auto aim
    public float dynCoeff = 0.4f;    // a 0~1 number describes how fast turret chases the target; 0: no chasing    1: perfectly chasing

    private float last_fire = 0;
    private float pitch_ang = 0;
    private float pitch_min = -30;
    private float pitch_max = 40;
    private Weapon wpn;
    [HideInInspector] public RoboState robo_state;

    private bool playing => Cursor.lockState == CursorLockMode.Locked;
    private Rigidbody _rigid => robo_state.rigid;

    /// <summary>
    /// non-API
    /// </summary>
    public override void OnStartClient() {
        base.OnStartClient();
        if (hasAuthority) {
            Transform tmp = Camera.main.transform;
            tmp.parent = view;
            tmp.localEulerAngles = Vector3.zero;
            tmp.localPosition = Vector3.zero;
        }
    }


    public override void OnStopClient() {
        base.OnStopClient();
        if (hasAuthority) {
            Camera.main.transform.parent = null;
            Cursor.lockState = CursorLockMode.None;
        }
    }


    Transform virt_yaw;
    void Awake() {
        robo_state = GetComponent<RoboState>();
        wpn = GetComponent<Weapon>();
        /* create virtual yaw transform (independent of chassis's transform) */
        virt_yaw = new GameObject("virt_yaw-" + this.name).transform;
        virt_yaw.transform.SetPositionAndRotation(this.transform.position, this.transform.rotation);
        virt_yaw.parent = this.transform.parent;
    }


    bool unowned => NetworkServer.active && this.netIdentity.connectionToClient == null;
    void Start() {
        /* even if no authority, external reference should be inited */
        _rigid.centerOfMass = centerOfMass;
        Cursor.lockState = CursorLockMode.Locked;

        if (hasAuthority) {
            BattleField.singleton.robo_local = this.robo_state;
        }
        if (unowned) {
            // Debug.Log("disable client auth of unowned robot");
            foreach (var tmp in GetComponents<NetworkTransformChild>()) {
                tmp.clientAuthority = false;
            }
        }
    }



    void Update() {
        if (!hasAuthority) {
            return;
        }

        if (robo_state.survival) {
            Look();
            Move();
            Shoot();
        } else
            StopMove();
        UpdateSelfUI();
    }


    RMUC_UI.BattleUI bat_ui => BattleField.singleton.bat_ui;    // alias
    void UpdateSelfUI() {
        bat_ui.rat_heat = wpn.heat_ratio;
        bat_ui.rat_cap = Mathf.Clamp01(currcap / maxcap);

        /* update buff display in UI */
        bat_ui.indic_buf[0] = Mathf.Approximately(robo_state.B_atk, 0f) ? -1            // atk
            : Mathf.Approximately(robo_state.B_atk, 0.5f) ? 0 : 1;
        bat_ui.indic_buf[1] = Mathf.Approximately(robo_state.B_cd, 1f) ? -1             // cldn
            : Mathf.Approximately(robo_state.B_cd, 3f) ? 0 : 1;
        bat_ui.indic_buf[2] = Mathf.Approximately(robo_state.B_rev, 0f) ? -1            // rev
            : Mathf.Approximately(robo_state.B_rev, 0.02f) ? 0 : 1;
        bat_ui.indic_buf[3] = Mathf.Approximately(robo_state.B_dfc, 0f) ? -1            // dfc
            : Mathf.Approximately(robo_state.B_dfc, 0.5f) ? 0 : 1;
        HeroState hs = robo_state.GetComponent<HeroState>();
        bat_ui.indic_buf[4] = hs == null || !hs.sniping ? -1 : 0;
        bat_ui.indic_buf[5] = Mathf.Approximately(robo_state.B_pow, 0f) ? -1 : 0;       // lea
    }


    void StopMove() {
        foreach (var wc in wheelColliders) {
            wc.motorTorque = 0;
            wc.brakeTorque = 0.1f;
        }
    }


    const int wheel_num = 4;
    const float efficiency = 0.5f;
    const float charge_coeff = 0.1f;          // how much of torque_avail will be used to charge capacity
    const float discharge_coeff = 1.8f;      // how fast capacity discharge 
    const float torque_drive = 20f;
    const float torque_spin = 20f;
    bool discharging = false;
    float torque_avail;
    bool braking => playing && Input.GetKey(KeyCode.X);
    bool spinning => playing && Input.GetKey(KeyCode.LeftShift);
    float h => playing ? Input.GetAxis("Horizontal") : 0;
    float v => playing ? Input.GetAxis("Vertical") : 0;
    void Move() {
        /* Manage Power */
        torque_avail = efficiency * robo_state.power;

        if (Input.GetKeyDown(KeyCode.C) && playing)
            discharging = !discharging;

        /* store energy in capacity */
        if (discharging && currcap > 1) {
            currcap -= (discharge_coeff - 1) * robo_state.power * Time.deltaTime;
            torque_avail *= discharge_coeff;
        } else {
            currcap += charge_coeff * torque_avail * Time.deltaTime;
            torque_avail *= 1 - charge_coeff;
        }
        currcap = Mathf.Min(maxcap, currcap);

        /* brake */
        if (braking) {
            for (int i = 0; i < wheel_num; i++) {
                wheelColliders[i].steerAngle = (45 + 90 * i) % 360 * Mathf.Deg2Rad;
                wheelColliders[i].motorTorque = 0;
                wheelColliders[i].brakeTorque = 10;
            }
            Debug.Log("braking");
            currcap += torque_avail * Time.deltaTime;
            return;
        } else  // remove previous brake torque
            foreach (var wc in wheelColliders)
                wc.brakeTorque = 0;

        float chas2yaw = Vector3.SignedAngle(_rigid.transform.forward, virt_yaw.forward, _rigid.transform.up);

        // move the car and steer wheels
        float steer_ang = Mathf.Rad2Deg * Mathf.Atan2(h, v);
        steer_ang = steer_ang + chas2yaw;
        foreach (var wc in wheelColliders) {
            /* Note: steerAngle will CLAMP angle to [-360, 360]
                Get remainder, make sure steer_ang is in [-360, 360] */
            wc.steerAngle = steer_ang % 360;
            wc.motorTorque = torque_drive * Mathf.Sqrt(h * h + v * v);
        }


        /* spin */
        float torque = 0;
        if (spinning) {
            torque = torque_spin;
        } else {
            // make chassis follow turret(aka, yaw)
            if (Mathf.Abs(chas2yaw) > 5)
                torque = 0.2f * this.PID(chas2yaw);
        }

        /* get sum of force */
        float torque_now = 0;
        for (int i = 0; i < wheel_num; i++) {
            float ang1 = wheelColliders[i].steerAngle * Mathf.Deg2Rad;
            float ang2 = (45 + 90 * i) % 360 * Mathf.Deg2Rad;
            Vector2 f1 = wheelColliders[i].motorTorque * new Vector2(Mathf.Cos(ang1), Mathf.Sin(ang1));
            Vector2 f2 = torque * new Vector2(Mathf.Cos(ang2), Mathf.Sin(ang2));
            Vector2 f_all = f1 + f2;
            wheelColliders[i].steerAngle = (Mathf.Rad2Deg * Mathf.Atan2(f_all.y, f_all.x));
            wheelColliders[i].motorTorque = f_all.magnitude;
            torque_now += wheelColliders[i].motorTorque;
            /* rotate the visual model */
            if (wheels.Length > i)
                wheels[i].transform.localEulerAngles = new Vector3(0, wheelColliders[i].steerAngle, 0);
        }
        if (torque_now < 1) {   // counted as not moving
            foreach (var wc in wheelColliders)
                wc.brakeTorque = 0.1f;
            currcap += torque_avail * Time.deltaTime;
            return;
        } else
            for (int i = 0; i < wheel_num; i++)
                wheelColliders[i].motorTorque *= torque_avail / torque_now;

    }


    /* keep yaw.up coincides with _rigid.up */
    void CalibVirtYaw() {
        // yaw.transform.position = _rigid.transform.position;
        Vector3 axis = Vector3.Cross(virt_yaw.transform.up, _rigid.transform.up);
        float ang = Vector3.Angle(virt_yaw.transform.up, _rigid.transform.up);
        virt_yaw.transform.Rotate(axis, ang, Space.World);
    }


    /* Get look dir from user input */
    bool autoaim => playing && Input.GetMouseButton(1);
    float mouseX => playing ? 2 * Input.GetAxis("Mouse X") : 0;
    float mouseY => playing ? 2 * Input.GetAxis("Mouse Y") : 0;
    void Look() {
        CalibVirtYaw();
        /* correct yaw's transform, i.e., elimate attitude error caused by following movement */
        yaw.rotation = virt_yaw.rotation;
        if (!autoaim || !AutoAim()) {
            pitch_ang -= mouseY;
            pitch_ang = Mathf.Clamp(pitch_ang, -pitch_max, -pitch_min);
            /* Rotate Transform "pitch" by user input */
            pitch.localEulerAngles = new Vector3(pitch_ang, 0, 0);
            /* Rotate Transform "virt_yaw" by user input */
            virt_yaw.transform.Rotate(_rigid.transform.up, mouseX, Space.World);

            last_target = null;
        }
        /* update yaw's transform, i.e., transform yaw to aim at target (store in virt yaw) */
        yaw.rotation = virt_yaw.rotation;
    }


    static float SignedAngleOnPlane(Vector3 from, Vector3 to, Vector3 norm) {
        Vector3 tmp1 = Vector3.ProjectOnPlane(from, norm);
        Vector3 tmp2 = Vector3.ProjectOnPlane(to, norm);
        return Vector3.SignedAngle(tmp1, tmp2, norm);
    }


    List<ArmorController> enemy_armors => robo_state.armor_color == ArmorColor.Red ?
        ArmorController.vis_armors_blue : ArmorController.vis_armors_red;
    Camera robo_cam => Camera.main;
    Vector3 start => bullet_start.transform.position;
    ArmorController target, last_target;
    bool AutoAim() {
        float minang = 45;
        foreach (ArmorController ac in enemy_armors) {
            // judge whether armor's enabled
            if (!ac.en)
                continue;
            // judge whether armor's facing turret
            Vector3 v1 = ac.transform.position - start;
            Vector3 v2 = ac.transform.TransformVector(ac.norm_in);
            if (Vector3.Angle(v1, v2) >= 75)
                continue;

            float ang = Vector3.Angle(ac.transform.position - start, bullet_start.transform.forward);
            if (ang < minang || ac == last_target) {
                RaycastHit hitinfo;
                Ray ray = new Ray(start, ac.transform.position - start);
                // judge armor under cover
                // max distance of autoaim: 10 meter
                if (!Physics.Raycast(ray, out hitinfo, maxDist, ~LayerMask.GetMask("Ignore Raycast")))
                    continue;
                if (hitinfo.collider.gameObject == ac.gameObject) {
                    minang = ang;
                    target = ac;
                    if (ac == last_target)  // preferably aiming at last target
                        break;
                }
                // Debug.DrawLine(ac.transform.position, start, Color.blue);
            }
        }
        if (minang >= 45) {
            return false;
        }
        Vector3 pos = target.transform.position;
        last_target = target;
        Debug.Log("last_target id: " + last_target.GetInstanceID());
        // Debug.DrawLine(start, target, Color.yellow);
        if (CalcFall(ref pos))
            AimAt(pos);
        else {
            Debug.Log("too far to reach");
            return false;
        }

        return true;
    }


    float spd => robo_state.bullspd;
    const float g = 9.8f;
    bool CalcFall(ref Vector3 target) {
        Vector3 r = target - start;
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
        // Debug.DrawLine(start, target, Color.yellow);
        target[1] += y_new - y;
        // Debug.DrawLine(start, target, Color.green);
        return true;
    }


    void AimAt(Vector3 target) {
        Vector3 d = target - pitch.transform.position;
        float d_yaw = dynCoeff * RoboController.SignedAngleOnPlane(bullet_start.forward, d, virt_yaw.transform.up);
        float d_pitch = dynCoeff * RoboController.SignedAngleOnPlane(bullet_start.forward, d, pitch.transform.right);
        virt_yaw.transform.Rotate(virt_yaw.transform.up, d_yaw, Space.World);
        pitch.transform.Rotate(pitch.transform.right, d_pitch, Space.World);
        pitch_ang += d_pitch;       // update pitch_ang so that when turret won't look around switch off auto-aim
    }


    // get ammunition supply at reborn spot
    [Command]
    public void CmdSupply(string robot_s, int num) {
        // todo: add judge of money
        GameObject obj = GameObject.Find(robot_s);
        RoboController rc = obj.GetComponent<RoboController>();
        bool in_supp_spot = rc.robo_state.robo_buff.FindIndex(i => i.tag == BuffType.rev) != -1;
        if (rc != null && in_supp_spot) {
            int money_req = rc.wpn.caliber == Caliber._17mm ? num : 15 * num;
            bool is_red = obj.GetComponent<RoboState>().armor_color == ArmorColor.Red;
            if (is_red) {
                if (money_req <= BattleField.singleton.money_red) {
                    rc.wpn.bullnum += num;
                    BattleField.singleton.money_red -= money_req;
                    Debug.Log("call supply");
                }
            } else if (money_req <= BattleField.singleton.money_blue) {
                rc.wpn.bullnum += num;
                BattleField.singleton.money_blue -= money_req;
                Debug.Log("call supply");
            }
        }
    }


    bool is_fire => playing && Input.GetMouseButton(0);
    void Shoot() {
        if (is_fire && Time.time - last_fire > 0.15) {
            CmdShoot(bullet_start.position, bullet_start.forward * robo_state.bullspd + _rigid.velocity);
            last_fire = Time.time;
        }
    }
    [Command]
    public void CmdShoot(Vector3 pos, Vector3 vel) {
        if (!NetworkClient.active) {
            ShootBull(pos, vel);
        }
        RpcShoot(pos, vel);
    }
    [ClientRpc]
    void RpcShoot(Vector3 pos, Vector3 vel) {
        ShootBull(pos, vel);
    }
    void ShootBull(Vector3 pos, Vector3 vel) {
        GameObject bullet = wpn.GetBullet();
        if (bullet == null) {
            Debug.Log("no bullet");
            return;
        }
        bullet.transform.position = pos;
        bullet.GetComponent<Rigidbody>().velocity = vel;
        bullet.GetComponent<Bullet>().hitter = this.gameObject;
    }
}
