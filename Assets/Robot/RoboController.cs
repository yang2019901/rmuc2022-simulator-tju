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
        _rigid = GetComponent<Rigidbody>();
        _rigid.centerOfMass = centerOfMass;
        Cursor.lockState = CursorLockMode.Locked;
        robo_state = GetComponent<RoboState>();
        wpn = GetComponent<Weapon>();
        if (yaw != null)
            yaw_ang = yaw.eulerAngles.y;
    }

    void Update() {
        if (!hasAuthority)
            return;

        SetCursor();
        if (robo_state.survival) {
            Move();
            Look();
            Supply();
            Shoot();
        } else {
            StopMove();
        }
        // if (BattleField.singleton.bat_ui == null)
        //     Debug.Log("BattleField.singleton.bat_ui == null");
        BattleField.singleton.bat_ui.ratio = wpn.heat_ratio;
    }


    void SetCursor() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None
                : CursorLockMode.Locked;
        }
    }


    void StopMove() {
        foreach (WheelCollider wc in wheelColliders) {
            wc.motorTorque = 0f;
        }
    }


    void Move() {
        /* Manage Power */
        int wheel_num = 4;
        float efficiency = 0.4f;
        float wheel_power_single = robo_state.power * efficiency / wheel_num;

        /* brake */
        if (Input.GetKey(KeyCode.X)){
            foreach (WheelCollider wc in wheelColliders)
                wc.brakeTorque = wheel_power_single;
            return ;
        } else 
            foreach (WheelCollider wc in wheelColliders)
                wc.brakeTorque = 0;

        /* Get move direction from user input */
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 mov_dir = h * _rigid.transform.right + v * _rigid.transform.forward;
        mov_dir.Normalize();

        /* Rotate wheels and move the car */
        if (Mathf.Abs(h) > 1e-3 || Mathf.Abs(v) > 1e-3) {
            float steer_ang = Mathf.Rad2Deg * Mathf.Atan2(h, v);
            /* get remainder, make sure steer_ang is in [-360, 360] */
            foreach (WheelCollider wc in wheelColliders) {
                /* steerAngle will CLAMP angle to [-360, 360] */
                wc.steerAngle = steer_ang;
                wc.motorTorque = wheel_power_single * Mathf.Sqrt(h * h + v * v);
            }
        } else {
            StopMove();
        }

        /* make chassis follow turret(aka, yaw) */
        float d_ang = -Mathf.DeltaAngle(yaw_ang, _rigid.transform.eulerAngles.y);
        if (Mathf.Abs(d_ang) < 5)  d_ang = 0;
        /* TODO: use PID controller */
        float torque = 0.2f * PID(d_ang);
        for (int i = 0; i < wheel_num; i++) {
            float ang1 = wheelColliders[i].steerAngle * Mathf.Deg2Rad;
            float ang2 = (45 + 90 * i) % 360 * Mathf.Deg2Rad;
            Vector2 f1 = wheelColliders[i].motorTorque * new Vector2(Mathf.Cos(ang1), Mathf.Sin(ang1));
            Vector2 f2 = torque * new Vector2(Mathf.Cos(ang2), Mathf.Sin(ang2));
            Vector2 f_all = f1 + f2;
            wheelColliders[i].steerAngle = (Mathf.Rad2Deg * Mathf.Atan2(f_all.y, f_all.x));
            wheelColliders[i].motorTorque = f_all.magnitude;
            /* rotate the visual model */
            if (wheels.Length > i)
                wheels[i].transform.localEulerAngles = new Vector3(0, wheelColliders[i].steerAngle, 0);
        }
    }
    float sum = 0;
    float last_err;
    float Kp = 1f;
    float Ki = 0f;
    float Kd = 0f;
    /* use PID controller (Kp > 0) to calc MV */
    float PID(float err) {
        sum += err;
        float d_err = err-last_err;
        last_err = err;
        float output = Kp * (err + Ki*sum + Kd*d_err);
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
        if (Input.GetKeyDown(KeyCode.O) && gameObject.name.ToLower().Contains("infantry")) {
            CmdSupply(this.gameObject.name, 50);
        } else if (Input.GetKeyDown(KeyCode.I) && gameObject.name.ToLower().Contains("hero")) {
            CmdSupply(this.gameObject.name, 5);
        }
    }
    [Command] public void CmdSupply(string robot_s, int num) {
        // /* detect what buff this robot currently has */
        // string buffs = "";
        // foreach (Buff tmp in this.robo_state.robo_buff) {
        //     buffs += tmp + " ";
        // }
        // Debug.Log(buffs);

        if (this.robo_state.robo_buff.FindIndex(i => i.tag==BuffType.rev) == -1) {
            Debug.Log("not in revive spot");
            return ;
        }
        GameObject obj = GameObject.Find(robot_s);
        obj.GetComponent<Weapon>().bull_num += num;
    }


    void Shoot() {
        bool is_fire = Input.GetMouseButton(0);
        if (is_fire && Time.time - last_fire > 0.15) {
            CmdShoot(bullet_start.position, bullet_start.forward * robo_state.bullspd + _rigid.velocity);
            last_fire = Time.time;
        }
    }
    [Command] public void CmdShoot(Vector3 pos, Vector3 vel) {
        if (!NetworkClient.active) {
            Debug.Log("gains heat in pure server");
            ShootBull(pos, vel);
        }
        RpcShoot(pos, vel);
    }
    [ClientRpc] void RpcShoot(Vector3 pos, Vector3 vel) {
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
