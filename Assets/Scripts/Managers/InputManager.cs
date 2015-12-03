using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour {

    public static InputManager Instance { get; private set; }

    public float _directionRegisterDeadzone;

    public Vector2 _moveAxis{ get; private set; }

    public Vector2 _normalMoveAxis { get { return _moveAxis.normalized; } }

    public Vector2 _lookAxis { get; private set; }

    public bool _left               = false;
    public bool _leftPressed        = false;
    public bool _right              = false;
    public bool _rightPressed       = false;
    public bool _up                 = false;
    public bool _upPressed          = false;
    public bool _down               = false;
    public bool _downPressed        = false;

    public bool _dPadLeft           = false;
    public bool _dPadLeftPressed    = false;
    public bool _dPadRight          = false;
    public bool _dPadRightPressed   = false;
    public bool _dPadUp             = false;
    public bool _dPadUpPressed      = false;
    public bool _dPadDown           = false;
    public bool _dPadDownPressed    = false;

    public bool _Y;
    public bool _YPressed;
    public bool _X;
    public bool _XPressed;
    public bool _B;
    public bool _BPressed;
    public bool _A;
    public bool _APressed;

    public bool _LT { get; private set; }
    public bool _LB { get; private set; }
    public bool _RT { get; private set; }
    public bool _RB { get; private set; }

    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        _moveAxis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (_moveAxis.x > _directionRegisterDeadzone && !(_moveAxis.x < -_directionRegisterDeadzone)) //intending to use customizeable keys, so this currently appears to be awkward
            _right = true;
        else
            _right = false;

        if (_moveAxis.x < -_directionRegisterDeadzone && !(_moveAxis.x > _directionRegisterDeadzone))
            _left = true;
        else
            _left = false;

        if (_moveAxis.y > _directionRegisterDeadzone && !(_moveAxis.y < -_directionRegisterDeadzone))
            _up = true;
        else
            _up = false;

        if (_moveAxis.y < -_directionRegisterDeadzone && !(_moveAxis.y > _directionRegisterDeadzone))
            _down = true;
        else
            _down = false;

        if(Input.GetKey(KeyCode.Space))
        {
            if (!_A)
                _APressed = true;
            else
                _APressed = false;
            _A = true;
        }
        else
        {
            _A = false;
            _APressed = false;
        }

        if (Input.GetKey(KeyCode.E))
        {
            if (!_Y)
                _YPressed = true;
            else
                _YPressed = false;
            _Y = true;
        }
        else
        {
            _Y = false;
            _YPressed = false;
        }

        if(Input.GetKey(KeyCode.LeftShift))
        {
            _RB = true;
        }
        else
        {
            _RB = false;
        }

	}    
}

public struct InputType
{

}