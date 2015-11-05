//using System.Linq;
//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using System.Collections;
//using Random = UnityEngine.Random;
//public class Zombie2 : Shared
//{
//    public bool run;
    
//    private float randomRot;

//    //internal Collider trigger;
//    public Vector3 targetPos;
//    public static int AvatarRange = 30;
//    private float time;
//    internal NavMeshAgent controller;
//    private Vector3 moveDir;
//    public Player enemy;
//    private bool oldDmgAnim;
//    internal Animator anim;
//    private SkinnedMeshRenderer skinnedMeshRenderer;
//    private DeadMaterial deadMaterial;
//    private Transform rootBone;
//    //public int id;
//    private float visTime;

//    public float animSpeed = 1;
//    public float speed;
//    public bool runing;
    
//    public WeaponSettings[] weapons;
//    internal WeaponSettings weapon;
//    internal GameObject weaponObj;
    
    

//    public void Start()
//    {
//        InitModel();
//        Init3();
//    }
//    private void InitModel()
//    {
//        maxlife = life;
//        _Game.zombies.Add(this);
//        var componentInChildren = transform.GetComponentInChildren<SkinnedMeshRenderer>();
//        if (componentInChildren == null)
//        {
//            var g = (GameObject)Instantiate(this.modelSkin, transform.position, transform.rotation);
//            g.transform.parent = transform;
//        }
//        anim = GetComponentInChildren<Animator>();
//        anim.SetLayerWeight(1, 1);
        
//        model = anim.transform;
//        //trigger = GetComponent<BoxCollider>();
//        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
//        deadMaterial = GetComponentInChildren<DeadMaterial>();
//        skinnedMeshRenderer.gameObject.AddComponent<Forward>().receiver = transform;
//        //if (_Loader.optState == OptState.rig)
//        //    colliders = transform.GetComponentsInChildren<Collider>();
//    }
//    List<CRC> crcs = new List<CRC>();
//    Dictionary<string, Rigidbody> rgs = new Dictionary<string, Rigidbody>();
    
//    public void Save(Transform t)
//    {
//        CRC cr = new CRC();
//        cr.t = t;
        
//        var j = t.GetComponent<CharacterJoint>();
//        if (j != null)
//            cr.CharacterJoint2 = new CharacterJoint2() { connectedBody = j.connectedBody.name, anchor = j.anchor, axis = j.axis, highTwistLimit = j.highTwistLimit, lowTwistLimit = j.lowTwistLimit, rotationDrive = j.rotationDrive, swing1Limit = j.swing1Limit, swing2Limit = j.swing2Limit, swingAxis = j.swingAxis, targetAngularVelocity = j.targetAngularVelocity, targetRotation = j.targetRotation };
//        Destroy(j);

//        var r = t.rigidbody;
//        if (r != null)
//        {
//            var rigidbody2 = new Rigidbody2() {mass = r.mass};
//            cr.Rigidbody2 = rigidbody2;
//        }
//        Destroy(r);

//        var c = t.collider;
//        var bc = c as BoxCollider;
//        if (bc!=null)
//            cr.BoxCollider2 = new BoxCollider2() { center = bc.center, extents = bc.extents };
//        var sc = c as SphereCollider;
//        if (sc != null)
//            cr.SphereCollider2 = new SphereCollider2() { center = sc.center, radius = sc.radius };
//        var cc = c as CapsuleCollider;
//        if (cc != null)
//            cr.CapsuleCollider2 = new CapsuleCollider2() { center = cc.center, direction = cc.direction, height = cc.height, radius = cc.radius };
//        Destroy(c);

//        crcs.Add(cr);
//    }
//    public void Load()
//    {
//        foreach (var cr in crcs)
//        {
//            var g = cr.t.gameObject;
//            if (cr.Rigidbody2 != null)
//            {
//                var r = g.AddComponent<Rigidbody>();
//                r.mass = cr.Rigidbody2.mass;
//                rgs.Add(g.name, r);
//            }
//            if (cr.CharacterJoint2 != null)
//            {
//                var a = g.AddComponent<CharacterJoint>();
//                var b = cr.CharacterJoint2;
//                a.anchor = b.anchor;
//                a.axis = b.axis;
//                a.connectedBody = rgs[b.connectedBody];
//                a.highTwistLimit = b.highTwistLimit;
//                a.lowTwistLimit = b.lowTwistLimit;
//                a.rotationDrive = b.rotationDrive;
//                a.swing1Limit = b.swing1Limit;
//                a.swing2Limit = b.swing2Limit;
//                a.swingAxis = b.swingAxis;
//                a.targetAngularVelocity = b.targetAngularVelocity;
//                a.targetRotation = b.targetRotation;
//            }
//            if (cr.BoxCollider2 != null)
//            {
//                var a = g.AddComponent<BoxCollider>();
//                a.center = cr.BoxCollider2.center;
//                a.extents = cr.BoxCollider2.extents;                
//            }
//            if (cr.SphereCollider2 != null)
//            {
//                var a = g.AddComponent<SphereCollider>();
//                a.center = cr.SphereCollider2.center;
//                a.radius = cr.SphereCollider2.radius;
//            }
//            if (cr.CapsuleCollider2 != null)
//            {
//                var a = g.AddComponent<CapsuleCollider>();
//                a.center = cr.CapsuleCollider2.center;
//                a.direction = cr.CapsuleCollider2.direction;
//                a.height = cr.CapsuleCollider2.height;
//                a.radius = cr.CapsuleCollider2.radius;
//            }
//        }
//    }
//    private void Init3()
//    {
//        foreach (Transform a in model)
//            if (skinnedMeshRenderer.transform != a)
//                rootBone = a;
//        var p = Random.insideUnitSphere;
//        p.y = 0;
//        targetPos = p * AvatarRange;
//        //var rigidBodies = transform.GetComponentsInChildren<Rigidbody>(); //todo Zombie.Init3:166 
//        //transforms = model.GetComponentsInChildren<Transform>();
//        //colliders = transform.GetComponentsInChildren<Collider>();
        

//        foreach (var a in transforms)
//            a.gameObject.layer = (int)Layer.Enemy;
//        RightHand = FirstOrDefault(transforms, a => a.tag == "rhand");
//        if (this.weapons.Length > 0)
//        {
//            weapon = this.weapons[Random.Range(0, this.weapons.Length)];
//            var original = weapon.weaponPrefab;
//            weaponObj = (GameObject)Instantiate(original, RightHand.position + original.transform.position, RightHand.rotation * original.transform.rotation);
//            weaponObj.transform.parent = RightHand;
//        }
//        controller = GetComponent<NavMeshAgent>();
//        hips = anim.GetBoneTransform(HumanBodyBones.Hips);

//        if (_Loader.disableRigidBodies)
//        {
//            foreach (var a in this.GetComponentsInChildren<Joint>())
//                Destroy(a);

//            foreach (Rigidbody a in this.GetComponentsInChildren<Rigidbody>())
//                Destroy(a);
//        }

//        foreach (var a in transform.GetComponentsInChildren<Rigidbody>())
//            if (_Loader.rigOpt == OptState.disableRig)
//                Destroy(a);
//            else if (_Loader.rigOpt == OptState.saverig)
//                Save(a.transform);
//            else
//                a.isKinematic = true;

//        if (_Loader.rigOpt != OptState.saverig)
//        foreach (Collider a in GetComponentsInChildren<Collider>())
//            if (a.transform != transform.root)
//            {
//                if (_Loader.rigOpt == OptState.disableRig)
//                    Destroy(a);
//                else if (_Loader.rigOpt == OptState.rig)
//                    a.enabled = false;
//            }

//        if (_Loader.enableShadows && camera.actualRenderingPath != RenderingPath.DeferredLighting)
//        {
//            var componentInChildren = GetComponentInChildren<Rigidbody>();
//            if (componentInChildren != null)
//            {
//                var c = componentInChildren.gameObject;
//                var fs = c.AddComponent<FS_ShadowSimple>();
//                this.fsShadowSimple = fs;
//                fs.useLightSource = true;
//                fs.lightSource = _Player.light;
//                fs.girth = 2;
//                fs.maxProjectionDistance = 5;
//                fs.layerMask = 1 << (int)Layer.Level;
//            }
//        }
//        if (weapon == null) weapon = _Database.defWep;

//    }
    
    
//    private List<AnimationState> enumerate(params object[] e)
//    {
//        List<AnimationState> l = new List<AnimationState>();
//        foreach (object o in e)
//            if (o != null)
//            {
//                if (o is AnimationState)
//                    l.Add((AnimationState)o);
//                else
//                    l.AddRange((List<AnimationState>)o);
//            }
//        while (l.Remove(null)) ;
//        return l;
//    }
//    Timer timer = new Timer();
//    private AnimatorStateInfo info;
//    private Attack attack;
//    //private State state;
//    public enum Attack { attack = 1, thrown = 2, hitAnim = 3, hitCAnim = 4 }
//    //enum State { walk = 1 ,run = 2}
//    public void Update()
//    {
//        UpdateAnims();
//        if (_Game.ZombiesFromAllDirs)
//            UpdateZombieRepos();
//        UpdateRandomAction();
//        UpdateAttack();
//        UpdateMoveAnimation();
//        UpdateOther();
//        timer.Update();
//    }

//    private void UpdateAnims()
//    {
//        info = anim.GetCurrentAnimatorStateInfo(1);
//        anim.SetInteger("attack", (int) attack);
//        anim.SetFloat("speed", veloticy);
//        veloticy = 0;
//        attack = 0;
//        if (oldCurve < .5f && anim.GetFloat("Curve") >= .5f)
//            onDoneAction();
//        oldCurve = anim.GetFloat("Curve");
//    }

//    private float veloticy;
//    //private float speed = 0;
//    private float oldCurve;
//    private void UpdateZombieRepos()
//    {
//        if (Time.time - visTime > 3 && Time.time - lastTimeHit > 10 && !fire)
//        {
//            if (!Rect.MinMaxRect(0, 0, 1, 1).Contains(camera.WorldToViewportPoint(pos)))
//            {
//                var v = pos - _Player.pos;
//                var p = ZeroY(-v + _Player.pos);
//                if (!Rect.MinMaxRect(-.2f, -.2f, 1.2f, 1.2f).Contains(camera.WorldToViewportPoint(p)))
//                {
//                    pos = ZeroY(-Vector3.ClampMagnitude(v, 20) + _Player.pos);
//                    visTime = Time.time;
//                }
//            }
//        }
//    }
//    private void UpdateRandomAction()
//    {
//        if (Time.time > time)
//        {
//            randomRot = Random.Range(-10, 10);
//            time = Time.time + Random.Range(1, 5);
//        }
//    }
//    private void UpdateAttack()
//    {
//        //_Loader.AppendLine(attao);
//        if ((_Player.pos - pos).magnitude < 5 && Vector3.Angle(_Player.pos - pos, forward) < 45 || _Game.zombies.Count < 3)
//        {
//            SetEnemy();
//        }
//        if (enemy != null)
//            targetPos = enemy.pos;

//        if (!attackOrGetHit)
//        {
//            posToTarget = ZeroY(targetPos - transform.position);
//            v3 = Quaternion.Euler(0, Mathf.Min(70, randomRot * _Game.moveToPointCurve.Evaluate(posToTarget.magnitude)), 0) * posToTarget;
            
//            if (posToTarget.magnitude < 1.5)
//            {
//                if (enemy != null)
//                {
//                    CrossFade(Attack.attack, onAttack);
//                }
//                else
//                {
//                    var p = Random.insideUnitSphere;
//                    p.y = 0;
//                    targetPos = p*AvatarRange;
//                }
//            }
//            else
//            {
//                if (enemy != null && weapon.weaponType == WeaponType.thrown && weaponObj && posToTarget.magnitude < 7)
//                {
//                    //todo Zombie.UpdateAttack:239  raycastcheck 
//                    if (!Physics.Raycast(upPos + forward.normalized, posToTarget, posToTarget.magnitude, 1 << (int)Layer.Enemy))
//                        CrossFade(Attack.thrown, onThrow);
//                    //print("throw");
//                }
//            }
//        }


        
//    }
    
//    private void onThrow()
//    {
//        //var weaponObj = (GameObject)Instantiate(this.weaponObj);
//        //weaponObj.transform.position = this.upPos + forward.normalized;
        
//        weaponObj.AddComponent<Rock>();
//        weaponObj.AddComponent<Rigidbody>();
//        weaponObj.AddComponent<BoxCollider>();
//        weaponObj.transform.parent = null;
//        weaponObj.rigidbody.isKinematic = false;
//        weaponObj.rigidbody.velocity =forward.normalized * 16 + Vector3.up;
//        weaponObj.rigidbody.angularVelocity = Random.insideUnitSphere;
//        weaponObj = null;
//    }

//    private void UpdateOther()
//    {
//        if (fsShadowSimple != null)
//            fsShadowSimple.transform.LookAt(_Player.light.transform);
//        if (Input.GetKeyDown(KeyCode.O))
//            SetLife(100, Vector3.zero, 1000);
//        if (fire && timer.TimeElapsed(500))
//            SetLife(Mathf.Clamp(maxlife * .25f, 5, 25));
//    }
//    private void SetEnemy()
//    {
//        if (enemy == null && Random.value > .5f)
//            run = true;
//        enemy = _Player;
//    }
//    public void onAttack()
//    {
//        //print("OnAttack");
//        if (enemy != null && (enemy.pos - pos).magnitude < 1.5f && Vector3.Angle(_Player.pos - pos, forward) < 45)
//            enemy.SetLife();
//    }
//    private Vector3 v3;
//    public void CrossFade(Attack a, Action o = null)
//    {
//        attack = a;
//        onDoneAction = o;
//    }

//    private Action onDoneAction;
    
//    public bool IsPlaying(List<AnimationState> a)
//    {
//        foreach (var b in a)
//            if (b.enabled && (b.normalizedTime < 1 || b.wrapMode != WrapMode.ClampForever))
//                return true;
//        return false;
//    }
//    public bool IsPlaying(AnimationState a)
//    {
//        return a.enabled && a.normalizedTime < 1;
//    }

//    //private float attackOrGetHitf;//{ get { return IsPlaying(attackAnim) || IsPlaying(hitAnim) || IsPlaying(hitCAnim) || IsPlaying(throwAnim); } }
//    private bool attackOrGetHit;
//    private Vector3 forward;
//    private Vector3 posToTarget;
//    private void UpdateMoveAnimation()
//    {
//        //if (!info.IsTag("idle"))
//            //attackOrGetHitf = Time.time;
//        attackOrGetHit = !info.IsTag("idle") || anim.IsInTransition(1);
        
//        //_Loader.AppendLine(info.IsTag("idle"));

//        anim.speed = this.animSpeed;
        
//        //if (IsPlaying(attackAnim))
//        if (attackOrGetHit)
//            forward = posToTarget;
//        if (!attackOrGetHit && posToTarget.magnitude > 1)            
//        {            
//            if (controller == null)
//            {
//                veloticy = run ? 2f : 1f;
//                transform.position += v3.normalized * speed * (run ? 2f : 1) * Time.deltaTime;
//                forward = v3;
//            }
//            if (controller != null)
//            {
//                var destination = v3 + pos;
//                controller.SetDestination(destination);
//                forward = controller.steeringTarget - pos;
//            }
//        }
//        model.forward = Vector3.Slerp(model.forward, forward, Time.deltaTime * 10);
//    }
//    private float lastTimeHit = float.MinValue;
//    public void SetLife(float damage)
//    {
//        SetLife(damage, Vector3.zero, 0);
//    }
//    public override void SetLife(float damage, Vector3 h, float forse)
//    {
//        if (!alive) return;
//        lastTimeHit = Time.time;
//        var v = (maxlife + life) / 2;
//        if (damage > v * .3f)
//        {
//            CrossFade(damage > v * .5 ? Attack.hitCAnim : Attack.hitAnim);
//        }
//        if (damage > 3)
//        {
//            SetEnemy();
//            _Game.Text(((int)damage).ToString(), upPos, Color.white);
//        }
//        if (!_Loader.Immortal)
//            life -= damage;
//        if (deadMaterial != null && life - damage < 0)
//            skinnedMeshRenderer.material.mainTexture = deadMaterial.deadTexture;
//        if (this.life <= 0)
//            Die(h, forse);
//    }
//    public void Die()
//    {
//        Die(Vector3.zero, 0);
//    }
//    public void Die(Vector3 h, float forse)
//    {
//        foreach (var a in this.transforms)
//            a.gameObject.layer = (int)Layer.Dead;


//        ICollection<Rigidbody> rigidBodies = null;

//        if (_Loader.rigOpt == OptState.saverig)
//        {
//            Load();
//            rigidBodies = rgs.Values;
//        }
//        else if (_Loader.rigOpt == OptState.rig)
//        {
//            rigidBodies = transform.GetComponentsInChildren<Rigidbody>();
//            foreach (var a in rigidBodies)
//                a.isKinematic = false;

//            foreach (var a in GetComponentsInChildren<Collider>())
//                a.enabled = true;
//        }

//        foreach (var a in rigidBodies)
//            a.AddExplosionForce(forse, h, 100);

//        foreach (var a in this.GetComponents<Collider>())
//            a.enabled = false;
//        if (controller != null)
//            controller.enabled = false;
        
//        skinnedMeshRenderer.updateWhenOffscreen = true;
        
//        this.anim.enabled = false;
//        if (fire)
//        {
//            foreach (var a in fire.GetComponentsInChildren<ParticleEmitter>())
//                a.emit = false;
//            fire.GetComponentInChildren<Light>().enabled = false;
//        }
//        StartCoroutine(MaterialFade(Freeze));
//        alive = false;
//        if (fsShadowSimple)
//        {
//            ((FS_ShadowManagerMesh)FindObjectOfType(typeof(FS_ShadowManagerMesh))).removeShadow(fsShadowSimple);
//            Destroy(fsShadowSimple);
//        }
//    }
    
//    public IEnumerator MaterialFade(Action a, float fade = .5f)
//    {
//        Color color = skinnedMeshRenderer.material.color;
//        Color color2 = skinnedMeshRenderer.material.color * fade;
//        for (int i = 0; i < 10; i++)
//        {
//            yield return new WaitForSeconds(.3f);
//            foreach (var m in skinnedMeshRenderer.materials)
//                m.color = Color.Lerp(color, color2, i / 10f);
//        }
//        a();
//    }
//    public void Freeze()
//    {
//        StartCoroutine(FreezeCor());
//    }
//    public IEnumerator FreezeCor()
//    {
//        while (_Loader.disableRigidBodies || hips.rigidbody.velocity.magnitude > .05f)
//            yield return null;
//        alive = false;
//        var dead = new GameObject("dead");
//        dead.transform.position = rootBone.position;
//        dead.transform.rotation = rootBone.rotation;
//        var filter = dead.AddComponent<MeshFilter>();
//        filter.mesh = skinnedMeshRenderer.sharedMesh;
//        var meshRenderer = dead.AddComponent<MeshRenderer>();
//        meshRenderer.castShadows = false;
//        meshRenderer.materials = skinnedMeshRenderer.sharedMaterials;
//        skinnedMeshRenderer.BakeMesh(filter.mesh);
//        dead.layer = (int)Layer.Dead;
//        if (_Loader.EnableDeadColliders)
//        {
//            var deadcol = new GameObject("deadCollider");
//            deadcol.transform.position = rootBone.position;
//            deadcol.transform.rotation = rootBone.rotation;
//            deadcol.transform.parent = dead.transform;
//            MeshCollider col2 = deadcol.AddComponent<MeshCollider>();
//            col2.sharedMesh = filter.mesh;
//            col2.convex = true;
//            deadcol.transform.position += Vector3.down * (col2.bounds.size.y / 3);
//        }
//        Destroy(fire);
//        Destroy(gameObject);
//        _Game.zombies.Remove(this);
//    }
    
//    public override void SetFire()
//    {
//        if (this.fire == null)
//        {
//            var f = (GameObject)Instantiate(Bs._Game.firePrefab, this.hips.position, this.rot);
//            f.transform.parent = this.hips;
//            this.fire = f;
//            this.run = true;
//            StartCoroutine(AddMethod(1, SetOtherOnFire));
//            this.StartCoroutine(this.MaterialFade(delegate { Destroy(fire); }, .3f));
//        }
//    }
//    private void SetOtherOnFire()
//    {
//        if (fire && alive)
//            foreach (var z in _Game.zombies)
//                if (z.fire == null && (z.pos - pos).magnitude < 2)
//                    z.SetFire();
//    }
  
//}
