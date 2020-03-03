using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpitHitBox : MonoBehaviour
{
    public float KnockBackForce;
    public int TeamID;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController otherPlayer = other.GetComponent<PlayerController>();
            if (otherPlayer.TeamID != TeamID)
            {
                KillPlayer(otherPlayer);
                Destroy(gameObject);
            }
        }
    }

    private void KillPlayer(PlayerController pc)
    {
        pc.KillPlayer();
    }
    
    private void KnockBackPlayer(Collider other)
    {
        Vector3 knockBackDir = transform.position - other.transform.position;
        knockBackDir = new Vector3(knockBackDir.x, 0, knockBackDir.z);
        knockBackDir.Normalize();
        other.GetComponent<Rigidbody>().AddForce(knockBackDir * KnockBackForce);
    }
}
