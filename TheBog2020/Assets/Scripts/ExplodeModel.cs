using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeModel : MonoBehaviour
{
    //Lists to Hold the rigidbodies and the joints
    private Rigidbody[] _rbs;
    private Joint[] _joints;
    
    //Tuneables
    public float ExplodeForce;
    
    // Start is called before the first frame update
    void Start()
    {
        _rbs = GetComponentsInChildren<Rigidbody>();
        _joints = GetComponentsInChildren<Joint>();
    }

    public void Explode()
    {
        //If the joints are not null, turn off joints
        if (_joints != null)
        {
            foreach (Joint joint in _joints)
            {
                joint.breakForce = 0.01f;
            }
        }
        
        //If the rigidbodies are not null...
        if (_rbs != null)
        {
            foreach (Rigidbody rb in _rbs)
            {
                //Turn on gravity
                rb.useGravity = true;
                
                //Become dynamic
                rb.isKinematic = false;
                
                //Add a force
                Vector3 direction = Random.insideUnitSphere.normalized;
                rb.AddForce(direction * ExplodeForce);
            }
        }
    }
}
