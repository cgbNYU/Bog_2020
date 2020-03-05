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

    #region Movement Variables

    [Header("Movement Variables")] 
    public float MaxForce;
    public float WingOffset;
    public float WingDrag; //MUST BE NEGATIVE
    public float QuadAngularDrag;
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

    public MoveState moveState;

    private float _stateTimer;

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
        moveState = MoveState.Neutral;
        _stateTimer = 0;
        
        //Initialize attacks
        LungeCollider.enabled = false;
    }

    

    // Update is called once per frame
    void Update()
    {
        GetInputs();
    }

    private void FixedUpdate()
    {
        switch (moveState)
        {
            case MoveState.Neutral:
                _animator.Play("TestAnim_Idle");
                LockOnCheck();
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
        //Get input from the sticks
        BufferedInputs();

        //Attack inputs
        _lungeButton = _rewiredPlayer.GetButtonDown("Lunge");
        _spitButtonHeld = _rewiredPlayer.GetButton("Spit");
    }

    //Input buffer variables
    private int leftReleasedFor;
    private int rightReleasedFor;
    private int leftHeldFor;
    private int rightHeldFor;

    private float leftHeldTime;
    private float rightHeldTime;

    public AnimationCurve WingForceCurve = new AnimationCurve();
    
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
        float pointOnCurve = WingForceCurve.Evaluate(heldTime);
        currentForce = pointOnCurve * MaxForce;
        
        return currentForce;
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
        Vector3 worldForceVectorLeft = WingForceMultiply(leftHeldTime) * transform.TransformVector(_leftStickVector);
        Vector3 worldForceVectorRight = WingForceMultiply(rightHeldTime) * transform.TransformVector(_rightStickVector);
        
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
            
            _rb.AddForce(transform.forward * LungeForce);

            _stateTimer = LungeTime;
            LungeCollider.enabled = true;
            _animator.Play("TestAnim_Lunge");
            moveState = MoveState.Lunging;
        }
    }

    private void LungeState()
    {
        _stateTimer -= Time.deltaTime;
        if (_stateTimer <= 0)
        {
            _stateTimer = 0;
            LungeCollider.enabled = false;
            moveState = MoveState.Neutral;
        }
    }

    public void Clash(Vector3 clashDir)
    {
        //Knock the player back based on how the hit came in (this vector is calculated by the hitbox
        _rb.AddForce(clashDir * ClashForce);
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
                if (dist <= LockOnRange && dist < minDist)
                {
                    closestEnemyTransform = pc.transform;
                    Debug.Log(closestEnemyTransform.GetComponent<PlayerController>().PlayerID);
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
            moveState = MoveState.LockOn;
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
        moveState = MoveState.Spitting;
    }
    
    private void SpitState()
    {
        _stateTimer -= Time.deltaTime;
        if (_stateTimer <= 0)
        {
            _stateTimer = 0;
            moveState = MoveState.Neutral;
        }
    }

    #endregion
    
    #region Death

    public void KillPlayer()
    {
        _rb.velocity = Vector3.zero;
        _stateTimer = DeathTime;
        _animator.Play("TestAnim_Death");
        moveState = MoveState.Dead;
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
}
