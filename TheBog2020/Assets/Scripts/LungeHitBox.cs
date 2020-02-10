using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Only active while player is lunging
/// Checks to see if it overlaps another player
/// If it does, it then sends a raycast to that player to see if it hits a lunge hitbox first, which leads to a clash
/// Either calls a clash on its player or death on the targeted enemy
/// </summary>
public class LungeHitBox : MonoBehaviour
{
    
    //Reference to PlayerController
    private PlayerController _pc;
    
    // Start is called before the first frame update
    void Start()
    {
        _pc = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController otherPlayer = other.GetComponent<PlayerController>();
            if (otherPlayer.TeamID != _pc.TeamID)
            {
                RaycastHit hit = new RaycastHit();
                Vector3 rayCastDir = other.transform.position - transform.position;
                if (Physics.Raycast(transform.position, rayCastDir, out hit, 10))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        //It's a kill   
                    }
                    else if (hit.transform.CompareTag("HitBox"))
                    {
                        //Its a clash
                    }
                }
            }
        }
    }
}
