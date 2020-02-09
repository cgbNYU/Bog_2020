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
    
    //References
    private Nest[] _nests;
    private PlayerController[] _playerControllers;
    
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
        
        InitializeArrays();
        GameStart();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitializeArrays()
    {
        //Nest array
        _nests = new Nest[2];
        _nests[0] = GameObject.Find("NestRed").GetComponent<Nest>();
        _nests[1] = GameObject.Find("NestBlue").GetComponent<Nest>();
        
        //Player array
        _playerControllers = new PlayerController[4];
    }

    private void GameStart()
    {
        //Spawn eggs at nests
        foreach (var nest in _nests)
        {
            nest.SpawnEggs();
        }
        
        //Reset anglerfish position
        
        //Select player spawn eggs
        
        //countdown
        
        //Start match
    }
}
