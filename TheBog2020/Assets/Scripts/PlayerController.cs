﻿using System.Collections;
using System.Collections.Generic;
using AlmenaraGames;
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
    private float _spitTimer;

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
    private Transform _previousLockTargetTransform;
    public Transform _antennaeStalkPivot;
    public GameObject _antennaeBulb;
    public Transform _antennaeStalkReset;
        
    #endregion
    
    #region General Variables
    [Header("General Variables")]
    //Public
    public int PlayerID;
    public int TeamID; //0 = red, 1 = blue
    public Transform Spitter;
    private SpitterScript _spitterScript;
    public float InvulnerableTime;
    public float HatchTime;
    public MeshRenderer ShieldMeshRenderer;
    
    //Private
    private Vector3 _leftStickVector;
    private Vector3 _rightStickVector;
    private Vector3 _leftStickLast;
    private Vector3 _rightStickLast;
    private int _bufferFrames;
    private bool _lungeButton;
    private bool _spitButton;
    private bool _lockButtonHeld;
    private Rigidbody _rb;
    private bool _isGrounded;
    private Player _rewiredPlayer;
    private Camera _cam;
    private Vector3 _camRelativeVector;
    private Animator _animator;
    private PlayerEggHolder _eggHolder;
    private PlayerModelSpawner _modelSpawner;
    private HighlightTarget _highlightTarget;
    private PlayerSpitSacs _spitSacs;
    private PlayerModelIndex _playerModelIndex;
    
    #endregion

    #region State Machine

    //Movement State
    public enum MoveState
    {
        Neutral,
        Invulnerable,
        Lunging,
        LockOn,
        Spitting,
        Airborne,
        Bouncing,
        Dead,
        Inactive,
        Hatching
    }

    public MoveState _moveState;

    private float _stateTimer;
    
    //Functions
    public void StateTransition(MoveState newState, float time) //handles ALL state transitions
    {
        //Set the state timer
        _stateTimer = time;
        
        //Change to the new state
        _moveState = newState;
        
        //Check for state specific things
        if (newState == MoveState.Neutral)
        {
            _stateTimer = 0;
            ShieldMeshRenderer.enabled = false;
        }
        else if (newState == MoveState.Invulnerable)
        {
            ShieldMeshRenderer.enabled = true;
        }
        else if (newState == MoveState.Hatching)
        {
            //Get the new model index
            _playerModelIndex = GetComponentInChildren<PlayerModelIndex>();
            //Activate egg tween
            _playerModelIndex.ScaleAnimation.DOPlay();
        }
        else if (newState == MoveState.Dead)
        {
            ResetInputs();
        }
    }

    public MoveState CheckState() //Called from ANYWHERE to check what the current state is
    {
        return _moveState;
    }

    #endregion

    #region Audio Variables
    [Header("Audio Variables")]
    public AudioObject SpitSound;
    public AudioObject DeathSound;
    public AudioObject HitWallSound;

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
        
        //Initialize spit sacs
        _spitSacs = GetComponent<PlayerSpitSacs>();
        
        //Initialize egg holder
        _eggHolder = GetComponent<PlayerEggHolder>();
        
        //Initialize Spitter
        _spitterScript = GetComponentInChildren<SpitterScript>();
        
        //Initialize inputs
        ResetInputs();
        
        //Initialize state
        StateTransition(MoveState.Inactive, 0);
        _stateTimer = 0;
        _spitTimer = 0;
        ShieldMeshRenderer.enabled = false;

        //Initialize PC tuning variables
        //Debug.Assert(_pcTune == null, "Please assign a PC tuning to the player controller.");
        InitializePCTuning(_pcTune);
        
        //Initialize Target Highlighter
        _highlightTarget = GetComponent<HighlightTarget>();
        

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
                //AntennaeRadar();
                LockOnCheck();
                Move();
                Spit();
                break;
            case MoveState.Invulnerable:
                //AntennaeRadar();
                LockOnCheck();
                Move();
                Spit();
                ReturnToNeutralCountdown();
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
                Spit();
                break;
            case MoveState.Airborne:
                break;
            case MoveState.Bouncing:
                break;
            case MoveState.Dead:
                DeathState();
                break;
            case MoveState.Inactive:
                break;
            case MoveState.Hatching:
                ReturnToInvulCountdown();
                break;
            default:
                Debug.Log("state machine broke: " + PlayerID);
                break;
        }
    }
    
    #endregion

    #region Movement
    
    private Vector3 _leftStickHeadingVector, _rightStickHeadingVector;
    private Vector3 _leftStickRefVel, _rightStickRefVel;

    private Vector3 _leftStickForceVector, _rightStickForceVector;
    private Vector3 _leftStickForceRefVel, _rightStickForceRefVel;
    
    private void GetInputs()
    {
        //Grab the input vectors from the sticks
        _leftStickVector = new Vector3(_rewiredPlayer.GetAxis("L_Horz"), 0, _rewiredPlayer.GetAxis("L_Vert"));
        _rightStickVector = new Vector3(_rewiredPlayer.GetAxis("R_Horz"), 0, _rewiredPlayer.GetAxis("R_Vert"));
        
        //smooth the input vectors
        
        _leftStickHeadingVector = Vector3.SmoothDamp(_leftStickHeadingVector, _leftStickVector.normalized,
            ref _leftStickRefVel, 0.14f, 12.0f);
        
       
        _rightStickHeadingVector = Vector3.SmoothDamp(_rightStickHeadingVector, _rightStickVector.normalized,
            ref _rightStickRefVel, 0.14f, 12.0f);

        //Attack inputs
        _lungeButton = _rewiredPlayer.GetButtonDown("Lunge");
        _spitTimer -= Time.deltaTime;
        _spitButton = _rewiredPlayer.GetButton("Spit");
        _lockButtonHeld = _rewiredPlayer.GetButton("Lock");
    }

    //Called from external scripts that need the heading vectors (TiltModel)
    public Vector3 ReadHeading()
    {
        Vector3 inputVector = _leftStickHeadingVector + _rightStickHeadingVector;

        return inputVector;
    }

    public Vector3 ReadLeftInput()
    {
        return _leftStickVector;
    }

    public Vector3 ReadRightInput()
    {
        return _rightStickVector;
    }

    private void ResetInputs()
    {
        _leftStickVector = Vector3.zero;
        _rightStickVector = Vector3.zero;

        _lungeButton = false;
        _spitButton = false;
        _lockButtonHeld = false;

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
        Vector3 worldForceVectorLeft = MaxForce * transform.TransformVector(_leftStickHeadingVector);
        Vector3 worldForceVectorRight = MaxForce * transform.TransformVector(_rightStickHeadingVector);
        
        Debug.DrawLine(leftWingWorldPoint,leftWingWorldPoint + worldForceVectorLeft,Color.blue); 
        Debug.DrawLine(rightWingWorldPoint,rightWingWorldPoint + worldForceVectorRight,Color.cyan); 
        
        //Apply wing forces
        _rb.AddForceAtPosition(worldForceVectorLeft, leftWingWorldPoint);
        _rb.AddForceAtPosition(worldForceVectorRight, rightWingWorldPoint);
        
        //Calculate Quadratic Wing Drag
        Vector3 leftWingVel = _rb.GetPointVelocity(leftWingWorldPoint);
        Vector3 rightWingVel = _rb.GetPointVelocity(rightWingWorldPoint);
        float leftWingVelFwd = Vector3.Dot(leftWingVel, _rb.transform.right*1);
        float rightWingVelFwd = Vector3.Dot(rightWingVel, _rb.transform.right*-1);
        
        Vector3 leftDragForce = _rb.transform.forward * leftWingVelFwd  * -0.2f;
        Vector3 rightDragForce = _rb.transform.forward * rightWingVelFwd * -0.2f;
        
        //Apply Quadratic Wing drag
        _rb.AddForceAtPosition(leftDragForce, leftWingWorldPoint);
        _rb.AddForceAtPosition(rightDragForce, rightWingWorldPoint);
        Debug.DrawLine(leftWingWorldPoint, leftWingWorldPoint + leftDragForce * 1.4f, Color.red);
        Debug.DrawLine(rightWingWorldPoint, rightWingWorldPoint + rightDragForce * 1.4f, Color.red);
        
        //the wing drag forces were fighting each other, so I turned them off. But that means we need a linear drag
        _rb.AddForce(_rb.velocity.sqrMagnitude * _rb.velocity.normalized * WingDrag);
        
        //Calculate *Linear* Angular Drag
        Vector3 rotationVel = _rb.angularVelocity;
        Vector3 rotationDragForce = rotationVel * QuadAngularDrag;
        
        //Apply Linear Rotational Drag
        _rb.AddTorque(rotationDragForce);
    }

    private void FixRotation()
    {
        Vector3 resetRotation = new Vector3(0, _rb.transform.localRotation.y * Mathf.Rad2Deg, 0);
        _rb.rotation = Quaternion.Euler(resetRotation);
    }

    private void ReturnToNeutralCountdown()
    {
        _stateTimer -= Time.deltaTime;
        if (_stateTimer <= 0)
        {
            StateTransition(MoveState.Neutral, 0);
        }
    }

    private void ReturnToInvulCountdown()
    {
        _stateTimer -= Time.deltaTime;
        if (_stateTimer <= 0)
        {
            StateTransition(MoveState.Invulnerable, InvulnerableTime);
        }
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
        if (_lockButtonHeld)
        {
            _moveState = MoveState.LockOn;
        }
    }

    //Player will have torque applied to them so that they rotate towards their target
    //If the player releases the spit button, they shoot and stop locking on
    private void LockState()
    {
        //If the enemy is out of range, unhighlight the enemy
        if (EnemyInRange() == null && _lockTargetTransform != null)
            _highlightTarget.UnHighlightEnemy(_lockTargetTransform);

        _lockTargetTransform = EnemyInRange(); //check to see if anyone is in range
        
        //If the target enemy switches, unhighlight the enemy  
        if(_lockTargetTransform!=_previousLockTargetTransform && _lockTargetTransform != null)
            _highlightTarget.UnHighlightEnemy(_previousLockTargetTransform);
        
        if (_lockTargetTransform != null) //if yes
        {
            //Calculate target direction
            Vector3 levelledTargetPos = new Vector3(_lockTargetTransform.position.x, transform.position.y, _lockTargetTransform.position.z);
            Vector3 targetDirection = levelledTargetPos - transform.position;
            targetDirection.Normalize();
        
            //Calculate angle between forward facing and target direction
            float angleInDegrees = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);

            //Apply torque, reducing force by the size of the angle
            _rb.AddTorque(transform.up * LockTorque * angleInDegrees);
            _rb.AddTorque(transform.up * _rb.angularVelocity.y * LockDrag);
            
            //Store a reference to the target player transform to un-highlight correctly
            _previousLockTargetTransform = _lockTargetTransform;
            //Highlight the enemy 
            _highlightTarget.HighlightEnemy(_lockTargetTransform);
        }
        
        //Check if you release the button
        LockReleaseCheck();
    }

    private void LockReleaseCheck()
    {
        if (!_lockButtonHeld) //release to return to Neutral
        {
            _highlightTarget.UnHighlightEnemy(_lockTargetTransform);
            StateTransition(MoveState.Neutral, 0);
            
        }
    }

    private void TestSpit()
    {
        if (_spitButton)
        {
            _spitterScript.Spit();
        }
    }

    private void Powerup(Transform powerup)
    {
        if (powerup.CompareTag("Auto"))
        {
            _spitterScript.ChangeState(SpitterScript.SpitState.Auto);
        }
        else if (powerup.CompareTag("Spread"))
        {
            _spitterScript.ChangeState(SpitterScript.SpitState.Spread);
        }
        else if (powerup.CompareTag("Sniper"))
        {
            _spitterScript.ChangeState(SpitterScript.SpitState.Sniper);
        }
    }

    private void Spit()
    {
        if (_spitButton && _spitTimer <= 0)
        {
            //Reset input and timer
            _spitButton = false;
            _spitTimer = SpitTime;
            
            //Instantiate spit and fire it
            GameObject spit = (GameObject) Instantiate(Resources.Load("Prefabs/Players/Spit"));
            spit.transform.position = Spitter.position;
            spit.transform.rotation = Spitter.rotation;
            spit.GetComponent<SpitHitBox>().TeamID = TeamID;
            spit.GetComponent<SpitHitBox>().PlayerID = PlayerID;
            spit.GetComponent<Rigidbody>().AddForce(transform.forward * SpitForce);
            
            //Empty the spit sacs
            _spitSacs.EmptySacs();
            //Refill the spit sacs
            _spitSacs.RefillSacs();
            
            //Play Sound
            MultiAudioManager.PlayAudioObject(SpitSound, transform.position);
        }
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
        if (CheckState() != MoveState.Invulnerable && CheckState() != MoveState.Dead && CheckState() != MoveState.Hatching)
        {
            //Turn on screen shake 
            StartCoroutine(GameManager.GM.CameraShake(PlayerID, GameManager.GM.playerDeathScreenShakeTime));
            
            //Turn off target highlighting on self and enemy target
            _highlightTarget.UnhighlightPlayer();
            _highlightTarget.UnHighlightEnemy(_lockTargetTransform);
            
            //Destroy the old player model index
            Destroy(GetComponentInChildren<PlayerModelIndex>());
            
            _rb.velocity = Vector3.zero;
            _eggHolder.DropEgg();

            //Explode the model
            ExplodeModel[] explodeModels = GetComponentsInChildren<ExplodeModel>();
            foreach (ExplodeModel explodeModel in explodeModels)
            {
                explodeModel.Explode();
            }
            
            //Play Death Sound
            MultiAudioManager.PlayAudioObject(DeathSound, transform.position);
            
            //Transition state
            StateTransition(MoveState.Dead, DeathTime);
        }
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

    #region On Collision/Trigger

    private void OnCollisionEnter(Collision other)
    {
        //Make a sound when bumping into walls
        if (other.transform.CompareTag("Wall"))
        {
            MultiAudioManager.PlayAudioObject(HitWallSound, transform.position);
        }
    }

    #endregion

    #region Archived Functions

    /*
     
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
