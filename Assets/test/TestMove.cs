using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TestMove : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return ;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        transform.Translate(Time.deltaTime*(h*transform.right + v*transform.forward));
    }
}
