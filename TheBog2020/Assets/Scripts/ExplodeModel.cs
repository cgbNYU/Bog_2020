using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeModel : MonoBehaviour
{
    //Lists to Hold the rigidbodies and the joints
    private Rigidbody[] _rbs;
    private Joint[] _joints;
    private bool _exploded;
    private float _timer;
    
    //Tuneables
    public float ExplodeForce;
    public float DespawnTime;
    
    // Start is called before the first frame update
    void Start()
    {
        _rbs = GetComponentsInChildren<Rigidbody>();
        _joints = GetComponentsInChildren<Joint>();

        _exploded = false;
        _timer = DespawnTime;
    }

    private void Update()
    {
        if (_exploded)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                Destroy(gameObject);
            }
        }
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

        _exploded = true;
    }
}
