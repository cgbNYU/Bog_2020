using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    
    //State Machine
    private enum GameState
    {
        Title,
        MatchInProgress,
        MatchEnd
    }

    [SerializeField]private GameState _gameState;
    
    
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
        //Initialize State
        _gameState = GameState.Title;
    }

    // Update is called once per frame
    void Update()
    {
        switch (_gameState)
        {
            case GameState.Title:
                UIManager.UM.DisplayStartGameUI();
                if (Input.anyKey)
                {
                    InitializeArrays();
                    GameStart();
                    UIManager.UM.ClearAllUIElements();
                    _gameState = GameState.MatchInProgress;
                }
                break;
            case GameState.MatchInProgress:
                //TODO:remove when done. Debug kill player one for testing
                if (Input.GetKeyDown(KeyCode.Backslash))
                {
                    PlayerControllers[0].KillPlayer();
                }
                break;
            case GameState.MatchEnd:
                if (Input.GetKeyDown(KeyCode.R))
                {
                    //TODO: Reset the game. temp: reloading scene
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    _gameState = GameState.Title;
                }
                break;
            default: 
                Debug.Log("Game Manager State Machine broke.");
                break;
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

    public void EndGame(int losingTeamId)
    {
        UIManager.UM.DisplayEndGameUI(losingTeamId);
        _gameState = GameState.MatchEnd;
    }

    #endregion
}
