using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game Manager tracks the arc of each game
/// Manages the beginning and ending of each game
/// </summary>
public class GameManager : MonoBehaviour
{
    
    //Singleton
    public static GameManager GM;
    
    // Start is called before the first frame update
    void Start()
    {
        //Singleton
        if (GM == null)
        {
            DontDestroyOnLoad(gameObject);
            GM = this;
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
