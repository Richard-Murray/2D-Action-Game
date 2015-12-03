using UnityEngine;
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

[RequireComponent(typeof(Controller2D))]
public class BaseCharacter : MonoBehaviour
{
    //Core state stuff
    protected OVERRIDESTATE _state = OVERRIDESTATE.MOVEMENT;
    protected POSITIONSTATE _statePosition = POSITIONSTATE.GROUNDED;
    protected BASECHARACTERSACTIONSTATE _stateAction = BASECHARACTERSACTIONSTATE.NONE;

    protected BaseAction _bufferedAction; //the action to be played as soon as the character is able to
    protected BaseAction _currentAction;

    GameObject _lockedTarget;
    DefaultTarget _defaultTarget;

    public bool _locked { get { return (bool)_lockedTarget; } }
    public int _trueDirection {get; private set;}

    float _iFrameTimer = 0;
    bool _invincibleFrames = false;
    float _cancelTimer = 0;
    bool _cancellable = false;
    int _actionEventIndex = 0;
    float _timeIntoAction = 0;
    float _gravityMultiplier = 1;
    float _aerialDegredation = 1;

    [Header("Base Character Attributes")]
    public float _maxHealth;
    public float _maxPoise;
    float _currentHealth;
    float _currentPoise;

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

    public void Initialise()
    {
        _controller = GetComponent<Controller2D>();

        GameObject target = Instantiate((GameObject)Resources.Load("Prefabs/DefaultTarget"));
        target.transform.SetParent(this.transform);
        _defaultTarget = target.GetComponent<DefaultTarget>();
    }


    // Update is called once per frame
    public void Update()
    {
        GetInputStates();
        ProcessCancelAndInvulnFrames();
        UseBufferedAction();
        
        switch(_state)
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

    virtual protected void GetInputStates()
    {
        //Get input vector
        _input = InputManager.Instance._moveAxis;

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
        
        //ifkeyboard
        //_input.x = Mathf.Sign(_input.x);

        //Debug.Log(_controller);
        if(_controller.collisions.below)
        {
            _statePosition = POSITIONSTATE.GROUNDED;
            _jumpsLeft = _numberOfJumps;
        }
        else
        {
            _statePosition = POSITIONSTATE.AERIAL;
        }

        if(InputManager.Instance._RB)
        {
            //_lockedTarget = 
        }
        else
        {
            _lockedTarget = _defaultTarget.gameObject;

            if(right)
            {
                _lockedTarget.transform.position = transform.position + new Vector3(5, 0, 0);
            }
            else if(left)
            {
                _lockedTarget.transform.position = transform.position + new Vector3(-5, 0, 0);
            }
        }

        if(_lockedTarget.transform.position.x >= transform.position.x)
        {
            _trueDirection = 1;
        }
        else
        {
            _trueDirection = -1;
        }

        ProcessInputsToBuffer();
    }
    
    virtual protected void ProcessInputsToBuffer()
    {
        
    }

    void UseBufferedAction()
    {
        if(_bufferedAction != null)
        {
            if(_state == OVERRIDESTATE.MOVEMENT || _cancellable)
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
        else if(_statePosition == POSITIONSTATE.AERIAL)
        {
            _velocity.x += _input.x * _moveSpeed * _accelerationTimeAirborne;
            if(Mathf.Abs(_velocity.x) > _moveSpeed)
            {
                _velocity.x = _moveSpeed * Mathf.Sign(_velocity.x);
            }

            //if(InputManager.Instance._APressed && _jumpsLeft > 0)
            //{
            //    _velocity.y = _jumpVelocity;
            //    _jumpsLeft--;
            //}
        }        

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
        if(_currentAction._groundCancel && _statePosition == POSITIONSTATE.GROUNDED)
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
        if(_cancelTimer > 0)
        {
            _cancellable = false;
            _cancelTimer -= Time.deltaTime;
        }
        else
        {
            _cancellable = true;
        }

        if(_iFrameTimer > 0)
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

//[System.Serializable]
public class BaseAction //READ ONLY
{
    public List<BaseEvent> _eventList;

    public bool _groundCancel;

    public BaseAction()
    {
        _eventList = new List<BaseEvent>();
        _groundCancel = false;
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
}

//[System.Serializable]
public class BaseEvent// : System.Object
{
    public float _triggerTime;

    virtual public void TriggerEvent(BaseCharacter a_character)
    {
        //a_character._velocity = new Vector2(50 * a_character._trueDirection, a_character._velocity.y + 6);
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
        
        if(_useX && !_useY)
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
        else if(_useY && !_useX)
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
        else if(_useX && _useY)
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