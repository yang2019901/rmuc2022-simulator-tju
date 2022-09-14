using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTest : MonoBehaviour {
    [Header("Kinematic")]
    public Rigidbody _rigid;
    public Vector3 centerOfMass;
    [Header("turret")]
    public Transform yaw;
    public Transform pitch;
    [Header("chassis")]
    public Transform chassis;
    public Transform[] wheels;
    [Header("order: FL-FR-BR-BL")]
    public WheelCollider[] wheelColliders;

    [Header("View")]
    public Transform robo_cam;

    private float pitch_ang = 0;
    private float pitch_min = -30;
    private float pitch_max = 40;
    private float yaw_ang = 0;

    void Start() {
        Transform tmp = Camera.main.transform;
        tmp.parent = robo_cam;
        tmp.rotation = robo_cam.rotation;
        tmp.position = robo_cam.position;

        _rigid.centerOfMass = centerOfMass;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        SetCursor();
        Move();
        Look();
    }


    void SetCursor() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None
                : CursorLockMode.Locked;
        }
    }


    void Move() {
        /* Manage Power */
        int wheel_num = 4;
        float efficiency = 0.6f;
        float wheel_power_single = 30 * efficiency / wheel_num;

        /* Get move direction from user input */
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 mov_dir = h * chassis.right + v * chassis.forward;
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
            foreach (WheelCollider wc in wheelColliders) {
                wc.steerAngle = 0f;
                wc.motorTorque = 0f;
            }
        }

        /* make chassis follow turret(aka, yaw) */
        float d_ang = -Mathf.DeltaAngle(yaw_ang, chassis.localEulerAngles.y);
        /* TODO: use PID controller */
        float torque = 0.2f * d_ang;
        for (int i = 0; i < wheel_num; i++) {
            float ang1 = wheelColliders[i].steerAngle * Mathf.Deg2Rad;
            float ang2 = (45 + 90 * i) % 360 * Mathf.Deg2Rad;
            Vector2 f1 = wheelColliders[i].motorTorque * new Vector2(Mathf.Cos(ang1), Mathf.Sin(ang1));
            Vector2 f2 = torque * new Vector2(Mathf.Cos(ang2), Mathf.Sin(ang2));
            Vector2 f_all = f1 + f2;
            wheelColliders[i].steerAngle = Mathf.Rad2Deg * Mathf.Atan2(f_all.y, f_all.x);
            wheelColliders[i].motorTorque = f_all.magnitude;
            /* rotate the visual model */
            if (wheels.Length > i)
                wheels[i].transform.localEulerAngles = new Vector3(0, wheelColliders[i].steerAngle, 0);
        }
    }


    void Look() {
        /* Get look dir from user input */
        float mouseX = 2 * Input.GetAxis("Mouse X");
        float mouseY = 2 * Input.GetAxis("Mouse Y");
        pitch_ang -= mouseY;
        pitch_ang = Mathf.Clamp(pitch_ang, -pitch_max, -pitch_min);
        yaw_ang += mouseX;
        Debug.Log("yaw_ang: " + yaw_ang);
        /* Rotate Transform "yaw" & "pitch" */
        pitch.localEulerAngles = new Vector3(pitch_ang, 0, 0);
        yaw.transform.Rotate(chassis.up, mouseX, Space.World);
    }
}