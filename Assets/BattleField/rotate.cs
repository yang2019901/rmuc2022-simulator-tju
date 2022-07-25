using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class rotate : MonoBehaviour
{
    public Transform triangle_armor;
    public Transform big_armor;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 1; i <= 2; i++)
        {
            GameObject big_armor1 = GameObject.Instantiate(big_armor.gameObject, big_armor.position, big_armor.rotation);
            big_armor1.transform.RotateAround(triangle_armor.position, Vector3.up, 120*i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
