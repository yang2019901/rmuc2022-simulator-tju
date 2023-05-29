using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EngineerController : BasicController {
    [Header("Kinematic")]
    public Vector3 centerOfMass;
    [Header("turret")]
    public Transform pitch;
    [Header("order: FL-FR-BR-BL")]
    public WheelCollider[] wheelColliders;

    [Header("View")]
    public Transform robo_cam;

    [Header("Catching")]
    public Transform elev_1st;
    public Transform elev_2nd;
    public Transform arm;
    public Transform wrist;
    public Transform claw;

    [Header("Revive Card")]
    public Transform rev_card;

    private CatchMine cm;


    bool playing => Cursor.lockState == CursorLockMode.Locked;
    bool cmd_C => playing && Input.GetKeyDown(KeyCode.C);
    bool cmd_Z => playing && Input.GetKeyDown(KeyCode.Z);
    bool cmd_R => playing && Input.GetKeyDown(KeyCode.R);
    bool cmd_lshift => playing && Input.GetKey(KeyCode.LeftShift);
    bool cmd_E => playing && Input.GetKey(KeyCode.E);
    bool cmd_Q => playing && Input.GetKey(KeyCode.Q);
    bool braking => playing && Input.GetKey(KeyCode.X);
    float h => playing ? Input.GetAxis("Horizontal") : 0;
    float v => playing ? Input.GetAxis("Vertical") : 0;
    float mouseX => playing ? 2 * Input.GetAxis("Mouse X") : 0;
    float mouseY => playing ? 2 * Input.GetAxis("Mouse Y") : 0;

    /// <summary>
    /// non-API
    /// </summary>
    public override void OnStartClient() {
        base.OnStartClient();
        if (isOwned) {
            Transform tmp = Camera.main.transform;
            tmp.parent = robo_cam;
            tmp.localEulerAngles = Vector3.zero;
            tmp.localPosition = Vector3.zero;
        }
    }


    void Awake() {
        robo_state = GetComponent<RoboState>();
        cm = GetComponentInChildren<CatchMine>();
    }


    void Start() {
        /* even if no authority, external reference should be inited */
        _rigid.centerOfMass = centerOfMass;
        yaw_ang = _rigid.transform.localEulerAngles.y;

        if (isOwned) {
            BattleField.singleton.robo_local = this.robo_state;
        }
    }


    void Update() {
        if (!isOwned)
            return;

        if (robo_state.survival) {
            Move();
            Look();
            MovClaw();
            Catch();
            Save();
        // used to debug mine exchanging
        // if (Input.GetKeyDown(KeyCode.G))
        //     BattleField.singleton.XchgMine(robo_state.armor_color, true);
        } else
            StopMove();

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


    void StopMove() {
        foreach (var wc in wheelColliders) {
            wc.motorTorque = 0;
            wc.brakeTorque = 0.1f;
        }
    }


    const int wheel_num = 4;
    const float torque_drive = 8f;
    const float torque_spin = 2f;
    PIDController chas_ctl = new PIDController(1, 0, 0);
    void Move() {
        if (cmd_lshift) {
            StopMove();
            return;
        }

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

        // Get move direction from user input

        // calc drive force
        float steer_ang = Mathf.Rad2Deg * Mathf.Atan2(h, v);
        float t1 = 0;
        if (Mathf.Abs(h) >= 1e-3 || Mathf.Abs(v) >= 1e-3)
            t1 = torque_drive;

        /* spin */
        float t2 = 0;
        if (cmd_Q ^ cmd_E)
            yaw_ang += (cmd_E ? 1 : -1) * 30 * Time.deltaTime;

        float d_ang = -Mathf.DeltaAngle(yaw_ang, _rigid.transform.eulerAngles.y);
        if (Mathf.Abs(d_ang) < 5) d_ang = 0;
        t2 = 0.2f * chas_ctl.PID(d_ang);


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


    float pitch_ang = 0;
    float yaw_ang = 0;
    const float pitch_min = -30;
    const float pitch_max = 40;
    void Look() {
        /* Get look dir from user input */
        pitch_ang -= mouseY;
        pitch_ang = Mathf.Clamp(pitch_ang, -pitch_max, -pitch_min);
        if (!cmd_lshift)
            yaw_ang += mouseX;
        /* Rotate Transform "yaw" & "pitch" */
        pitch.localEulerAngles = new Vector3(pitch_ang, 0, 0);
    }


    readonly Vector3 elev_1st_start = new Vector3(0, 0, 0);
    readonly Vector3 elev_1st_end = new Vector3(0, 0, -0.24f);
    readonly Vector3 elev_2nd_start = new Vector3(0, 0, 0);
    readonly Vector3 elev_2nd_end = new Vector3(0, -0.2f, 0);
    readonly Vector3 arm_start = new Vector3(0, 0, 0);
    readonly Vector3 arm_end = new Vector3(-0.4f, 0, 0);
    readonly Vector3 claw_lt = new Vector3(-0.32f, 0.178f, 0.054f);
    readonly Vector3 claw_rt = new Vector3(-0.08f, 0.178f, 0.054f);
    float rat_arm = 0;
    float rat_claw = 0.5f;
    int st_wrist = 0;
    float ang = 0;
    float rat_elev = 0f;
    void MovClaw() {
        if (cmd_lshift) {
            /* elevate */
            if (cmd_E ^ cmd_Q)
                rat_elev = Mathf.Clamp01(rat_elev + (cmd_E ? Time.deltaTime : -Time.deltaTime));
            /* move arm */
            rat_arm = Mathf.Clamp01(rat_arm + v * Time.deltaTime);
            /* move wrist */
            if (cmd_Z ^ cmd_C)
                st_wrist = st_wrist + (cmd_C ? 90 : -90);
            /* move claw */
            rat_claw = Mathf.Clamp01(rat_claw + h * Time.deltaTime);
        }
        elev_1st.localPosition = Vector3.Lerp(elev_1st_start, elev_1st_end, rat_elev);
        elev_2nd.localPosition = Vector3.Lerp(elev_2nd_start, elev_2nd_end, rat_elev);
        arm.localPosition = Vector3.Lerp(arm_start, arm_end, rat_arm);
        ang -= 8 * Time.deltaTime * Mathf.DeltaAngle(st_wrist, ang);
        wrist.localEulerAngles = new Vector3(ang, 0, 0);
        claw.localPosition = Vector3.Lerp(claw_lt, claw_rt, rat_claw);
    }

    
    public bool holding = false;
    void Catch() {
        /* if no cmd to change holding state, there's nothing to do */
        if (cmd_R && cmd_lshift) {
            CmdCatch(holding);
        }
    }
    [Command]
    void CmdCatch(bool holding) {
        RpcCatch(holding);
    }
    [ClientRpc]
    void RpcCatch(bool holding) {
        if (holding) {
            cm.Release();
            cm.enabled = false;
        } else {
            cm.enabled = true;
        }
        this.holding = !holding;
    }


    readonly Vector3 card_start = new Vector3(0, 0.084f, 0.22f);
    readonly Vector3 card_end = new Vector3(0, 0.084f, 0.50f);
    bool saving = false;
    float rat_rev = 0;
    public void Save() {
        if (!cmd_lshift && cmd_R) {
            saving = !saving;
        }
        rat_rev = Mathf.Clamp01(rat_rev + (saving ? Time.deltaTime : -Time.deltaTime));
        rev_card.localPosition = Vector3.Lerp(card_start, card_end, rat_rev);
    }

}