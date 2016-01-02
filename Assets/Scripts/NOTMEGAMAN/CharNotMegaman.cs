using UnityEngine;
using System.Collections;

public class CharNotMegaman : BaseCharacter
{
    [Header("NotMegaman Attributes")]

    [SerializeField]
    GameObject _projectile;

    public override void Initialise()
    {
        base.Initialise();

        _playerControlled = true;

        BaseAction _jumpAction = new BaseAction();
        BaseAction _airY = new BaseAction();
        BaseAction _groundY = new BaseAction();

        _actionList.Add(_jumpAction);
        _actionList.Add(_airY);
        _actionList.Add(_groundY);

        _jumpAction.SetMoveName("groundJump");
        _jumpAction.AddInput(INPUTTYPE.A);
        _jumpAction.AddInput(INPUTTYPE.GROUNDED); //making it only on the ground
        _jumpAction.AddEvent(new EventBecomeCancellableAt(0)); //need to zero X velocity
        _jumpAction.AddEvent(new EventSetAerialDegredation(1));
        _jumpAction.AddEvent(new EventSetVelocityWithInputMagnitude(new Vector2(_moveSpeed, 0), true, false));
        _jumpAction.AddEvent(new EventSetYVelocity(_jumpVelocity));
        //_jumpAction.AddEvent(new EventSetVelocityWithInputMagnitude(new Vector2(_moveSpeed, 0), true, true));
        _jumpAction.AddEvent(new EventFinish());

        _airY.SetMoveName("airY");
        _airY.AddInput(INPUTTYPE.AERIAL);
        _airY.AddInput(INPUTTYPE.Y);
        _airY.UseGroundCancel();
        _airY.AddEvent(new EventBecomeCancellableAt(0.2f));
        _airY.AddEvent(new EventDegradeAerial(0.1f));
        _airY.AddEvent(new EventSetXVelocity(-1.5f));
        _airY.AddEvent(new EventSetYVelocity(3, true));
        _airY.AddEvent(new EventSetGravity(0.6f));
        _airY.AddEvent(new EventFireMegaProjectile(new Vector2(0.5f, 0), 7));
        _airY.AddEvent(new EventCapYVelocity(10));
        _airY.AddEvent(new EventFinish(), 0.2f);

        _groundY.SetMoveName("groundY");
        _groundY.AddInput(INPUTTYPE.GROUNDED);
        _groundY.AddInput(INPUTTYPE.Y);
        _groundY.AddEvent(new EventBecomeCancellableAt(0.2f));
        _groundY.AddEvent(new EventSetXVelocity(0));
        _groundY.AddEvent(new EventFireMegaProjectile(new Vector2(0.5f, 0), 7));
        _groundY.AddEvent(new EventFinish(), 0);
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }

    public void FireProjectile(Vector2 a_origin, float a_speed)
    {
        Debug.Log("fired");
        GameObject projectile = Instantiate(_projectile);
        MegaProjectile projectileScript = projectile.GetComponent<MegaProjectile>();
        projectileScript.Initialise(this.gameObject, transform.position + new Vector3(a_origin.x * _trueDirection, a_origin.y, 0), new Vector2(_trueDirection * a_speed, 0));
    }
}

public class EventFireMegaProjectile : BaseEvent
{
    public Vector2 _origin;
    public float _speed;

    public EventFireMegaProjectile(Vector2 a_origin, float a_speed)
    {
        _origin = a_origin;
        _speed = a_speed;
    }

    public override void TriggerEvent(BaseCharacter a_character)
    {
        (a_character as CharNotMegaman).FireProjectile(_origin, _speed);
    }
}