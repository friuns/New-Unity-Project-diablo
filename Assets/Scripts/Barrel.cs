using UnityEngine;
using System.Collections;

public class Barrel : Destroyable
{
    public void Start()
    {
        _Game.destroyables.Add(this);
    }
    public override void SetLife(float damage, Vector3 h, float forse = 100, bool knockout = false)
    {
        Explode();
    }
    public override void Die(Vector3 h, float forse)
    {
        Explode();
    }
    private void Explode()
    {
        if (!enabled) return;
        enabled = false;
        exec = true;
        Instantiate(_Database.explosionPrefab, upPos, Quaternion.identity);
        var pos = this.pos;
        pos.y = Mass;
        this.pos = pos;
        transform.rotation = Quaternion.identity;
        foreach (Transform a in transform.GetChild(0))
            a.gameObject.SetActive(!a.gameObject.activeSelf);
        transform.GetChild(0).animation.Play();
        Destroy(rigidbody);
        _Game.Explode(pos, 5, 100, true);
    }

    private bool exec;
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.impactForceSum.magnitude > 7)
            if (!exec)
            {
                if (collision.collider.gameObject.layer == (int) Layer.Level)
                {
                    exec = true;
                    StartCoroutine(AddMethod(.5f, Explode));
                }
                if (collision.collider.gameObject.layer == (int)Layer.Enemy)
                    Explode();
            }
    }
}
