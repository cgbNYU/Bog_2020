using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

/// <summary>
/// Takes the player input and manages the positions, states, and current stats of each player
/// Placed on each player object
/// </summary>
public class PlayerController : MonoBehaviour
{

    #region Physics Variables

    public float MaxForce = 10f;
    public float WingOffset = 1f;
    public float Drag = -0.01f;
    public float Gravity;
    public float Bounce;

    #endregion
    
    #region General Variables

    //Public
    public int PlayerID;
    public int TeamID; //0 = red, 1 = blue
    
    //Private
    private Vector3 _leftStickVector;
    private Vector3 _rightStickVector;
    private Rigidbody _rb;
    private bool _isGrounded;
    private Player _rewiredPlayer;
    private Camera _cam;
    private Vector3 _camRelativeVector;
    #endregion

    #region State Machine

    //Movement State
    private enum MoveState
    {
        Neutral,
        Airborne,
        Bouncing,
        Dead
    }

    private MoveState _moveState;

    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        //Initialize RigidBody
        _rb = GetComponent<Rigidbody>();
        
        //Initialize rewired
        _rewiredPlayer = ReInput.players.GetPlayer(PlayerID);
        
        //Initialize camera
        _cam = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
    }

    private void FixedUpdate()
    {
        switch (_moveState)
        {
            case MoveState.Neutral:
                break;
            case MoveState.Airborne:
                break;
            case MoveState.Bouncing:
                break;
            case MoveState.Dead:
                break;
            default:
                Debug.Log("state machine broke: " + PlayerID);
                break;
        }
        Move();
    }

    #region Movement

    private void GetInputs()
    {
        //Get input from the sticks
        _leftStickVector = new Vector3(_rewiredPlayer.GetAxis("L_Horz"), 0, _rewiredPlayer.GetAxis("L_Vert"));
        _rightStickVector = new Vector3(_rewiredPlayer.GetAxis("R_Horz"), 0, _rewiredPlayer.GetAxis("R_Vert"));
    }

    private void Move()
    {
        //Find the points on the wings to apply forces
        Vector3 leftWingWorldPoint = transform.TransformPoint(new Vector2(-WingOffset, 0));
        Vector3 rightWingWorldPoint = transform.TransformPoint(new Vector2(WingOffset, 0));

        //Get the forces being applied to each wing
        Vector3 worldForceVectorLeft = MaxForce * transform.TransformVector(_leftStickVector);
        Vector3 worldForceVectorRight = MaxForce * transform.TransformVector(_rightStickVector);
        
        Debug.DrawLine(leftWingWorldPoint,leftWingWorldPoint + worldForceVectorLeft,Color.blue); 
        Debug.DrawLine(rightWingWorldPoint,rightWingWorldPoint + worldForceVectorRight,Color.cyan); 
        
        //Apply wing forces
        _rb.AddForceAtPosition(worldForceVectorLeft, leftWingWorldPoint);
        _rb.AddForceAtPosition(worldForceVectorRight, rightWingWorldPoint);
        
        //Calculate Quadratic Drag
        Vector3 leftWingVel = _rb.GetPointVelocity(leftWingWorldPoint);
        Vector3 rightWingVel = _rb.GetPointVelocity(rightWingWorldPoint);
        Vector3 leftDragForce = leftWingVel.sqrMagnitude * leftWingVel.normalized * Drag;
        Vector3 rightDragForce = rightWingVel.sqrMagnitude * rightWingVel.normalized * Drag;
        
        //Apply Quadratic drag
        _rb.AddForceAtPosition(leftDragForce, leftWingWorldPoint);
        _rb.AddForceAtPosition(rightDragForce, rightWingWorldPoint);
        
        //Debugs
        Debug.DrawRay(leftWingWorldPoint, transform.InverseTransformVector(_leftStickVector));
        Debug.DrawRay(rightWingWorldPoint, transform.InverseTransformVector(_rightStickVector));
    }

    #endregion

    #region Death

    public void KillPlayer()
    {
        EventManager.Instance.Fire(new Events.PlayerDeath(TeamID, PlayerID));
    }

    #endregion
}
