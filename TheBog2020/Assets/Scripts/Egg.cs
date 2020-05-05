using System.Collections;
using System.Collections.Generic;
using AlmenaraGames;
using UnityEngine;

/// <summary>
/// Tracks team ID
/// Track if it is in the Nest
/// Tracks whether it is held
/// </summary>
public class Egg : MonoBehaviour
{
    //0 = red, 1 = blue
    public int TeamID;
    
    //Bools
    public bool IsHeld;
    public bool OutOfNest;
    public bool IsSpawning;
    public bool inWhirlpool;
    
    //Audio
    public AudioObject SplashSound;
    
    //Play splash on contact with water
    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("Water"))
        {
            MultiAudioManager.PlayAudioObject(SplashSound, transform.position);
        }
    }
}
