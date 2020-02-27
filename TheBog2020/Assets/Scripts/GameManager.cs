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
    public PlayerController[] PlayerControllers;
    
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
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            PlayerControllers[0].KillPlayer();
        }
    }

    private void InitializeArrays()
    {
        //Nest array
        _nests = new Nest[2];
        _nests[0] = GameObject.Find("NestRed").GetComponent<Nest>();
        _nests[1] = GameObject.Find("NestBlue").GetComponent<Nest>();
        
        //Player array
        PlayerControllers = new PlayerController[4];

        PlayerControllers[0] = GameObject.Find("Player1").GetComponent<PlayerController>();
        PlayerControllers[1] = GameObject.Find("Player2").GetComponent<PlayerController>();
        PlayerControllers[2] = GameObject.Find("Player3").GetComponent<PlayerController>();
        PlayerControllers[3] = GameObject.Find("Player4").GetComponent<PlayerController>();
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

    #region Public Functions
    
    //Called from an object that is destroying an egg (eg. the whirlpool)
    public void DestroyEgg(int teamId, Egg egg)
    {
        _nests[teamId].DestroyEgg(egg);
    }

    #endregion
}
