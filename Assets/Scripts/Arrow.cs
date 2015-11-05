using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
public class Arrow : Bs
{
    private new Transform transform;
    void Start()
    {
        transform = base.transform;
        if (bullet)
            transform.GetChild(0).localPosition += Vector3.forward * Random.Range(1, 3);
        if (fireObj)
            fireObj.SetActive(fire);
    }
    public float speed = 100;
    //internal Vector3 oldPos;
    public bool bullet;
    internal int damage = 10;
    public GameObject fireObj;
    public bool magic;
    public GameObject blood;
    public bool fire = true;
    public GameObject Wave;
    internal bool enemy;
    void Update()
    {
        //foreach (var b in _Game.zombies)
        //{
        //    if (b.life > 0)
        //        if (ZeroY(b.pos - pos).magnitude < 1)
        //        {
        //            OnHit(b);
        //            return;
        //        }
        //}
        RaycastHit h;

        var viewportPointToRay = camera.ViewportPointToRay(camera.WorldToViewportPoint(pos));
        if (Physics.Raycast(viewportPointToRay, out h, 10000, 1 << (int)Layer.Level | 1 << (int)(enemy ? Layer.Player : Layer.Enemy) | 1 << (int)Layer.bones))
        {
            var rigidbody1 = h.rigidbody;
            var explosionPosition = h.point + h.normal;
            if (rigidbody1 != null && !rigidbody1.isKinematic)
            {
                AddExplosionForce(rigidbody1, 1000, explosionPosition, 500);
                Destroy(gameObject);
            }

            var root = h.transform.root;
            if (root.tag == "Destroyable")
            {
                Game.Destruct(h.transform, explosionPosition);
                Destroy(gameObject);
            }
            if (root.tag == "Enemy")
            {
                Destroy(Wave);
                var d = root.GetComponent<Destroyable>();
                var z = d as Zombie;
                if (z != null)
                {
                    Transform b = z.transforms[Random.Range(0, z.transforms.Length)];
                    OnHit2(z, b.position, -transform.forward, b);
                }
                else
                {
                    d.SetLife(10, pos, 10);
                    Destroy(gameObject);
                }
            }
        }
        
        //oldPos = transform.position;
        if (_Player.lastArrow == this && magic)
            transform.forward += ZeroY(_Player.mousePos - pos).normalized * 3 * Time.deltaTime;
        transform.position += transform.forward * Time.deltaTime * speed;
    }

    

    public float bangRadius = 0;
    public void OnDestroy()
    {
        if (exiting) return;
        if (magic)
            Bs._Game.CreateBlood(pos, Vector3.up, fire, blood);

        if (bangRadius > 0)
        {
            _Game.Explode(pos, bangRadius, damage);
        }

    }

    

    private void OnHit2(Zombie zomb, Vector3 point, Vector3 normal, Transform tr)
    {
        pos = point;
        zomb.transform.position = zomb.pos += (transform.forward * .2f);
        if (!fire)
        {
            zomb.CreateBloodSplatter();
            if (bangRadius == 0)
            {
                //Bs._Game.CreateBlood(point, normal, fire, blood);
                zomb.SetLife(damage, point + normal, 700);
            }
        }

        enabled = false;
        if (bullet || magic)
            Destroy(gameObject);
        else
        {
            transform.position = point;
            transform.forward = -normal;
            transform.parent = tr;
            Destroy(this.gameObject, 5);
            if (fire)
                zomb.StartCoroutine(Bs.AddMethod(.5f, zomb.SetFire));
            Destroy(this);
        }
    }
}
