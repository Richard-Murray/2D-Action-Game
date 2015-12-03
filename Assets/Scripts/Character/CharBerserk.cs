using UnityEngine;
using System.Collections;

public class CharBerserk : BaseCharacter {

	// Use this for initialization
	void Start () {
        Initialise();
	}

    public void Initialise()
    {
        base.Initialise();

        BaseAction _jumpAction = new BaseAction();
        BaseAction _dodgeAction = new BaseAction();
        BaseAction _airY = new BaseAction();
        BaseAction _groundY = new BaseAction();
        BaseAction _groundForwardY = new BaseAction();

        _actionList.Add(_jumpAction);
        _actionList.Add(_dodgeAction);
        _actionList.Add(_airY);
        _actionList.Add(_groundForwardY);
        _actionList.Add(_groundY);

        _jumpAction.SetMoveName("groundJump");
        _jumpAction.AddInput(INPUTTYPE.A);
        _jumpAction.AddEvent(new EventBecomeCancellableAt(0)); //need to zero X velocity
        _jumpAction.AddEvent(new EventSetAerialDegredation(1));
        _jumpAction.AddEvent(new EventSetVelocityWithInputMagnitude(new Vector2(_moveSpeed, 0), true, false));
        _jumpAction.AddEvent(new EventSetYVelocity(_jumpVelocity));
        //_jumpAction.AddEvent(new EventSetVelocityWithInputMagnitude(new Vector2(_moveSpeed, 0), true, true));
        _jumpAction.AddEvent(new EventFinish());

        //_dodgeAction.UseGroundCancel(); //for air version

        _dodgeAction.SetMoveName("airDodge");
        _dodgeAction.AddInput(INPUTTYPE.LT);
        _dodgeAction.AddInput(INPUTTYPE.ANYAXIS);
        _dodgeAction.AddEvent(new EventBecomeCancellableAt(0.1f)); //need to zero X velocity
        _dodgeAction.AddEvent(new EventTurnOnIFrames(0.1f));
        _dodgeAction.AddEvent(new EventSetAerialDegredation(1));
        _dodgeAction.AddEvent(new EventSetVelocityWithInputMagnitude(new Vector2(50, 0), true, true));
        _dodgeAction.AddEvent(new EventSetVelocityOnce(new Vector2(0, 0)), 0.1f);
        _dodgeAction.AddEvent(new EventFinish(), 0.1f);

        _airY.SetMoveName("airY");
        _airY.AddInput(INPUTTYPE.AERIAL);
        _airY.AddInput(INPUTTYPE.Y);
        _airY.UseGroundCancel();
        _airY.AddEvent(new EventBecomeCancellableAt(0.15f));
        _airY.AddEvent(new EventDegradeAerial(0.1f));
        _airY.AddEvent(new EventSetXVelocity(-1.5f));
        _airY.AddEvent(new EventSetYVelocity(5, true));
        _airY.AddEvent(new EventSetGravity(0.6f));
        _airY.AddEvent(new EventCapYVelocity(10));
        _airY.AddEvent(new EventFinish(), 0.1f);

        _groundY.SetMoveName("groundY");
        _groundY.AddInput(INPUTTYPE.GROUNDED);
        _groundY.AddInput(INPUTTYPE.Y);
        _groundY.AddEvent(new EventSetVelocityOnce(new Vector2(2, 0)));
        _groundY.AddEvent(new EventFinish(), 0.15f);

        _groundForwardY.SetMoveName("groundForwardY");
        _groundForwardY.AddInput(INPUTTYPE.GROUNDED);
        _groundForwardY.AddInput(INPUTTYPE.Y);
        _groundForwardY.AddInput(INPUTTYPE.HORIZONTALFORWARD);
        _groundForwardY.AddEvent(new EventBecomeCancellableAt(0.2f));
        _groundForwardY.AddEvent(new EventSetVelocityOnce(new Vector2(20, 0)));
        _groundForwardY.AddEvent(new EventFinish(), 0.2f);
    }
	
	// Update is called once per frame
	void Update () {
        base.Update();
	}

    protected override void ProcessInputsToBuffer()
    {
        base.ProcessInputsToBuffer();
        /*
        //base.ProcessInputsToBuffer();
        bool actionChosen = false;
        if (_statePosition == POSITIONSTATE.GROUNDED)
        {
            if (InputManager.Instance._APressed)
            {
                _bufferedAction = _jumpAction;
                actionChosen = true;
            }
            if (InputManager.Instance._YPressed && InputManager.Instance._RB)
            {
                Debug.Log(InputManager.Instance._right);
                if (InputManager.Instance._right && _trueDirection == 1)
                {
                    _bufferedAction = _groundForwardY;
                    actionChosen = true;
                }
                if (InputManager.Instance._left && _trueDirection == -1)
                {
                    _bufferedAction = _groundForwardY;
                    actionChosen = true;
                }
            }
            if (InputManager.Instance._YPressed && !actionChosen)
            {
                _bufferedAction = _groundY;
                actionChosen = true;
            }
        }
        if (_statePosition == POSITIONSTATE.AERIAL)
        {
            if (InputManager.Instance._APressed && _jumpsLeft > 0)
            {
                _bufferedAction = _jumpAction;
                _jumpsLeft--;
                actionChosen = true;
            }
            if (InputManager.Instance._YPressed)
            {
                _bufferedAction = _airY;
                actionChosen = true;
            }
        }
        if (Input.GetMouseButtonDown(0) && (InputManager.Instance._moveAxis.x != 0 || InputManager.Instance._moveAxis.y != 0))
        {
            _bufferedAction = _dodgeAction;
            actionChosen = true;
        }

        */
    }
}
