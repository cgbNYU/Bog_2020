using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    
    //Tuning
    public PlayerControllerTuning NewTune;
    
    //Cameras
    private GameObject _playerCams;
    private GameObject _followCams;
    private GameObject _endGameCam;
    
    //State Machine
    private enum GameState
    {
        AttractScreen,
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
        _gameState = GameState.AttractScreen;
        
        //Get camera references
        _playerCams = GameObject.Find("Cameras");
        _followCams = GameObject.Find("FollowCams");
        _endGameCam = GameObject.Find("EndGameCam");
    }

    private float loadingTimer = 0;
    // Update is called once per frame
    void Update()
    {
        switch (_gameState)
        {
            case GameState.AttractScreen:
                //Timer to ignore input briefly in Attract Mode to let the Video buffer
                loadingTimer += Time.deltaTime;
                if (loadingTimer >= 3)
                {
                    if (Input.anyKey)
                    {
                        GameObject.Find("AttractScreen_Player").GetComponent<AttractScreen>().StopVideo();
                        //Play bgm
                        _gameState = GameState.Title;
                    }
                }

                break;
            case GameState.Title: //Tutorial popups
                if (UIManager.UM.AllPlayersReady())
                {
                    InitializeArrays();
                    GameStart(); 
                    UIManager.UM.CloseTutorialPopups();
                    UIManager.UM.ClearAllUIElements();
                    _gameState = GameState.MatchInProgress;
                    foreach (PlayerController pc in PlayerControllers)
                    {
                        pc.StateTransition(PlayerController.MoveState.Dead, 0);
                    }
                }
                break;
            case GameState.MatchInProgress:
                //TODO:remove when done. Debug kill player one for testing
                if (Input.GetKeyDown(KeyCode.Backslash))
                {
                    PlayerControllers[0].KillPlayer();
                }

                if (Input.GetKeyDown(KeyCode.LeftControl))
                {
                    Debug.Log("Tuning updated");
                        foreach (PlayerController pc in PlayerControllers)
                        {
                            pc.InitializePCTuning(NewTune);
                        }
                    
                }
                break;
            case GameState.MatchEnd:
                if (Input.GetKeyDown(KeyCode.Backspace))
                {
                    //TODO: Reset the game. temp: reloading scene
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    _gameState = GameState.AttractScreen;
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
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        PlayerControllers = new PlayerController[players.Length];

        for (int i = 0; i < players.Length; i++)
        {
            PlayerControllers[i] = players[i].GetComponent<PlayerController>();
        }
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
        KillCamera();
        _gameState = GameState.MatchEnd;
    }

    public void UpdateEggsRemainingUI(int teamID, int eggsRemaining)
    {
        UIManager.UM.UpdateEggsRemainingUI(teamID,eggsRemaining);
    }

    private int lastKillerPlayerID;
    public float killCamDuration = 10f;
    
    public void GetLastKillerPlayerID(int playerID)
    {
        lastKillerPlayerID = playerID;
        Debug.Log(lastKillerPlayerID);
    }

    public void KillCamera()
    {
        //set all the virtual cams inactive
        _followCams.SetActive(false);
        
        //set all cam brains, except the last player who made a kill, to inactive 
        for (int i = 0; i < _playerCams.transform.childCount; i++)
        {
            if(i != lastKillerPlayerID) _playerCams.transform.GetChild(i).gameObject.SetActive(false);
            else
            {
                //lerp the winning players camera to full screen
                _playerCams.transform.GetChild(i).GetComponent<Camera>().DORect(new Rect(0, 0, 1, 1), killCamDuration);
            }
        }

        //set all end game camera objects to active
        foreach (Transform child  in _endGameCam.transform)
        {
            child.gameObject.SetActive(true);
        }
    }
    

    #endregion

    #region Inspector

    #endregion
}
