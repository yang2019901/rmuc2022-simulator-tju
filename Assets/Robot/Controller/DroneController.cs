using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DroneController : BasicController {
    [Header("Kinematic")]
    public Vector3 centerOfMass;
    [Header("turret")]
    public Transform yaw;
    public Transform pitch;

    [Header("Weapon")]
    public Transform bullet_start;
    [Header("View")]
    public Transform view;

    public const float speed = 0.5f;

    private float last_fire = 0;
    private float pitch_ang = 0;
    private float pitch_min = -30;
    private float pitch_max = 40;
    private Weapon wpn;
    private Animator anim_posture;   // control visual effect of leaning to make flying realistic
    private Rigidbody _rigid => robo_state.rigid;

    bool playing => Cursor.lockState == CursorLockMode.Locked;
    bool cmd_E => playing && Input.GetKey(KeyCode.E);
    bool cmd_Q => playing && Input.GetKey(KeyCode.Q);
    float v => playing ? Input.GetAxis("Vertical") : 0;
    float h => playing ? Input.GetAxis("Horizontal") : 0;

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
        anim_posture = GetComponent<Animator>();
        /* create virtual yaw transform (independent of chassis's transform) */
        virt_yaw = new GameObject("virt_yaw-" + this.name).transform;
        virt_yaw.transform.SetPositionAndRotation(yaw.transform.position, yaw.transform.rotation);
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
        if (!hasAuthority || !BattleField.singleton.started_game) {
            return;
        }

        if (robo_state.survival) {
            Look();
            Move();
            Shoot();
        }
        UpdateSelfUI();
    }


    RMUC_UI.BattleUI bat_ui => BattleField.singleton.bat_ui;    // alias
    void UpdateSelfUI() {
        /* update buff display in UI */
        bat_ui.indic_buf[0] = Mathf.Approximately(robo_state.B_atk, 0f) ? -1            // atk
            : Mathf.Approximately(robo_state.B_atk, 0.5f) ? 0 : 1;
        bat_ui.indic_buf[1] = Mathf.Approximately(robo_state.B_cd, 1f) ? -1             // cd
            : Mathf.Approximately(robo_state.B_cd, 3f) ? 0 : 1;
        bat_ui.indic_buf[2] = Mathf.Approximately(robo_state.B_rev, 0f) ? -1            // rev
            : Mathf.Approximately(robo_state.B_rev, 0.02f) ? 0 : 1;
        bat_ui.indic_buf[3] = Mathf.Approximately(robo_state.B_dfc, 0f) ? -1            // dfc
            : Mathf.Approximately(robo_state.B_dfc, 0.5f) ? 0 : 1;
        HeroState hs = robo_state.GetComponent<HeroState>();
        bat_ui.indic_buf[4] = hs == null || !hs.sniping ? -1 : 0;
        bat_ui.indic_buf[5] = Mathf.Approximately(robo_state.B_pow, 0f) ? -1 : 0;       // lea
    }


    PIDController pid_follow = new PIDController(0.5f, 0f, 10f);         // T = J*theta''. theta is set (by mouse) and T is CV. 
                                                                       //  so it's a second-order system. use PD controller
    PIDController pid_throttle = new PIDController(6f, 0.5f, 0.1f);
    PIDController pid_force_v = new PIDController(2f, 0.01f, 0f);                // control force forward
    PIDController pid_force_h = new PIDController(2f, 0.01f, 0f);                // control force right
    PIDController pid_lean_v = new PIDController(7f, 0.01f, 25f);
    PIDController pid_lean_h = new PIDController(7f, 0.01f, 25f);
    void Move() {
        // ascend and descend
        float vel_set = speed * ((cmd_E ? 1 : 0) - (cmd_Q ? 1 : 0));
        Vector3 f_thro = pid_throttle.PID(vel_set - _rigid.velocity.y) * Vector3.up;
        _rigid.AddForce(f_thro, ForceMode.Acceleration);

        // fly horizontally
        Vector3 vec_set = new Vector3();
        if (Mathf.Abs(v) > 1e-3 || Mathf.Abs(h) > 1e-3) {
            Vector3 vec_v = Vector3.ProjectOnPlane(virt_yaw.forward, Vector3.up).normalized;
            Vector3 vec_h = Vector3.ProjectOnPlane(virt_yaw.right, Vector3.up).normalized;
            vec_set = (h * vec_h + v * vec_v).normalized;
        } else
            vec_set = Vector3.zero;
        float tmp_v = pid_force_v.PID(vec_set.z - _rigid.velocity.z);
        float tmp_h = pid_force_h.PID(vec_set.x - _rigid.velocity.x);
        Vector3 force = tmp_v * Vector3.forward + tmp_h * Vector3.right;
        _rigid.AddForce(force, ForceMode.Acceleration);

        // set visual effect of leaning
        Vector3 error = 0.15f * vec_set - Vector3.ProjectOnPlane(_rigid.transform.up, Vector3.up);
    // Debug.LogFormat("vec_set: {0}\t error: {1}", vec_set, error);
        Vector3 torque = pid_lean_v.PID(error.z) * Vector3.right + pid_lean_h.PID(error.x) * Vector3.back;
        _rigid.AddTorque(torque, ForceMode.Acceleration);

        // wings follow turret
        float wing2yaw = Vector3.SignedAngle(_rigid.transform.forward, virt_yaw.forward, _rigid.transform.up);
        Vector3 torque_fol = pid_follow.PID(wing2yaw) * _rigid.transform.up;
        _rigid.AddTorque(torque_fol, ForceMode.Acceleration);

        // yaw rotates with _rigid, hence calibration is needed
        CalibYaw();
    }


    /* keep yaw.up coincides with _rigid.up */
    void CalibVirtYaw() {
        // yaw.transform.position = _rigid.transform.position;
        Vector3 axis = Vector3.Cross(virt_yaw.transform.up, _rigid.transform.up);
        float ang = Vector3.Angle(virt_yaw.transform.up, _rigid.transform.up);
        virt_yaw.transform.Rotate(axis, ang, Space.World);
    }


    /** align yaw's rotation to virt_yaw's rotation, should be called when yaw is 'dirty'
        (when yaw ain't aligned with virt_yaw and transform of yaw or any child is to use)
    */
    void CalibYaw() {
        yaw.rotation = virt_yaw.rotation;
    }

    /* Get look dir from user input */
    bool autoaim => playing && Input.GetMouseButton(1);
    bool runeMode => Input.GetKey(KeyCode.R);
    float mouseX => playing ? 2 * Input.GetAxis("Mouse X") : 0;
    float mouseY => playing ? 2 * Input.GetAxis("Mouse Y") : 0;
    void Look() {
        CalibVirtYaw();
        // must align yaw to virt_yaw now because 'bullet_start', which is child of yaw is to use 
        CalibYaw();
        if (!autoaim || !base.AutoAim(bullet_start, runeMode)) {
            pitch_ang -= mouseY;
            pitch_ang = Mathf.Clamp(pitch_ang, -pitch_max, -pitch_min);
            /* Rotate Transform "pitch" by user input */
            pitch.localEulerAngles = new Vector3(pitch_ang, 0, 0);
            /* Rotate Transform "virt_yaw" by user input */
            virt_yaw.transform.Rotate(_rigid.transform.up, mouseX, Space.World);

            base.last_target = null;
        }
        /* update yaw's transform, i.e., transform yaw to aim at target (store in virt yaw) */
        CalibYaw();
    }


    protected override void AimAt(Vector3 target) {
        Vector3 d = target - pitch.transform.position;
        float d_yaw = dynCoeff * RoboController.SignedAngleOnPlane(bullet_start.forward, d, virt_yaw.transform.up);
        float d_pitch = dynCoeff * RoboController.SignedAngleOnPlane(bullet_start.forward, d, pitch.transform.right);
        d_pitch = Mathf.Clamp(pitch_ang + d_pitch, -pitch_max, -pitch_min) - Mathf.Clamp(pitch_ang, -pitch_max, -pitch_min);
        virt_yaw.transform.Rotate(virt_yaw.transform.up, d_yaw, Space.World);
        pitch.transform.Rotate(pitch.transform.right, d_pitch, Space.World);
        pitch_ang += d_pitch;       // update pitch_ang so that when turret won't look around switch off auto-aim
    }


    // get ammunition supply at reborn spot
    [Command]
    public void CmdSupply(string robot_s, int num) {
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
