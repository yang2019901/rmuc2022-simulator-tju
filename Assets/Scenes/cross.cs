using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cross : MonoBehaviour {
    public Transform v1;
    public Transform v2;


    void Start() {
        v1 = GameObject.Find("v1").transform;
        v2 = GameObject.Find("v2").transform;
    }
    
    void Update() {
        test();
    }

    void test() {
        // if (Input.GetKeyDown(KeyCode.Space)) {
            Vector3 axis = Vector3.Cross(v2.up, v1.up);
            float ang = Vector3.Angle(v1.up, v2.up);
            Debug.Log(string.Format("v1.up: {0}\tv2.up: {1}\naxis: {2}\tang: {3}", v1.up, v2.up, axis, ang));

            v2.transform.Rotate(axis * 0.4f, ang, Space.World);
        // }
    }
}
