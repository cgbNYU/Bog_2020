using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpitHitBox : MonoBehaviour
{
    public float spitLifeTime;
    public float KnockBackForce;
    public Collider Col;
    public MeshRenderer MR;
    public ParticleSystem Particles;
    public int TeamID;

    IEnumerator destroySelfAfterLifeTime(float t)
    {
        yield return new WaitForSeconds(t);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        // Don't destroy the spit when it collides with these objects 
        if (!other.gameObject.CompareTag("Nest") && !other.gameObject.CompareTag("DropTrigger") &&
            !other.gameObject.CompareTag("Spit") && !other.gameObject.CompareTag("Egg"))
        {
             Destroy(gameObject);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController otherPlayer = other.GetComponent<PlayerController>();
            if (otherPlayer.TeamID != TeamID)
            {
                KillPlayer(otherPlayer);
                DeactivateSpit();
            }
        }
        // Don't destroy the spit when it collides with these objects 
        if (!other.gameObject.CompareTag("Nest") && !other.gameObject.CompareTag("DropTrigger") &&
            !other.gameObject.CompareTag("Spit") && !other.gameObject.CompareTag("Egg"))
        {
            DeactivateSpit();
        }
    }

    private void KillPlayer(PlayerController pc)
    {
        pc.KillPlayer();
    }

    private void DeactivateSpit()
    {
        Col.enabled = false;
        MR.enabled = false;
        Particles.Play();
        StartCoroutine(destroySelfAfterLifeTime(spitLifeTime));
    }
    
    private void KnockBackPlayer(Collider other)
    {
        Vector3 knockBackDir = transform.position - other.transform.position;
        knockBackDir = new Vector3(knockBackDir.x, 0, knockBackDir.z);
        knockBackDir.Normalize();
        other.GetComponent<Rigidbody>().AddForce(knockBackDir * KnockBackForce);
    }
}
