using UnityEngine;
using System.Collections;

public class MegaProjectile : MonoBehaviour {

    protected GameObject _originObject;
    Vector2 _velocity;
    float _rotation;

	// Use this for initialization
	void Start () {

	}

    public void Initialise(GameObject a_originObject, Vector2 a_position, Vector2 a_velocity)
    {
        _originObject = a_originObject;
        transform.position = a_position;
        _velocity = a_velocity;
    }
	
	// Update is called once per frame
	void Update () {
        transform.position += new Vector3(_velocity.x, _velocity.y, 0) * Time.deltaTime;
	}

    void OnCollisionEnter2D()
    {
        Debug.Log("collision");
        Destroy(this.gameObject);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject != _originObject && collider.gameObject.layer != 12)
        {
            //if hits enemy object deal damage
            BaseCharacter characterHit = collider.GetComponent<BaseCharacter>();
            if(characterHit)
            {
                DamageInformation damage = new DamageInformation(10, 0);
                characterHit.DistributeDamageInformation(damage);
            }
            Destroy(this.gameObject);
        }
    }
}
