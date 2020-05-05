using System.Collections;
using System.Collections.Generic;
using AlmenaraGames;
using UnityEngine;


/// <summary>
/// Holds an array of random whale sounds and picks on to play every X seconds after the last one finishes
/// </summary>
public class RandomWhaleSong : MonoBehaviour
{
    
    //Sound source
    private MultiAudioSource _audioSource;
    
    //Timing variables
    public float SoundDelay;
    private float _timer;
    
    // Start is called before the first frame update
    void Start()
    {
        _timer = SoundDelay;
        _audioSource = GetComponent<MultiAudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0)
        {
            _audioSource.Play();
            
            _timer = SoundDelay + 10f;
        }
    }
}
