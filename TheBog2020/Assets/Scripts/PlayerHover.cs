using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Raycast down from a certain number of points to find distance to ground
/// Increase force up based on how close you are to the ground
/// </summary>
public class PlayerHover : MonoBehaviour
{
    
    //Public Tuneables
    public float HoverForce;
    
    //Private
    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //Begin raycast above player
        Vector3 rayCastOrigin = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            
            //Grab the distance to the ground
            float groundDist = Vector3.Distance(hit.point, transform.position);
            Debug.Log("Ground dist = " + groundDist);
            //Apply an upward force divided by distance to ground
            _rb.AddForce(Vector3.up * HoverForce * (1 / groundDist));
        }
    }
}
