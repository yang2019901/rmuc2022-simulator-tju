using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RoboController : NetworkBehaviour {
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

    private Rigidbody _rigid;
    private float last_fire = 0;
    private float pitch_ang = 0;
    private float pitch_min = -30;
    private float pitch_max = 40;
    private float yaw_ang = 0;
    private Weapon wpn;
    private RoboState robo_state;


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
        if (!hasAuthority)
            return;
        _rigid = GetComponent<Rigidbody>();
        _rigid.centerOfMass = centerOfMass;
        Cursor.lockState = CursorLockMode.Locked;
        robo_state = GetComponent<RoboState>();
        wpn = GetComponent<Weapon>();
        if (yaw != null)
            yaw_ang = yaw.eulerAngles.y;

        BattleField.singleton.robo_local = this.robo_state;
        // BattleField.singleton.bat_ui.my_roboidx = BattleField.singleton.robo_all.FindIndex(i => i == this.robo_state);
    }

    void Update() {
        if (!hasAuthority)
            return;

        SetCursor();
        bool playing = Cursor.lockState == CursorLockMode.Locked;
        if (robo_state.survival && playing) {
            Move();
            Look();
            Shoot();
        } else {
            StopMove();
        }
        Supply();
        UpdateSelfUI();
    }

    RMUC_UI.BattleUI bat_ui => BattleField.singleton.bat_ui;    // alias
    void UpdateSelfUI() {
        bat_ui.ratio = wpn.heat_ratio;
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


    void Move() {
        /* Manage Power */
        int wheel_num = wheelColliders.Length;
        float efficiency = 0.4f;
        float torque_drive = 10f;
        float torque_spin = 10f;

        float torque_avail = efficiency * robo_state.power;

        bool braking = Input.GetKey(KeyCode.X);
        bool spinning = Input.GetKey(KeyCode.LeftShift);

        /* brake */
        if (braking) {
            for (int i = 0; i < wheel_num; i++) {
                wheelColliders[i].steerAngle = (45 + 90 * i) % 360 * Mathf.Deg2Rad;
                wheelColliders[i].brakeTorque = torque_avail / wheel_num;
            }
            Debug.Log("braking");
            return;
        } else  // remove previous brake torque
            foreach (var wc in wheelColliders)
                wc.brakeTorque = 0;

        /* Transform */
        // Get move direction from user input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

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
        if (torque_now < 10) {
            foreach (var wc in wheelColliders)
                wc.brakeTorque = 0.1f;
            return;
        } else
            for (int i = 0; i < wheel_num; i++)
                wheelColliders[i].motorTorque *= torque_avail / torque_now;
    }


    float sum = 0;
    float last_err;
    float Kp = 1f;
    float Ki = 0f;
    float Kd = 0f;
    /* use PID controller (Kp > 0) to calc MV */
    float PID(float err) {
        sum += err;
        float d_err = err - last_err;
        last_err = err;
        float output = Kp * (err + Ki * sum + Kd * d_err);
        // Debug.Log("pid output: " + output);
        return output;
    }


    void Look() {
        /* Get look dir from user input */
        float mouseX = 2 * Input.GetAxis("Mouse X");
        float mouseY = 2 * Input.GetAxis("Mouse Y");
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
        if (Input.GetKeyDown(KeyCode.O)) {
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
            weap.bullnum += num;
        }
    }


    void Shoot() {
        bool is_fire = Input.GetMouseButton(0);
        if (is_fire && Time.time - last_fire > 0.15) {
            CmdShoot(bullet_start.position, bullet_start.forward * robo_state.bullspd + _rigid.velocity);
            last_fire = Time.time;
        }
    }
    [Command]
    public void CmdShoot(Vector3 pos, Vector3 vel) {
        if (!NetworkClient.active) {
            Debug.Log("gains heat in pure server");
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
