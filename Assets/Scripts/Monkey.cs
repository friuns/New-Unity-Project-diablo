using UnityEngine;
using System.Collections;

public class Monkey : Shared
{
    private CharacterController controler;
	public void Start ()
	{
	    controler = GetComponent<CharacterController>();
        Destroy(gameObject, 3);
        _Game.monkeys.Add(this);
	}

    public float speed = 10;
    public Vector3 vel;
    void Update()
    {
        //vel *= .96f;
        vel = Vector3.Lerp(vel, Vector3.zero, Time.deltaTime*10);
        controler.Move((transform.forward * speed + vel+Vector3.down*5)*Time.deltaTime);
            if (ZeroY(controler.velocity) != Vector3.zero)
                transform.forward = ZeroY(controler.velocity);
    }
    public void OnDestroy()
    {
        if (_Game == null) return;
        _Game.monkeys.Remove(this);
        Instantiate(_Database.explosionPrefab, pos + Vector3.up, Quaternion.identity);
        //Instantiate(_Game.blood, pos + Vector3.up, Quaternion.identity);
        _Game.Explode(pos, 5, 150);
    }
}
