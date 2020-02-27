using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Placed onto the whirlpool parent object
/// Will create a large overlap sphere that pulls objects into towards the center
/// Has a small trigger in the center which kills entities that enter it
/// </summary>
public class WhirlpoolScript : MonoBehaviour
{
    //Forces
    public float PullForce; //how hard are entities pulled into the whirlpool
    
    //OverlapSphere Values
    public float WhirlpoolRange;

    // Update is called once per frame
    void Update()
    {
        PullObjects();
    }

    //Pulls objects towards the center of the whirlpool when they enter range
    private void PullObjects()
    {
        Collider[] itemsInWhirlpool = Physics.OverlapSphere(transform.position, WhirlpoolRange);
        foreach (Collider col in itemsInWhirlpool)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 pullDir = col.transform.position - transform.position;
                pullDir.Normalize();
                rb.AddForce(pullDir * PullForce);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }
}
