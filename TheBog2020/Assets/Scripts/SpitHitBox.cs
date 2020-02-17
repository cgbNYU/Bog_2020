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
                Vector3 knockBackDir = transform.position - other.transform.position;
                other.GetComponent<Rigidbody>().AddForce(knockBackDir * KnockBackForce);
                Destroy(gameObject);
            }
        }
    }
}
