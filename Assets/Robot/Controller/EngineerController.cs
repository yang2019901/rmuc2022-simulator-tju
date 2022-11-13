using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EngineerController : BasicController{
    [Header("Kinematic")]
    public Vector3 centerOfMass;
    [Header("turret")]
    public Transform pitch;
    [Header("order: FL-FR-BR-BL")]
    public WheelCollider[] wheelColliders;

    [Header("View")]
    public Transform robo_cam;

    private Rigidbody _rigid;
    private float pitch_ang = 0;
    private float pitch_min = -30;
    private float pitch_max = 40;
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
        /* even if no authority, external reference should be inited */
        _rigid = GetComponent<Rigidbody>();
        _rigid.centerOfMass = centerOfMass;
        Cursor.lockState = CursorLockMode.Locked;
        robo_state = GetComponent<RoboState>();

        if (hasAuthority) {
            BattleField.singleton.robo_local = this.robo_state;
        }
    }


    void Update() {
        if (!hasAuthority)
            return;

        SetCursor();
        bool playing = Cursor.lockState == CursorLockMode.Locked;
        if (robo_state.survival && playing) {
            Move();
            Look();
        } else {
            StopMove();
        }
        UpdateSelfUI();
    }


    RMUC_UI.BattleUI bat_ui => BattleField.singleton.bat_ui;    // alias
    void UpdateSelfUI() {
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
    const float torque_drive = 10f;
    const float torque_spin = 5f;
    void Move() {
        bool catching = Input.GetKey(KeyCode.LeftShift);
        if (catching)
            return;

        bool braking = Input.GetKey(KeyCode.X);
        bool spinning = Input.GetKey(KeyCode.E) ^ Input.GetKey(KeyCode.Q);  // exclusive or

        /* brake */
        if (braking) {
            for (int i = 0; i < wheel_num; i++) {
                wheelColliders[i].steerAngle = (45 + 90 * i) % 360 * Mathf.Deg2Rad;
                wheelColliders[i].brakeTorque = 5;
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

        // calc drive force
        float steer_ang = Mathf.Rad2Deg * Mathf.Atan2(h, v);
        float t1 = 0;
        if (Mathf.Abs(h) >= 1e-3 || Mathf.Abs(v) >= 1e-3)
            t1 = torque_drive;

        /* spin */
        float t2 = 0;
        if (spinning)
            t2 = (Input.GetKey(KeyCode.E) ? 1: -1) * torque_spin;

        /* get sum of force */
        float torque = 0;
        for (int i = 0; i < wheel_num; i++) {
            float ang1 = steer_ang * Mathf.Deg2Rad;
            float ang2 = (45 + 90 * i) % 360 * Mathf.Deg2Rad;
            Vector2 f1 = t1 * new Vector2(Mathf.Cos(ang1), Mathf.Sin(ang1));
            Vector2 f2 = t2 * new Vector2(Mathf.Cos(ang2), Mathf.Sin(ang2));
            Vector2 f_all = f1 + f2;
            wheelColliders[i].steerAngle = (Mathf.Rad2Deg * Mathf.Atan2(f_all.y, f_all.x));
            wheelColliders[i].motorTorque = f_all.magnitude;
            torque += wheelColliders[i].motorTorque;
        }
        if (torque < 1)   // counted as not moving
            foreach (var wc in wheelColliders)
                wc.brakeTorque = 0.1f;

        return;
    }


    void Look() {
        /* Get look dir from user input */
        float mouseY = 2 * Input.GetAxis("Mouse Y");
        pitch_ang -= mouseY;
        pitch_ang = Mathf.Clamp(pitch_ang, -pitch_max, -pitch_min);
        /* Rotate Transform "yaw" & "pitch" */
        pitch.localEulerAngles = new Vector3(pitch_ang, 0, 0);
    }

}
