using System.Collections;
using System.Collections.Generic;
using AlmenaraGames;
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
    
    //Egg destroying sound
    public AudioObject EggDestroySound;

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
                Vector3 pullDir = transform.position - col.transform.position;
                pullDir.Normalize();
                rb.AddForce(pullDir * PullForce);
            }
        }
    }

    //This will be on the center killbox object
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Egg"))
        {
            Egg egg = other.GetComponent<Egg>();
            GameManager.GM.DestroyEgg(egg.TeamID, egg);
            
            //Play sound
            MultiAudioManager.PlayAudioObject(EggDestroySound, transform.position);
        }
        else if (other.gameObject.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            pc.KillPlayer();
        }
    }
}
