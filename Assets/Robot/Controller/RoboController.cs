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
    public Transform robo_cam;
    
    [HideInInspector]public float currcap = 0;
    const int maxcap = 500;

    private Rigidbody _rigid;
    private float last_fire = 0;
    private float pitch_ang = 0;
    private float pitch_min = -30;
    private float pitch_max = 40;
    private float yaw_ang = 0;
    private Weapon wpn;
    private RoboState robo_state;

    private bool playing => Cursor.lockState == CursorLockMode.Locked;

    /// <summary>
    /// non-API
    /// </summary>
    public override void OnStartClient() {
        base.OnStartClient();
        if (hasAuthority) {
            Transform tmp = Camera.main.transform;
            tmp.parent = robo_cam;
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


    void Start() {
        /* even if no authority, external reference should be inited */
        _rigid = GetComponent<Rigidbody>();
        _rigid.centerOfMass = centerOfMass;
        Cursor.lockState = CursorLockMode.Locked;
        robo_state = GetComponent<RoboState>();
        wpn = GetComponent<Weapon>();
        if (yaw != null)
            yaw_ang = yaw.eulerAngles.y;

        if (hasAuthority) {
            BattleField.singleton.robo_local = this.robo_state;
        }
    }


    void Update() {
        if (!hasAuthority)
            return;

        SetCursor();
        if (robo_state.survival) {
            Move();
            Look();
            Shoot();
        }
        else
            StopMove();
        Supply();
        UpdateSelfUI();
    }


    RMUC_UI.BattleUI bat_ui => BattleField.singleton.bat_ui;    // alias
    void UpdateSelfUI() {
        bat_ui.rat_heat = wpn.heat_ratio;
        bat_ui.rat_cap = Mathf.Clamp01(currcap / maxcap);

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



    void SetCursor() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None
                : CursorLockMode.Locked;
        }
    }


    void StopMove() {
        foreach (var wc in wheelColliders) {
            wc.motorTorque = 0;
            wc.brakeTorque = 0.1f;
        }
    }


    const int wheel_num = 4;
    const float efficiency = 0.4f;
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
            currcap -= (discharge_coeff - 1) * robo_state.power * Time.deltaTime ;
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
                wheelColliders[i].brakeTorque = 5;
            }
            Debug.Log("braking");
            currcap += torque_avail * Time.deltaTime;
            return;
        } else  // remove previous brake torque
            foreach (var wc in wheelColliders)
                wc.brakeTorque = 0;


        // move the car and steer wheels
        float steer_ang = Mathf.Rad2Deg * Mathf.Atan2(h, v);
        steer_ang = steer_ang + yaw.localEulerAngles.y;
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
            float d_ang = -Mathf.DeltaAngle(yaw_ang, _rigid.transform.eulerAngles.y);
            if (Mathf.Abs(d_ang) < 5) d_ang = 0;
            /* TODO: use PID controller */
            torque = 0.2f * PID(d_ang);
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


    /* Get look dir from user input */
    float mouseX => playing ? 2 * Input.GetAxis("Mouse X") : 0;
    float mouseY => playing ? 2 * Input.GetAxis("Mouse Y") : 0;
    void Look() {
        pitch_ang -= mouseY;
        pitch_ang = Mathf.Clamp(pitch_ang, -pitch_max, -pitch_min);
        yaw_ang += mouseX;
        /* Rotate Transform "yaw" & "pitch" */
        pitch.localEulerAngles = new Vector3(pitch_ang, 0, 0);
        yaw.eulerAngles = new Vector3(0, yaw_ang, 0);
        yaw.localEulerAngles = new Vector3(0, yaw.localEulerAngles.y, 0);
    }


    // get ammunition supply at reborn spot
    void Supply() {
        if (Input.GetKeyDown(KeyCode.O) && playing) {
            bool in_supp_spot = robo_state.robo_buff.FindIndex(i => i.tag == BuffType.rev) != -1;
            if (in_supp_spot) {
                bool shw = !BattleField.singleton.bat_ui.supp_ui.activeSelf;
                BattleField.singleton.bat_ui.supp_ui.SetActive(shw);
                Cursor.lockState = shw ? CursorLockMode.None : CursorLockMode.Locked;
            }
        }
    }
    [Command]
    public void CmdSupply(string robot_s, int num) {
        // todo: add judge of money
        GameObject obj = GameObject.Find(robot_s);
        Weapon weap;
        if (TryGetComponent<Weapon>(out weap)) {
            int money_req = weap.caliber == Caliber._17mm ? num : 15 * num;
            bool is_red = robot_s.Contains("red");
            if (is_red) {
                if (money_req <= BattleField.singleton.money_red) {
                    weap.bullnum += num;
                    BattleField.singleton.money_red -= money_req;
                    Debug.Log("call supply");
                }
            }
            else if (money_req <= BattleField.singleton.money_blue) {
                weap.bullnum += num;
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
