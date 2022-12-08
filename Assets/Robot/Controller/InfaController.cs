using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


/* infantry and hero's controller */
public class InfaController : BasicController {
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

    [SerializeField] private Rigidbody _rigid;
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
        _rigid.centerOfMass = centerOfMass;
        Cursor.lockState = CursorLockMode.Locked;
        robo_state = GetComponent<RoboState>();
        wpn = GetComponent<Weapon>();
        if (yaw != null)
            yaw_ang = yaw.transform.eulerAngles.y;

        if (hasAuthority) {
            BattleField.singleton.robo_local = this.robo_state;
        }
    }


    void Update() {
        if (!hasAuthority)
            return;

        SetCursor();
        // Look();
        Move();
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
        /* clear all wheelcolliders motortorque */
        for (int i = 0; i < wheel_num; i++)
            wheelColliders[i].motorTorque = 0;
        float chas2yaw = Vector3.SignedAngle(_rigid.transform.forward, yaw.forward, yaw.up);
        Debug.Log(string.Format("_rigid: {0}, yaw: {1}, axis: {2}, chas2yaw: {3}", _rigid.transform.forward,
            yaw.forward, yaw.up, chas2yaw));

        // move the car and steer wheels
        float steer_ang = Mathf.Rad2Deg * Mathf.Atan2(h, v);
        steer_ang = steer_ang + chas2yaw;
        foreach (var wc in wheelColliders) {
            /* Note: steerAngle will CLAMP angle to [-360, 360]
                Get remainder, make sure steer_ang is in [-360, 360] */
            wc.steerAngle = steer_ang % 360;
            wc.motorTorque = torque_drive * 0.5f * Mathf.Sqrt(h * h + v * v);
        }


        /* spin */
        float torque = 0;
        if (spinning) {
            torque = -torque_spin * 0.5f;
        } 
        // else {
        //     // chassis follows turret
        //     if (Mathf.Abs(chas2yaw) > 5)
        //         torque = 0.2f * PID(chas2yaw);
        // }

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
    }


    /* Get look dir from user input */
    float mouseX => playing ? 2 * Input.GetAxis("Mouse X") : 0;
    float mouseY => playing ? 2 * Input.GetAxis("Mouse Y") : 0;
    void Look() {
        pitch_ang -= mouseY;
        pitch_ang = Mathf.Clamp(pitch_ang, -pitch_max, -pitch_min);
        /* Rotate Transform "yaw" & "pitch" */
        pitch.localEulerAngles = new Vector3(pitch_ang, 0, 0);

        // yaw.transform.position = _rigid.transform.position;
        // Vector3 v1 = yaw.transform.up;
        // Vector3 v2 = _rigid.transform.up;
        // Vector3 axis = Vector3.Cross(v1, v2);
        // float ang = Vector3.Angle(v1, v2);
        // yaw.transform.Rotate(axis, ang, Space.World);

        yaw.transform.Rotate(yaw.up, mouseX, Space.World);
        Debug.Log("mouseX: " + mouseX);
    }

}