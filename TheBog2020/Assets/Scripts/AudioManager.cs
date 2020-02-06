using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all of the Lists of sound effects and plays them when called from other objects
/// Manages the ambient sound and music cues
/// </summary>
public class AudioManager : MonoBehaviour
{
    
    //Singleton
    public static AudioManager AM;
    
    // Start is called before the first frame update
    void Start()
    {
        if (AM == null)
        {
            DontDestroyOnLoad(gameObject);
            AM = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
