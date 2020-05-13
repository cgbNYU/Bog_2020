using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPowerup : MonoBehaviour
{

    public GameObject powerupPrefab;

    private GameObject _currentPowerup;

    public float SpawnTime;

    public Transform spawnLocation;

    public bool Spawning;

    private float _timer;
    // Start is called before the first frame update
    void Start()
    {
        Spawning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Spawning)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                Spawning = false;
                _currentPowerup = Instantiate(powerupPrefab, transform.position, Quaternion.identity);
                _currentPowerup.GetComponent<PowerupScript>()._spawner = this;
                _timer = SpawnTime;
            }
        }
    }
}
