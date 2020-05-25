using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupScript : MonoBehaviour
{

    public SpitterScript.SpitState whichPowerup;
    private ParticleSystem _particle;
    private bool _canPowerup;
    private Rigidbody _rb;
    public SpawnPowerup _spawner;

    private void Start()
    {
        _canPowerup = true;
        _rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _spawner.Spawning = true;
            other.GetComponentInChildren<SpitterScript>().ChangeState(whichPowerup);
            Destroy(gameObject);
        }
    }
}
