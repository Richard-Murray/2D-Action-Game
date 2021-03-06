﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum POSITIONSTATE
{
    GROUNDED,
    AERIAL
}

public enum OVERRIDESTATE
{
    MOVEMENT,
    ACTION,
    INTERRUPT,
}

public enum BASECHARACTERSACTIONSTATE //one-use for debugging, every enemy will have their own set of states
{
    NONE,
    GROUNDY,
    AIRY
}

public enum INPUTTYPE
{
    GROUNDED,
    AERIAL,
    A,
    B,
    X,
    Y,
    RB,
    RT,
    LB,
    LT,
    HORIZONTALAXIS, //any use of the horizontal axis
    VERTICALAXIS, //any use of the vertical axis
    ANYAXIS, //any use of the left thumbstick/movement axis
    HORIZONTALFORWARD,
    VERTICALUP,
    VERTICALDOWN,
    AIMING //any use of the right thumbstick
}

[RequireComponent(typeof(Controller2D))]
public class BaseCharacter : MonoBehaviour
{
    //Core state stuff
    protected bool _playerControlled = false;

    protected OVERRIDESTATE _state = OVERRIDESTATE.MOVEMENT;
    protected POSITIONSTATE _statePosition = POSITIONSTATE.GROUNDED;
    protected BASECHARACTERSACTIONSTATE _stateAction = BASECHARACTERSACTIONSTATE.NONE;

    protected List<BaseAction> _actionList;
    protected List<BaseAction> _interruptList; //TODO
    protected List<INPUTTYPE> _inputList;

    protected BaseAction _bufferedAction; //the action to be played as soon as the character is able to
    protected BaseAction _currentAction;

    //may not be needed
    GameObject _lockedTarget;
    DefaultTarget _defaultTarget;

    public bool _locked { get { return (bool)_lockedTarget; } }
    public int _trueDirection { get; private set; }

    float _iFrameTimer = 0;
    bool _invincibleFrames = false;
    float _cancelTimer = 0;
    bool _cancellable = false;
    int _actionEventIndex = 0;
    float _timeIntoAction = 0;
    float _gravityMultiplier = 1;
    float _aerialDegredation = 1;

    [Header("Base Character Attributes")]
    public float _HPMax;
    float _HPCurrent;
    public float _poiseMax;
    float _poiseCurrent;

    [Header("Base Movement")]
    public float _moveSpeed = 4;
    public float _gravity;
    public float _jumpVelocity;
    public float _accelerationTimeAirborne = 0.05f;
    public int _numberOfJumps;
    protected int _jumpsLeft;
    //    float _accelerationTimeGrounded = 0.1f;

    [HideInInspector]
    public Vector3 _velocity;
    Vector2 _input;
    float _velocityXSmoothing;

    [HideInInspector]
    public Controller2D _controller;

    // Use this for initialization
    void Start()
    {
        Initialise();
    }

    //Custom characters overload initialise to build their behaviours (this will be moved to exterior class)
    //PLAN FOR LATER, every update will be in its own overriden function, base class everything inherits from has the state controller
    virtual public void Initialise()
    {
        _controller = GetComponent<Controller2D>();

        _actionList = new List<BaseAction>();
        _interruptList = new List<BaseAction>(); //
        _inputList = new List<INPUTTYPE>();

        GameObject target = Instantiate((GameObject)Resources.Load("Prefabs/DefaultTarget"));
        target.transform.SetParent(this.transform);
        _defaultTarget = target.GetComponent<DefaultTarget>();

        _HPCurrent = _HPMax;
        _poiseCurrent = _poiseMax;
    }

    public void Update()
    {
        GetInputStates();
        ProcessInputsToBuffer();
        ProcessCancelAndInvulnFrames();
        UseBufferedAction();

        switch (_state)
        {
            case OVERRIDESTATE.MOVEMENT:
                {
                    ProcessMovement();
                    break;
                }
            case OVERRIDESTATE.ACTION:
                {
                    ProcessAction();
                    break;
                }
            case OVERRIDESTATE.INTERRUPT:
                {
                    break;
                }
        }

        //Send move command through to the controller2D
        _velocity.y += _gravity * Time.deltaTime * _gravityMultiplier;
        _controller.Move(_velocity * Time.deltaTime);
    }

    //Core functions
    virtual protected void GetInputStates()
    {
        //Get input vector
        _input = InputManager.Instance._moveAxis;

        //detect all current inputs being used
        _inputList.Clear();

        if (_controller.collisions.below)
        {
            _inputList.Add(INPUTTYPE.GROUNDED);
            _statePosition = POSITIONSTATE.GROUNDED;
            _jumpsLeft = _numberOfJumps;
        }
        else
        {
            _inputList.Add(INPUTTYPE.AERIAL);
            _statePosition = POSITIONSTATE.AERIAL;
        }

        if (_playerControlled)
        {
            ProcessPlayerInput();
        }

        if (_controller.collisions.below)
        {
        }
        else
        {
        }
    }

    virtual protected void ProcessPlayerInput()
    {
        if (_input.x > 0 || _input.x < 0)
        {
            _inputList.Add(INPUTTYPE.HORIZONTALAXIS);
        }
        if (_input.y > 0 || _input.y < 0)
        {
            _inputList.Add(INPUTTYPE.VERTICALAXIS);
        }
        if (_inputList.Contains(INPUTTYPE.HORIZONTALAXIS) || _inputList.Contains(INPUTTYPE.VERTICALAXIS))
        {
            _inputList.Add(INPUTTYPE.ANYAXIS);
        }
        if (_input.x > InputManager.Instance._directionRegisterDeadzone || _input.x < -InputManager.Instance._directionRegisterDeadzone)
        {
            _inputList.Add(INPUTTYPE.HORIZONTALFORWARD);
        }
        if (InputManager.Instance._APressed)
        {
            _inputList.Add(INPUTTYPE.A);
        }
        if (InputManager.Instance._BPressed)
        {
            _inputList.Add(INPUTTYPE.B);
        }
        if (InputManager.Instance._XPressed)
        {
            _inputList.Add(INPUTTYPE.X);
        }
        if (InputManager.Instance._YPressed)
        {
            _inputList.Add(INPUTTYPE.Y);
        }
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            _inputList.Add(INPUTTYPE.LT);
        }

        bool left = false;
        bool right = false;
        if (_state == OVERRIDESTATE.MOVEMENT)
        {
            if (_input.x > 0)
            {
                right = true;
            }
            else if (_input.x < 0)
            {
                left = true;
            }
            else
            {
                if (_trueDirection == 1)
                {
                    right = true;
                }
                if (_trueDirection == -1)
                {
                    left = true;
                }
            }
        }


        //targetting stuff, may not use
        if (InputManager.Instance._RB)
        {
            //_lockedTarget = 
        }
        else
        {
            _lockedTarget = _defaultTarget.gameObject;

            if (right)
            {
                _lockedTarget.transform.position = transform.position + new Vector3(5, 0, 0);
            }
            else if (left)
            {
                _lockedTarget.transform.position = transform.position + new Vector3(-5, 0, 0);
            }
        }

        if (_lockedTarget.transform.position.x >= transform.position.x)
        {
            _trueDirection = 1;
        }
        else
        {
            _trueDirection = -1;
        }
    }

    virtual protected void ProcessInputsToBuffer()
    {
        bool actionChosen = false;
        for (int i = 0; i < _actionList.Count; i++)
        {
            if (!actionChosen)
            {
                bool inputsMatch = true;
                if(_actionList[i]._inputAccessible == false) //Checks if the move is 'accessible', therefore not using it (useful when it has no inputs specified)
                {
                    inputsMatch = false;
                }
                for (int ii = 0; ii < _actionList[i]._inputsRequired.Count; ii++)
                {
                    if (!_inputList.Contains(_actionList[i]._inputsRequired[ii]))
                    {
                        inputsMatch = false;
                    }
                }
                if (inputsMatch)
                {
                    actionChosen = true;
                    _bufferedAction = _actionList[i];
                }
            }
            else
            {
                break;
            }
        }
    }

    void UseBufferedAction()
    {
        if (_bufferedAction != null)
        {
            if (_state == OVERRIDESTATE.MOVEMENT || _cancellable)
            {
                _actionEventIndex = 0;
                _timeIntoAction = 0;
                _currentAction = _bufferedAction;
                _bufferedAction = null;

                _state = OVERRIDESTATE.ACTION; //not necessarily, could be a 'passive' action

            }
        }
    }

    virtual protected void ProcessMovement()
    {
        SetGravityIntensity(1);

        if (_controller.collisions.above || _controller.collisions.below)
        {
            _velocity.y = 0;
        }

        //        if (_playerControlled)
        //        {
        if (_statePosition == POSITIONSTATE.GROUNDED)
        {
            SetAerialDegredation(1);
            //Calculate x velocity
            _velocity.x = _input.x * _moveSpeed; // Mathf.SmoothDamp(_velocity.x, targetVelocityX, ref _velocityXSmoothing, (controller.collisions.below) ? _accelerationTimeGrounded : _accelerationTimeAirborne);

            //Takes the platform's velocity into account when jumping
            //if (InputManager.Instance._APressed)
            //{
            //    _velocity.y = _jumpVelocity + _controller.collisions.platformVelocity.y;
            //    _velocity.x += _controller.collisions.platformVelocity.x;
            //    //targetVelocityX = _controller.collisions.platformVelocity.x;
            //}
        }
        else if (_statePosition == POSITIONSTATE.AERIAL)
        {
            _velocity.x += _input.x * _moveSpeed * _accelerationTimeAirborne;
            if (Mathf.Abs(_velocity.x) > _moveSpeed)
            {
                _velocity.x = _moveSpeed * Mathf.Sign(_velocity.x);
            }

            //if(InputManager.Instance._APressed && _jumpsLeft > 0)
            //{
            //    _velocity.y = _jumpVelocity;
            //    _jumpsLeft--;
            //}
        }
        //        }
        //        else
        //        {
        //            if(_statePosition == POSITIONSTATE.GROUNDED)
        //            {
        //                _velocity.x = 0;
        //            }
        //        }

        //this may go under grounded positionstate
        if (((_velocity.x < 0 && _controller.collisions.left) || (_velocity.x > 0 && _controller.collisions.right)))
        {
            _velocity.x = 0;
        }
    }

    virtual protected void ProcessAction()
    {
        _timeIntoAction += Time.deltaTime;
        bool actionsLeftThisFrame = true;
        if (_currentAction._groundCancel && _statePosition == POSITIONSTATE.GROUNDED)
        {
            Cancel();
            actionsLeftThisFrame = false;
        }

        while (actionsLeftThisFrame)
        {
            if (_currentAction._eventList.Count > _actionEventIndex)
            {
                if (_timeIntoAction > _currentAction._eventList[_actionEventIndex]._triggerTime)
                {
                    _currentAction._eventList[_actionEventIndex].TriggerEvent(this);
                    _actionEventIndex++;
                }
                else
                {
                    actionsLeftThisFrame = false;
                }
            }
            else
            {
                actionsLeftThisFrame = false;
                _state = OVERRIDESTATE.MOVEMENT;
                _stateAction = BASECHARACTERSACTIONSTATE.NONE;
            }
        }
    }

    public void ProcessCancelAndInvulnFrames()
    {
        if (_cancelTimer > 0)
        {
            _cancellable = false;
            _cancelTimer -= Time.deltaTime;
        }
        else
        {
            _cancellable = true;
        }

        if (_iFrameTimer > 0)
        {
            _invincibleFrames = true;
            _iFrameTimer -= Time.deltaTime;
        }
        else
        {
            _invincibleFrames = false;
        }
    }

    public void Cancel()
    {
        _cancellable = false;
        _cancelTimer = 0;
        _invincibleFrames = false;
        _iFrameTimer = 0;
        SetGravityIntensity(1);
        _state = OVERRIDESTATE.MOVEMENT;
        _stateAction = BASECHARACTERSACTIONSTATE.NONE;
    }

    public void DistributeDamageInformation(DamageInformation a_damageInfo)
    {
        if(a_damageInfo._cancels)
        {
            Cancel();
        }
        _HPCurrent -= a_damageInfo._HP;
        _poiseCurrent -= a_damageInfo._poise;

        if(_HPCurrent < 0)
        {
            Destroy(_defaultTarget.gameObject);
            Destroy(this.gameObject);
        }
    }

    //Single-use functions
    public void CancellableAt(float a_time)
    {
        _cancellable = false;
        _cancelTimer = a_time;
    }

    public void AddIFrames(float a_time)
    {
        _invincibleFrames = true;
        _iFrameTimer = a_time;
    }

    public void SetGravityIntensity(float a_gravityMultiplier)
    {
        _gravityMultiplier = a_gravityMultiplier;
    }

    public void SetAerialDegredation(float a_aerialDegredation)
    {
        _aerialDegredation = a_aerialDegredation;
    }

    public void DegradeAerials(float a_aerialDegredation)
    {
        _aerialDegredation -= a_aerialDegredation;
        if (_aerialDegredation < 0)
            _aerialDegredation = 0;
    }

    public float GetAerialDegredation()
    {
        return _aerialDegredation;
    }
}

public struct DamageInformation
{
    public float _HP;
    public float _poise;
    public bool _cancels;

    public DamageInformation(float a_HP, float a_poise, bool a_cancels = true)
    {
        _HP = a_HP;
        _poise = a_poise;
        _cancels = a_cancels;
    }
}

//[System.Serializable]
public class BaseAction //READ ONLY
{
    public List<BaseEvent> _eventList;
    public List<INPUTTYPE> _inputsRequired;

    public bool _inputAccessible;
    public bool _groundCancel;

    public string _actionName;

    public BaseAction()
    {
        _eventList = new List<BaseEvent>();
        _inputsRequired = new List<INPUTTYPE>();
        _groundCancel = false;
        _inputAccessible = true;
    }

    public void UseGroundCancel()
    {
        _groundCancel = true;
    }

    public void AddEvent(BaseEvent a_event, float a_timeToTrigger = 0)
    {
        a_event.SetTriggerTime(a_timeToTrigger);
        _eventList.Add(a_event);
    }

    public void AddInput(INPUTTYPE a_inputType)
    {
        _inputsRequired.Add(a_inputType);
    }

    public void AddPreviousMoveInput(string a_moveName)
    {
        //does nothing yet
    }

    public void SetMoveName(string a_name)
    {
        _actionName = a_name;
    }

    public void SetAccessibility(bool a_playerAccessible)
    {
        _inputAccessible = a_playerAccessible;
    }
}

//[System.Serializable]
public class BaseEvent// : System.Object
{
    public float _triggerTime;

    virtual public void TriggerEvent(BaseCharacter a_character)
    {

    }

    public void SetTriggerTime(float a_time)
    {
        _triggerTime = a_time;
    }
}

public class EventBecomeCancellableAt : BaseEvent
{
    public float _time;

    public EventBecomeCancellableAt(float a_time)
    {
        _time = a_time;
    }

    public override void TriggerEvent(BaseCharacter a_character)
    {
        a_character.CancellableAt(_time);
    }
}

public class EventSetVelocityOnce : BaseEvent
{
    public Vector2 _velocity;

    public EventSetVelocityOnce(Vector2 a_velocity)
    {
        _velocity = a_velocity;
    }

    public override void TriggerEvent(BaseCharacter a_character)
    {
        Vector2 finalVelocity = _velocity;
        finalVelocity.x *= a_character._trueDirection;
        //finalVelocity.y += a_character._velocity.y; //
        a_character._velocity = finalVelocity;
    }
}

public class EventSetXVelocity : BaseEvent
{
    public float _XVelocity;

    public EventSetXVelocity(float a_XVelocity)
    {
        _XVelocity = a_XVelocity;
    }

    public override void TriggerEvent(BaseCharacter a_character)
    {
        a_character._velocity.x = _XVelocity * a_character._trueDirection;
    }
}

public class EventSetVelocityWithInputMagnitude : BaseEvent
{
    public Vector2 _velocity;
    public bool _useX;
    public bool _useY;
    public int _relative;

    public EventSetVelocityWithInputMagnitude(Vector2 a_velocity, bool a_useX = true, bool a_useY = true, int a_relative = 0)
    {
        _velocity = a_velocity;
        _useX = a_useX;
        _useY = a_useY;
        _relative = a_relative;
    }

    public override void TriggerEvent(BaseCharacter a_character)
    {

        if (_useX && !_useY)
        {
            if (InputManager.Instance._moveAxis.x != 0)
            {
                a_character._velocity.x = Mathf.Sign(InputManager.Instance._moveAxis.x) * _velocity.x + (a_character._velocity.x * _relative);
            }
            else
            {
                a_character._velocity.x = 0;
            }
        }
        else if (_useY && !_useX)
        {
            if (InputManager.Instance._moveAxis.y != 0)
            {
                a_character._velocity.y = Mathf.Sign(InputManager.Instance._moveAxis.y) * _velocity.y + (a_character._velocity.y * _relative);
            }
            else
            {
                a_character._velocity.y = 0;
            }
        }
        else if (_useX && _useY)
        {
            Vector3 finalVelocity = new Vector3(InputManager.Instance._moveAxis.x, InputManager.Instance._moveAxis.y, 0);
            //Debug.Log(finalVelocity);
            finalVelocity = finalVelocity.normalized * _velocity.x;
            //Debug.Log(finalVelocity);
            a_character._velocity = finalVelocity;
        }

    }
}

public class EventSetYVelocityRelative : BaseEvent
{
    public float _YVelocity;

    public EventSetYVelocityRelative(float a_YVelocity)
    {
        _YVelocity = a_YVelocity;
    }

    public override void TriggerEvent(BaseCharacter a_character)
    {
        a_character._velocity.y += _YVelocity;
    }
}

public class EventSetYVelocity : BaseEvent
{
    public float _YVelocity;
    public bool _degradedAerial;

    public EventSetYVelocity(float a_YVelocity, bool a_degradedAerial = false)
    {
        _YVelocity = a_YVelocity;
        _degradedAerial = a_degradedAerial;
    }

    public override void TriggerEvent(BaseCharacter a_character)
    {
        if (_degradedAerial)
        {
            a_character._velocity.y = _YVelocity * a_character.GetAerialDegredation();
        }
        else
        {
            a_character._velocity.y = _YVelocity;
        }
    }
}

public class EventSetAerialDegredation : BaseEvent
{
    public float _aerialDegredation;

    public EventSetAerialDegredation(float a_aerialDegredation)
    {
        _aerialDegredation = a_aerialDegredation;
    }

    public override void TriggerEvent(BaseCharacter a_character)
    {
        a_character.SetAerialDegredation(_aerialDegredation);
    }
}

public class EventDegradeAerial : BaseEvent
{
    public float _aerialDegredation;

    public EventDegradeAerial(float a_aerialDegredation)
    {
        _aerialDegredation = a_aerialDegredation;
    }

    public override void TriggerEvent(BaseCharacter a_character)
    {
        a_character.DegradeAerials(_aerialDegredation);
    }
}

public class EventSetGravity : BaseEvent
{
    public float _gravityMultiplier;

    public EventSetGravity(float a_gravityMultiplier)
    {
        _gravityMultiplier = a_gravityMultiplier;
    }

    public override void TriggerEvent(BaseCharacter a_character)
    {
        a_character.SetGravityIntensity(_gravityMultiplier);
    }
}

public class EventCapYVelocity : BaseEvent
{
    public float _capY;

    public EventCapYVelocity(float a_cap)
    {
        _capY = a_cap;
    }

    public override void TriggerEvent(BaseCharacter a_character)
    {
        if (a_character._velocity.y > _capY)
            a_character._velocity.y = _capY;
    }
}

public class EventTurnOnIFrames : BaseEvent
{
    public float _iFrameTime;

    public EventTurnOnIFrames(float a_iFrameTime)
    {
        _iFrameTime = a_iFrameTime;
    }

    public override void TriggerEvent(BaseCharacter a_character)
    {
        a_character.AddIFrames(_iFrameTime);
    }
}

public class EventFinish : BaseEvent
{
    public EventFinish()
    {

    }

    public override void TriggerEvent(BaseCharacter a_character)
    {
        a_character.Cancel();
    }
}

//public class EventSetVelocity : public BaseEvent
//{

//}


//code for later possibly


//Zero velocities if there is a collision on that axis to prevent velocities increasing during collision
//TODO: THIS MAY KILL WALLJUMPING BUT IS IMPORTANT FOR FEEL
/*if ((controller.collisions.left && velocity.x < 0) || (controller.collisions.right && velocity.x > 0)) 
    velocity.x = 0;*/
//velocity.x = velocity.x + controller.collisions.platformVelocity.x;