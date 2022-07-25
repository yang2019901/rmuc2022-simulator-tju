using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Kinematic")]
    public Rigidbody _rigid;
    public Vector3 centerOfMass;
    [Header("turret")]
    public Transform yaw;
    public Transform pitch;
    [Header("chassis")]
    public Transform chassis;
    public Transform[] wheels;
    public WheelCollider[] wheelColliders;

    [Header("Weapon")]
    public Transform bullet_start;
    
    private float last_fire = 0;
    private float pitch_ang = 0;
    private float pitch_min = -20;
    private float pitch_max = 40;
    private float yaw_ang = 0;
    private delegate GameObject GetBullet();
    private GetBullet getBullet;

    void Start()
    {
        _rigid = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        _rigid.centerOfMass = centerOfMass;

        if (this.gameObject.name.Contains("infantry")) 
            getBullet = BulletPool.singleton.GetSmallBullet;
        else if (this.gameObject.name.Contains("hero"))
            getBullet = BulletPool.singleton.GetBigBullet;
        else 
            Debug.LogError("wrong car name: " + this.gameObject.name);
    }

    void FixedUpdate()
    {
        Move();
        Shoot();
    }

    void Move()
    {
        /* Manage Power */

        /* Get move direction from user input */
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 mov_dir = h*chassis.right + v*chassis.forward;
        mov_dir.Normalize();
        
        /* Rotate wheels and move the car */
        if (Mathf.Abs(h) > 1e-3 || Mathf.Abs(v) > 1e-3)
        {
            float dir = Mathf.Rad2Deg * Mathf.Atan2(h, v);
            foreach(Transform wheel in wheels)
                wheel.transform.localEulerAngles = new Vector3(0, dir, 0);
            /* get remainder, make sure steer_ang is in [-360, 360] */
            float steer_ang = (dir + chassis.localEulerAngles.y) % 360;
            foreach(WheelCollider wc in wheelColliders)
            {
                /* steerAngle will CLAMP angle to [-360, 360] */
                wc.steerAngle = steer_ang;
                wc.motorTorque = 8f * Mathf.Sqrt(h*h + v*v);
            }
        }
        else
        {
            foreach(WheelCollider wc in wheelColliders)
            {
                wc.steerAngle = chassis.localEulerAngles.y;
                wc.motorTorque = 0;
            }
        }

        /* Get look dir from user input */
        float mouseX = 3*Input.GetAxis("Mouse X");
        float mouseY = 2*Input.GetAxis("Mouse Y");
        pitch_ang -= mouseY;
        pitch_ang = Mathf.Clamp(pitch_ang, -pitch_max, -pitch_min);
        yaw_ang += mouseX;
        
        /* Rotate Transform "yaw" & "pitch" */
        pitch.localEulerAngles = new Vector3(pitch_ang, 0, 0);
        yaw.localEulerAngles = new Vector3(0, yaw_ang, 0);

        /* make chassis follow turret(aka, yaw) */
        Vector3 rotation = 3*Vector3.Cross(chassis.forward, yaw.forward);
        chassis.transform.Rotate(rotation, Space.World);
    }


    void Shoot()
    {
        bool is_fire = Input.GetMouseButton(0);
        if (is_fire && Time.time - last_fire > 0.15)
        {
            GameObject bullet = getBullet();
            bullet.transform.position = bullet_start.position;
            bullet.GetComponent<Rigidbody>().velocity = bullet_start.forward * 30;
            bullet.GetComponent<Bullet>().hitter = this.name;
            last_fire = Time.time;
        }
    }
}
