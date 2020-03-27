﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

/// <summary>
/// Takes the player input and manages the positions, states, and current stats of each player
/// Placed on each player object
/// </summary>
public class PlayerController : MonoBehaviour
{

    public PlayerControllerTuning _pcTune;
    
    #region Movement Variables

    [Header("Movement Variables")] 
    private float _leftForce;
    private float _rightForce;
    public float ForceIncrease;
    public float ForceDecrease;
    public float MaxForce;
    public float WingOffset;
    public float WingDrag; //MUST BE NEGATIVE
    public float QuadAngularDrag;
    public float MaxTorque;
    public int BufferFrames;

    #endregion

    #region Attack Variables
    [Header("Attack Variables")]
    public float LungeTargetRadius;
    public float LungeRange;
    public float LungeForce;
    public float LungeTime;
    public float ClashForce;

    public float SpitForce;
    public float SpitTime;

    public float DeathTime;
    
    #endregion

    #region LockOn

    [Header("Lock On Variables")] 
    public float LockOnRange;
    [Range(0.01f, 5f)]
    public float LockTorque;
    [Range(-0.001f, -2f)]
    public float LockDrag;

    private Transform _lockTargetTransform;
    public Transform _antennaeStalkPivot;
    public GameObject _antennaeBulb;
    public Transform _antennaeStalkReset;
        
    #endregion
    
    #region General Variables
    [Header("General Variables")]
    //Public
    public int PlayerID;
    public int TeamID; //0 = red, 1 = blue
    public Collider LungeCollider;
    public Transform Spitter;
    
    //Private
    private Vector3 _leftStickVector;
    private Vector3 _rightStickVector;
    private Vector3 _leftStickLast;
    private Vector3 _rightStickLast;
    private int _bufferFrames;
    private bool _lungeButton;
    private bool _spitButtonHeld;
    private Rigidbody _rb;
    private bool _isGrounded;
    private Player _rewiredPlayer;
    private Camera _cam;
    private Vector3 _camRelativeVector;
    private Animator _animator;
    private PlayerEggHolder _eggHolder;
    
    #endregion

    #region State Machine

    //Movement State
    public enum MoveState
    {
        Neutral,
        Lunging,
        LockOn,
        Spitting,
        Airborne,
        Bouncing,
        Dead
    }

    private MoveState _moveState;

    private float _stateTimer;
    
    //Functions
    public void StateTransition(MoveState newState, float time) //handles ALL state transitions
    {
        //Set the state timer
        _stateTimer = time;
        
        //Change to the new state
        _moveState = newState;
        
        //Check for state specific things
        if (newState == MoveState.Lunging)
        {
            LungeCollider.enabled = true;
            _animator.Play("TestAnim_Lunge");
            _rb.AddForce(transform.forward * LungeForce);
        }
        else if (newState == MoveState.Neutral)
        {
            _stateTimer = 0;
            LungeCollider.enabled = false;
        }
    }

    public MoveState CheckState() //Called from ANYWHERE to check what the current state is
    {
        return _moveState;
    }

    #endregion
    
    #region Start/Updates
    
    // Start is called before the first frame update
    void Start()
    {
        //Initialize RigidBody
        _rb = GetComponent<Rigidbody>();
        
        //Initialize rewired
        _rewiredPlayer = ReInput.players.GetPlayer(PlayerID);
        
        //Initialize camera
        _cam = GetComponentInChildren<Camera>();
        
        //Initialize animator
        _animator = GetComponentInChildren<Animator>();
        
        //Initialize inputs
        ResetInputs();
        
        //Initialize state
        StateTransition(MoveState.Neutral, 0);
        _stateTimer = 0;
        
        //Initialize attacks
        LungeCollider.enabled = false;
        
        //Initialize egg holder
        _eggHolder = GetComponent<PlayerEggHolder>();
        
        //Initialize PC tuning variables
        //Debug.Assert(_pcTune == null, "Please assign a PC tuning to the player controller.");
        InitializePCTuning(_pcTune);
        
    }
    
    // This function initializes all the tuning variables from the scriptable PC tuning object attached to this player.
    public void InitializePCTuning(PlayerControllerTuning _tune)
    {
        //Movement vars
        ForceIncrease = _tune.ForceIncrease;
        ForceDecrease = _tune.ForceDecrease;
        MaxForce = _tune.MaxForce;
        WingOffset = _tune.WingOffset;
        WingDrag = _tune.WingDrag;
        MaxTorque = _tune.MaxTorque;
        QuadAngularDrag = _tune.QuadAngularDrag;
        BufferFrames = _tune.BufferFrames; 
        //Attack vars
        LungeTargetRadius = _tune.LungeTargetRadius;
        LungeRange = _tune.LungeRange;
        LungeForce = _tune.LungeForce;
        LungeTime = _tune.LungeTime;
        ClashForce = _tune.ClashForce;
        SpitForce = _tune.SpitForce;
        SpitTime = _tune.SpitTime;
        DeathTime = _tune.DeathTime;
        
        //Lock-on vars
        LockOnRange = _tune.LockOnRange;
        LockTorque = _tune.LockTorque;
        LockDrag = _tune.LockDrag;
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
                _animator.Play("TestAnim_Idle");
                AntennaeRadar();
                LockOnCheck();
                RampedForce();
                Move();
                break;
            case MoveState.Lunging:
                LungeState();
                break;
            case MoveState.Spitting:
                SpitState();
                break;
            case MoveState.LockOn:
                LockReleaseCheck();
                LockState();
                RampedForce();
                Move();
                break;
            case MoveState.Airborne:
                break;
            case MoveState.Bouncing:
                break;
            case MoveState.Dead:
                DeathState();
                break;
            default:
                Debug.Log("state machine broke: " + PlayerID);
                break;
        }
    }
    
    #endregion

    #region Movement
    
    private void GetInputs()
    {
        //Grab the input vectors from the sticks
        _leftStickVector = new Vector3(_rewiredPlayer.GetAxis("L_Horz"), 0, _rewiredPlayer.GetAxis("L_Vert"));
        _rightStickVector = new Vector3(_rewiredPlayer.GetAxis("R_Horz"), 0, _rewiredPlayer.GetAxis("R_Vert"));

        //Attack inputs
        _lungeButton = _rewiredPlayer.GetButtonDown("Lunge");
        _spitButtonHeld = _rewiredPlayer.GetButton("Spit");
    }

    private void RampedForce()
    {
        //Check to see if the sticks are being pushed or not
        if (_leftStickVector.magnitude > 0)
        {
            //lerp towards max force
            _leftForce = Mathf.Lerp(_leftForce, MaxForce, ForceIncrease);
        }
        else
        {
            //lerp force towards zero
            _leftForce = Mathf.Lerp(_leftForce, 0, ForceDecrease);
        }

        if (_rightStickVector.magnitude > 0)
        {
            //lerp towards max force
            _rightForce = Mathf.Lerp(_rightForce, MaxForce, ForceIncrease);
        }
        else
        {
            //lerp towards zero
            _rightForce = Mathf.Lerp(_rightForce, 0, ForceDecrease);
        }
    }

    private void ResetInputs()
    {
        _leftStickVector = Vector3.zero;
        _rightStickVector = Vector3.zero;

        _lungeButton = false;
        _spitButtonHeld = false;

        _bufferFrames = BufferFrames;
    }

    
    //Taking player input
    //Applying forces on each side of the player transform
    //Using a rigidbody.addforce
    private void Move()
    {
        //Find the points on the wings to apply forces
        Vector3 leftWingWorldPoint = transform.TransformPoint(new Vector2(-WingOffset, 0));
        Vector3 rightWingWorldPoint = transform.TransformPoint(new Vector2(WingOffset, 0));

        //Get the forces being applied to each wingw
        Vector3 worldForceVectorLeft = _leftForce * transform.TransformVector(_leftStickVector);
        Vector3 worldForceVectorRight = _rightForce * transform.TransformVector(_rightStickVector);
        
        Debug.DrawLine(leftWingWorldPoint,leftWingWorldPoint + worldForceVectorLeft,Color.blue); 
        Debug.DrawLine(rightWingWorldPoint,rightWingWorldPoint + worldForceVectorRight,Color.cyan); 
        
        //Apply wing forces
        _rb.AddForceAtPosition(worldForceVectorLeft, leftWingWorldPoint);
        _rb.AddForceAtPosition(worldForceVectorRight, rightWingWorldPoint);
        
        //Calculate Quadratic Wing Drag
        Vector3 leftWingVel = _rb.GetPointVelocity(leftWingWorldPoint);
        Vector3 rightWingVel = _rb.GetPointVelocity(rightWingWorldPoint);
        Vector3 leftDragForce = leftWingVel.sqrMagnitude * leftWingVel.normalized * WingDrag;
        Vector3 rightDragForce = rightWingVel.sqrMagnitude * rightWingVel.normalized * WingDrag;
        
        //Apply Quadratic Wing drag
        _rb.AddForceAtPosition(leftDragForce, leftWingWorldPoint);
        _rb.AddForceAtPosition(rightDragForce, rightWingWorldPoint);
        
        //Calculate Quadratice Angular Drag
        Vector3 rotationVel = _rb.angularVelocity;
        Vector3 rotationDragForce = rotationVel.sqrMagnitude * rotationVel.normalized * QuadAngularDrag;
        
        //Apply Quadratic Rotational Drag
        _rb.AddTorque(rotationDragForce);
        
        //Debugs
        Debug.DrawRay(leftWingWorldPoint, transform.InverseTransformVector(_leftStickVector));
        Debug.DrawRay(rightWingWorldPoint, transform.InverseTransformVector(_rightStickVector));
    }

    

    #endregion

    #region Attacks
    
    private void Lunge()
    {
        if (_lungeButton)
        {
            _lungeButton = false;
            Debug.Log("LungeButton");
            //Check to see if there is a target in front of the player
            
            StateTransition(MoveState.Lunging, LungeTime);
        }
    }

    private void LungeState()
    {
        _stateTimer -= Time.deltaTime;
        if (_stateTimer <= 0)
        {
            
            _moveState = MoveState.Neutral;
        }
    }

    private void LungeRecovery()
    {
        
    }

    public void Clash(Vector3 clashDir)
    {
        //Knock the player back based on how the hit came in (this vector is calculated by the hitbox
        _rb.AddForce(clashDir * ClashForce);
    }

    private void AntennaeRadar()
    {
        if (EnemyInRange() != null) _antennaeStalkPivot.LookAt(EnemyInRange());
        else _antennaeStalkPivot.LookAt(_antennaeStalkReset);
        //TODO: add bulb changing color
    }


    //Iterate through the array of player controllers in GM, ignoring same team
    //Determine which is the closest within range and return that, otherwise return null
    private Transform EnemyInRange()
    {
        Transform closestEnemyTransform = null;
        float minDist = Mathf.Infinity;
        foreach (PlayerController pc in GameManager.GM.PlayerControllers)
        {
            if (pc.TeamID != TeamID)
            {
                float dist = Vector3.Distance(pc.transform.position, transform.position);
                if (dist <= LockOnRange && dist < minDist && pc._moveState != MoveState.Dead)
                {
                    closestEnemyTransform = pc.transform;
                    minDist = dist;
                }
            }
        }
        return closestEnemyTransform;
    }
    
    //Checks to see if you press the spit button within range of an enemy, which begins the lockon process
    private void LockOnCheck()
    {
        if (_spitButtonHeld)
        {
            Debug.Log("Button down  = " + _spitButtonHeld);
            _moveState = MoveState.LockOn;
        }
    }

    //Player will have torque applied to them so that they rotate towards their target
    //If the player releases the spit button, they shoot and stop locking on
    private void LockState()
    {     
        _lockTargetTransform = EnemyInRange(); //check to see if anyone is in range
        if (_lockTargetTransform != null) //if yes
        {
            //Calculate target direction
            Vector3 targetDirection = _lockTargetTransform.position - transform.position;
            targetDirection.Normalize();
        
            //Calculate angle between forward facing and target direction
            float angleInDegrees = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);

            //Apply torque, reducing force by the size of the angle
            _rb.AddTorque(transform.up * LockTorque * angleInDegrees);
            _rb.AddTorque(transform.up * _rb.angularVelocity.y * LockDrag);
        }
        
        //Check if you release the button
        LockReleaseCheck();
    }

    private void LockReleaseCheck()
    {
        if (!_spitButtonHeld) //when you fire
        {
            Spit();
        }
    }

    private void Spit()
    {
        GameObject spit = (GameObject) Instantiate(Resources.Load("Prefabs/Spit"));
        spit.transform.position = Spitter.position;
        spit.transform.rotation = Spitter.rotation;
        spit.GetComponent<SpitHitBox>().TeamID = TeamID;
        spit.GetComponent<Rigidbody>().AddForce(transform.forward * SpitForce);
        _stateTimer = SpitTime;
        _moveState = MoveState.Spitting;
    }
    
    private void SpitState()
    {
        _stateTimer -= Time.deltaTime;
        if (_stateTimer <= 0)
        {
            _stateTimer = 0;
            _moveState = MoveState.Neutral;
        }
    }

    #endregion
    
    #region Death

    public void KillPlayer()
    {
        _rb.velocity = Vector3.zero;
        _stateTimer = DeathTime;
        _animator.Play("TestAnim_Death");
        _moveState = MoveState.Dead;
        _eggHolder.DropEgg();
    }

    public void DeathState()
    {
        _stateTimer -= Time.deltaTime;
        if (_stateTimer <= 0)
        {
            _stateTimer = 0;
            EventManager.Instance.Fire(new Events.PlayerDeath(TeamID, PlayerID));
        }
    }
    
    #endregion

    #region Archived Functions

    /*
     
     
     private void BufferedInputs()
    {
        Vector3 tempLeftStick = new Vector3(_rewiredPlayer.GetAxis("L_Horz"), 0, _rewiredPlayer.GetAxis("L_Vert"));
        Vector3 tempRightStick = new Vector3(_rewiredPlayer.GetAxis("R_Horz"), 0, _rewiredPlayer.GetAxis("R_Vert"));
        if (tempLeftStick.magnitude == 0 && tempRightStick.magnitude == 0)
        {
            leftHeldFor = 0;
            rightHeldFor = 0;
            //Reset stick hold timers
            leftHeldTime = 0;
            rightHeldTime = 0;
            _leftStickVector = tempLeftStick;
            _rightStickVector = tempRightStick;
        }
        else if (tempLeftStick.magnitude > 0 && tempRightStick.magnitude > 0)
        {
            leftReleasedFor = 0;
            rightReleasedFor = 0;
            //Set stick held timers to max
            leftHeldTime = 1;
            rightHeldTime = 1;
            _leftStickVector = tempLeftStick;
            _rightStickVector = tempRightStick;
        }
        else
        {
            //count instead       
            if (tempLeftStick.magnitude > 0)
            {
                leftHeldFor++;
                
                leftReleasedFor = 0;
            }

            if (tempRightStick.magnitude > 0)
            {
                rightHeldFor++;
                
                rightReleasedFor = 0;
            }

            if (tempLeftStick.magnitude == 0)
            {
                leftReleasedFor++;
               
                leftHeldFor = 0;
            }

            if (tempRightStick.magnitude == 0)
            {
                rightReleasedFor++;
                
                rightHeldFor = 0;
            }

            //if the left stick has been buffering for a while, update it
            if (leftHeldFor > BufferFrames)
            {
                _leftStickVector = tempLeftStick;
                leftHeldTime += Time.deltaTime;
               
            }
            else if (leftReleasedFor > BufferFrames)
            {
                _leftStickVector = tempLeftStick;
                leftHeldTime = 0;
            }
            
            //if the right stick has been buffering for a while, update it
            if (rightHeldFor > BufferFrames)
            {
                _rightStickVector = tempRightStick;
                rightHeldTime += Time.deltaTime;
                
            }
            else if (rightReleasedFor > BufferFrames)
            {
                _rightStickVector = tempRightStick;
                rightHeldTime = 0;
            }
            
        }

    }

    private float WingForceMultiply(float heldTime)
    {
        float currentForce = 0;
        float pointOnCurve = ForceOnCurve.Evaluate(heldTime);
        currentForce = pointOnCurve * MaxForce;
        
        return currentForce;
    }
    
    
    private void MoveOneStick()
    {
        //Go forward or back
        _rb.AddForce(transform.forward * _leftStickVector.z * MaxForce);
        
        //Rotate
        _rb.AddTorque(transform.up * _leftStickVector.x * MaxTorque);
        
        //Calculate standard quadratic drag
        Vector3 moveVel = _rb.velocity;
        Vector3 moveDragForce = moveVel.sqrMagnitude * moveVel.normalized * WingDrag;
        
        //Apply quadratic drag
        _rb.AddForce(moveDragForce);
        
        //Calculate Quadratice Angular Drag
        Vector3 rotationVel = _rb.angularVelocity;
        Vector3 rotationDragForce = rotationVel.sqrMagnitude * rotationVel.normalized * QuadAngularDrag;
        
        //Apply Quadratic Rotational Drag
        _rb.AddTorque(rotationDragForce);
    }
    
    */

    #endregion
}
