using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo_ManualMoveTarget : MonoBehaviour
{
    public float speed;

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
            transform.position += Vector3.up * Time.deltaTime * speed;
        if (Input.GetKey(KeyCode.S))
            transform.position -= Vector3.up * Time.deltaTime * speed;
        if (Input.GetKey(KeyCode.D))
            transform.position += Vector3.right * Time.deltaTime * speed;
        if (Input.GetKey(KeyCode.A))
            transform.position -= Vector3.right * Time.deltaTime * speed;
    }
}
