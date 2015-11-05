using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
public class Zombie : Shared
{
    public bool run;
    public bool skeleton;
    public bool runing;
    public bool awaken;
    private bool oldDmgAnim;
    private bool m_alive = true;
    public override bool alive { get { return enabled && m_alive && (wakeup == null || !anim[wakeup.name].enabled); } set { enabled = value; } }
    private float randomRot;
    public Vector3 targetPos;
    public GameObject deathObj;
    public static int AvatarRange = 30;
    private float time;
    //internal NavMeshAgent controller;
    private Vector3 moveDir;
    public Shared enemy;
    internal Animation anim;
    
    private DeadMaterial deadMaterial;
    private float visTime;
    public AnimationClip roar;
    public AnimationClip runAnim;
    public AnimationClip walkAnim;
    public List<AnimationClip> attackAnim = new List<AnimationClip>();
    public AnimationClip explode;
    public AnimationClip throwAnim;
    public AnimationClip hitAnim;
    public AnimationClip hitCAnim;
    internal AnimationClip dance;
    public List<AnimationClip> dances = new List<AnimationClip>();
    public AnimationClip idleAnim;
    public AnimationClip crawlAnim;
    public AnimationClip wakeup;
    public AnimationClip standUp;
    public AnimationClip castUp;
    public AnimationClip spellCast;
    public AnimationClip deathAnim;
    public AnimationClip jump;
    public float animSpeed = 1;
    public float speed;
    public WeaponSettings[] weapons;
    internal WeaponSettings weapon;
    internal GameObject weaponObj;
    internal List<CRC> crcs = new List<CRC>();
    Dictionary<string, Rigidbody> rgs;
    public int rand;
    
    private GameObject dead;
    internal float slowDownTime = float.MinValue;
    public List<BoneCut> boneCuts = new List<BoneCut>();
    //internal List<BoneCut> boneCutsOther = new List<BoneCut>();
    public List<BoneCut> boneCutsCritical = new List<BoneCut>();
    public BoneCut torsoCut;
    public BoneCut legCut;
    private Vector3 _posToTargetFoward;
    
    //public bool waistSplitted;
    public Transform[] lookAt;
    public override void Awake()
    {
        if (awaken) return;
        if (_Loader.PreAwakeObjects)
            Debug.Log("Awake " + awaken, this);
        awaken = true;
        foreach (var a in GetComponentsInChildren<BoneCut>()) // .OrderByDescending(a => a.waist)//todo Zombie.Start:65 
        {
            a.InitMe();
            if (!a.Critical)
                boneCuts.Add(a);
            boneCutsCritical.Add(a);
        }

        base.Awake();
    }
    public void Start()
    {
        
        _Game.destroyables.Add(this);
        rand = Random.Range(0, int.MaxValue);
        pos = transform.position;
        InitModel();
        //InitShadows();
        InitAnim();
        InitOptimize();
        InitOther();
        InitWep();
        if (_Loader.enableWakeup)
            WakeUp();
        Mass = skinnedMeshRenderer.bounds.size.y * .7f;
        size = ZeroY(skinnedMeshRenderer.bounds.size).magnitude;
    }
    private void InitModel()
    {
        //foreach (var a in GetComponentsInChildren<CapsuleCollider>())
        //{
        //    var caps = a as CapsuleCollider;
        //    if (caps !=null)
        //    {
        //        var box = caps.gameObject.AddComponent<BoxCollider>();
        //        box.center = caps.center;
        //        box.size = new Vector3(caps.radius, caps.height, caps.radius);
        //        Destroy(caps);
        //    }
        //}

        maxlife = life;
        _Game.zombies.Add(this);
        anim = GetComponentInChildren<Animation>();
        anim.playAutomatically = false;
        anim.Stop();
        model = anim.transform;
        
        deadMaterial = GetComponentInChildren<DeadMaterial>();
        if (model.GetComponent<Forward>() == null)
            model.gameObject.AddComponent<Forward>().receiver = transform;
        foreach (var a in rigidBodies)
            a.angularDrag = 2;        

    }
    private void InitOther()
    {
        
        var p = Random.insideUnitSphere;
        p.y = 0;
        targetPos = p * AvatarRange;
        modelCollider.gameObject.layer = (int)Layer.Enemy;

        if (hips == null)
            hips = model.GetComponentInChildren<Rigidbody>().transform;
        spine = FirstOrDefault(transforms, a => a.tag == "spine");

        if (spineBones.Count == 0)
        foreach (var a in model.GetComponentsInChildren<Transform>())
            if (a.tag == "spine2" || a.tag == "spine")
            {
                spineBones.Add(a);
                spineBonesPos.Add(a.localPosition);
            }

    }

    public void LateUpdate()
    {
        if (hitAnim == null)
        {
            var zeroY = ZeroY(pos - lastTimeHitPos).normalized;
            var normalized = zeroY;
            normalized = Vector3.Cross(Vector3.up, zeroY);
            if (Time.time - lastTimeHit < _Game.aniDmgCurve.keys[_Game.aniDmgCurve.length - 1].time)
            {
                var angle = _Game.aniDmgCurve.Evaluate(Time.time - lastTimeHit);

                foreach (Transform t in spineBones)
                    t.RotateAround(normalized, angle);
                foreach (Transform t in armBones)
                    t.RotateAround(-normalized, angle * spineBones.Count);
            }
        }
    }
    public List<Transform> spineBones = new List<Transform>();
    public List<Transform> armBones = new List<Transform>();
    internal List<Vector3> spineBonesPos = new List<Vector3>();
    private void WakeUp()
    {
        anim.Stop();
        var ps = (ParticleSystem)Instantiate(_Game.dustParticles, pos, Quaternion.identity);
        Destroy(ps.gameObject, 3);
        CrossFade(wakeup, null, 2);
    }
    private void InitWep()
    {
        if (this.weapons.Length > 0)
        {
            RightHand = FirstOrDefault(transforms, a => a.tag == "rhand");
            weapon = this.weapons[Random.Range(0, this.weapons.Length)];
            if (RightHand != null)
            {
                var original = weapon.weaponPrefab;
                weaponObj = (GameObject)Instantiate(original, RightHand.position + original.transform.position, RightHand.rotation * original.transform.rotation);
                if (weaponObj.collider == null)
                {
                    weaponObj.AddComponent<BoxCollider>();
                    weaponObj.collider.enabled = false;
                    m_colliders = null;
                }
                weaponObj.transform.parent = RightHand;
            }
        }
        if (weapon == null) weapon = _Database.defWep;
    }
    
    //private void InitShadows()
    //{
      
        //if (_Loader.enableShadows && camera.actualRenderingPath != RenderingPath.DeferredLighting)
        //{
        //    var componentInChildren = GetComponentInChildren<Rigidbody>();
        //    if (componentInChildren != null)
        //    {
        //        var c = componentInChildren.gameObject;
        //        var fs = c.AddComponent<FS_ShadowSimple>();
        //        this.fsShadowSimple = fs;
        //        fs.useLightSource = true;
        //        fs.lightSource = _Player.light;
        //        fs.girth = 2;
        //        fs.maxProjectionDistance = 5;
        //        fs.layerMask = 1 << (int)Layer.Level;
        //    }
        //}
    //}
    private List<AnimationClip> animationClips;
    private void InitAnim()
    {
        dance = dances.Count > 0 ? dances[Random.Range(0, dances.Count)]:null;

        animationClips = enumerate(hitCAnim, hitAnim, attackAnim, throwAnim, wakeup, standUp, castUp, deathAnim, explode, spellCast, roar,jump);
        foreach (var a in animationClips)
            if (a != null)
            {
                anim[a.name].layer = 0;
                a.wrapMode = WrapMode.ClampForever;
            }
        foreach (var a in enumerate(walkAnim, runAnim, idleAnim, dance, crawlAnim))
            if (a != null)
            {
                anim[a.name].layer = 0;
                a.wrapMode = WrapMode.Loop;
                anim[a.name].normalizedTime = Random.value;
            }
    }
    public override void OnEditorGui()
    {
        if (GUILayout.Button("nnnn"))
        {
            edit();
        }
        base.OnEditorGui();
    }
    private void edit()
    {
        anim = GetComponentInChildren<Animation>();
        Debug.Log(anim,anim);
        
        runAnim = anim.GetClip("run");
        attackAnim.Clear();
        for (int i = 1; i < 6; i++)
            if (anim["attack" + i] != null)
                attackAnim.Add(anim.GetClip("attack" + i));
        dances.Clear();
        for (int i = 1; i < 10; i++)
            if (anim["dance" + i] != null)
                dances.Add(anim.GetClip("dance" + i));
        idleAnim = Anim("idle");
        hitAnim = Anim("hit");
        hitCAnim = Anim("hitCritical");
        wakeup = Anim("wakeup");
        standUp = Anim("standUp");
        castUp = Anim("castup");
        walkAnim = Anim("walk", true);
        crawlAnim = Anim("crawl");
        throwAnim = Anim("throw");
        SetDirty();
    }


    private AnimationClip Anim(string a, bool important = false)
    {
        var animationState = anim[a];
        if (animationState == null)
        {
            if (important)
                Debug.Log(name + "." + a + " is null", gameObject);
            //return idleAnim;
        }
        return anim.GetClip(a);
    }
    private List<AnimationClip> enumerate(params object[] e)
    {
        List<AnimationClip> l = new List<AnimationClip>();
        foreach (object o in e)
            if (o != null)
            {
                if (o is AnimationClip)
                    l.Add((AnimationClip)o);
                else
                    l.AddRange((List<AnimationClip>)o);
            }

        return l;
    }

    

    Timer timer = new Timer();
    private Vector3 jumpStartPos;
    private Vector3 jumpEndPos;
    //private Vector3 plPos;
    public void Update()
    {
        
        if (runAnim == null)
            InitAnim();
        if (IsPlaying(wakeup, .7f))
            return;
        if (jump != null)
            UpdateJump();
        UpdateHealer();
        UpdateAttack();
        UpdateMoveAnimation();
        UpdateOther();
        if (_Game.ZombiesFromAllDirs && !boar)
            UpdateZombieRepos();
        timer.Update();
    }

    private void UpdateJump()
    {
        if (enemy != null && !attackOrGetHit && timer.TimeElapsed(100) && Math.Abs((pos - _Player.pos).magnitude - 7) < .1f && Random.value < .5f || GetKeyDown(KeyCode.Alpha7))
        {
            CrossFade(jump);
            jumpStartPos = pos;
            jumpEndPos = jumpStartPos - (jumpStartPos - _Player.pos) * 2 + _Player.forward * 6;
            //plPos = _Player.pos;
            modelCollider.enabled = false;
            m_alive = false;
            Debug.DrawLine(jumpStartPos, jumpEndPos, Color.red,3);
        }

        var isPlaying = IsPlaying(jump, 1, delegate
        {        
            modelCollider.enabled = true;
            pos = ZeroY(pos);
            m_alive = true;
        });

        if (isPlaying)
        {
            var time = _Game.zombJumpCurve.Evaluate(anim[jump.name].normalizedTime) - .3f;
            if (time < 0) time = 0;
            var vector3 = Vector3.Lerp(jumpStartPos, jumpEndPos , time);
            posToTargetFoward = ZeroY(targetPos - pos);
            attackOrGetHit = true;
            time = _Game.zombJumpCurve.Evaluate(anim[jump.name].normalizedTime);
            if (time > .5f)
                time = 1 - time;
            pos = vector3 + Vector3.up*time*5;
            Debug.DrawLine(jumpStartPos,pos,Color.red);
        }
    }

    public bool healer;

    public void Cut(Vector3 p, float force, bool critical = false, bool demolish = false, bool createblood=true)
    {
        if (cut || demolish)
        {
            var cuts = critical ? boneCutsCritical : boneCuts;
            for (int i = Mathf.Min(demolish ? cuts.Count : 1, cuts.Count) - 1; i >= 0; i--)
            {
                BoneCut b = cuts[demolish ? i : Random.Range(0, cuts.Count)];
                b.Cut(p, force / 2, createblood);
                cuts.Remove(b);
                boneCutsCritical.Remove(b);
            }
        }
    }
    
    private void UpdateZombieRepos()
    {
        if (Time.time - visTime > 3 && Time.time - lastTimeHit > 10 && !fire)
        {
            if (!visible)
            {
                var v = pos - _Player.pos;
                var p = ZeroY(-v + _Player.pos);
                var contains = Rect.MinMaxRect(-.2f, -.2f, 1.2f, 1.2f).Contains(camera.WorldToViewportPoint(p));
                bool spawnAnywhere = !AnimEnabled(dance) && _Loader.enableWakeup && Random.value < .3f;
                if (spawnAnywhere || !contains)
                {
                    setPos(ZeroY(-Vector3.ClampMagnitude(v, 20) + _Player.pos));
                    visTime = Time.time;
                }
                if (contains && _Loader.enableWakeup)
                    WakeUp();
            }
        }
    }
    
    bool AnimEnabled(AnimationClip an)
    {
        return an != null && anim[an.name].enabled;
    }
    public void standUp2()
    {
        life = maxlife;
        gameObject.SetActive(true);
        StartCoroutine(StandUp());
    }
    public void onHeal()
    {
        lastAttack = Time.time;        
        enemy = _Player;
        foreach (Zombie a in _Game.canStandup)
            if (Vector3.Magnitude(pos - a.hipsPos) < 5)
            {
                Instantiate(_Database.heal, a.hipsPos + Vector3.up, Quaternion.identity);
                a.standUp2();
            }
    }

    private void UpdateAttack()
    {
        if (AnimEnabled(dance)) return;
        if (enemy == null)
            run = false;
        foreach (var a in _Game.monkeys)
            if (Vector3.Magnitude(a.pos - pos) < 8)
            {
                enemy = a;
                run = true;
            }
        if (enemy == null && (_Player.pos - pos).magnitude < 5 && Vector3.Angle(_Player.pos - pos, forward) < 45)
            enemy = _Player;
        if (enemy != null)
            targetPos = enemy.hipsPos;
        if (healer)
            Debug.DrawLine(pos, targetPos);


        if (!attackOrGetHit)
        {
            posToTargetFoward = ZeroY(targetPos - pos);

            if (!boar)
                if (posToTargetFoward.magnitude < size / 2)
                {
                    if (enemy is Zombie)
                    {
                        CrossFade(castUp, onHeal);
                        Instantiate(_Database.heal2, pos, Quaternion.identity);
                    }
                    else if (enemy != null)
                    {
                        if (explode != null)
                            CrossFade(explode, onExplode);
                        else
                            CrossFade(attackAnim, onAttack);
                    }
                    else
                    {
                        var p = Random.insideUnitSphere;
                        p.y = 0;
                        targetPos = p * AvatarRange;
                    }
                }
                else
                {

                    if (enemy != null && weapon.weaponType == WeaponType.Bow && Time.time - lastAttack > 5 && visible)
                    {
                        CrossFade(spellCast, OnSpellCast);
                    }
                    if (enemy != null && weapon.weaponType == WeaponType.thrown && weaponObj && posToTargetFoward.magnitude < 7)
                    {
                        if (!Physics.Raycast(upPos + forward.normalized, posToTargetFoward, posToTargetFoward.magnitude, 1 << (int)Layer.Enemy))
                            CrossFade(throwAnim, onThrow);
                    }
                }
        }
    }

    private void OnSpellCast()
    {
        lastAttack = Time.time;
        var vector3 = (_Player.upPos - upPos) + ZeroY(Random.insideUnitSphere*weapon.accuraty).normalized;
        var instantiate = (GameObject)Instantiate(weapon.bullet, upPos+vector3*.5f, Quaternion.LookRotation(vector3));
        if (_Loader.hideEffects)
            instantiate.hideFlags = HideFlags.HideInHierarchy;
        var arrow = instantiate.GetComponent<Arrow>();
        arrow.enemy = true;
        arrow.damage = 5;
        Destroy(instantiate, 5);
    }

    List<Zombie> near = new List<Zombie>();
    private void UpdateHealer()
    {
        
        if (AnimEnabled(dance) || !healer) return;
        if (healer && visible)
        {
            near.Clear();
            foreach (var a in _Game.zombies)
                if (a != this && a.alive && (a.pos - pos).magnitude < 5)
                    near.Add(a);

            if (roar != null && !attackOrGetHit && near.Count > 2 && Time.time - lastAttack > 4)
            {
                lastAttack = Time.time;
                Instantiate(_Database.deathExplosionBase, upPos + Vector3.up, Quaternion.identity);
                posToTargetFoward = (_Player.pos - pos);
                attackOrGetHit = true;
                CrossFade(roar, delegate
                {
                    int i = 0;
                    foreach (var a in near)
                        if (i++ < 4)
                        {
                            ParticleSystem g = (ParticleSystem)Instantiate(_Database.moveEffect, a.skinnedMeshRenderer.bounds.center, Quaternion.identity);
                            g.transform.parent = a.transform;
                            Destroy(g.gameObject, 1f);
                            g.transform.localScale = a.skinnedMeshRenderer.bounds.size;
                            a.StartCoroutine(AddMethod(.2f, delegate { g.enableEmission = false; }));
                            a.StartCoroutine(MoveObject(a, (_Player.pos - pos), .2f));
                        }
                });
            }
        }
        if (healer && Time.time - lastAttack > 1 && (!(enemy is Zombie) || enemy == null || !enemy.visible))
        {
            if (_Game.canStandup.Count > 1)
            {
                float mg = float.MaxValue;
                Zombie zmb = null;
                foreach (Zombie a in _Game.canStandup)
                    if (a.visible && mg > (a.hipsPos - hipsPos).magnitude)
                    {
                        zmb = a;
                        mg = (a.hipsPos - hipsPos).magnitude;
                    }
                if (zmb != null) enemy = zmb;
            }
        }
    }

    private void onThrow()
    {
        weaponObj.AddComponent<Rock>();
        weaponObj.AddComponent<Rigidbody>();
        var c = weaponObj.GetComponent<BoxCollider>();
        c.enabled = true;
        if (modelCollider.enabled)
            Physics.IgnoreCollision(modelCollider, c);
        weaponObj.transform.parent = null;
        weaponObj.rigidbody.isKinematic = false;
        weaponObj.rigidbody.velocity = forward.normalized * 10 + Vector3.up;
        weaponObj.rigidbody.angularVelocity = Random.insideUnitSphere;
        Destroy(weaponObj.gameObject, 5);
        weaponObj = null;
    }
    public Vector2 randomRotRange = new Vector2(0, 10);
    private void UpdateOther()
    {
        if (boar || _Game.alwaysTargetPlayer)
            enemy = _Player;
        if (Time.time > time)
        {
            randomRot = Random.Range(randomRotRange.x, randomRotRange.y) * (Random.value > .5f ? 1f : -1f);
            time = Time.time + Random.Range(1, 5);
        }
        //if (fsShadowSimple != null)
            //fsShadowSimple.transform.LookAt(_Player.light.transform);
        if (Input.GetKeyDown(KeyCode.V))
            Die(Vector3.zero, 0);
        if (fire && timer.TimeElapsed(500))
            SetLife(Mathf.Clamp(maxlife * .25f, 5, 25));
        foreach (var a in lookAt)
            a.LookAt(camera.transform);
    }

    private Action animEvent;
    public void onEvent()
    {
        if (animEvent!=null)
        animEvent();
    }
    public void onExplode()
    {
        if (enemy != null)
        {
            var magnitude = (enemy.pos - pos).magnitude;
            if (magnitude < 5)
                enemy.SetLife((5f - magnitude)*10);
        }
        Instantiate(_Database.Hit_Poison_Chest, upPos, Quaternion.identity);
        enabled = false;
        gameObject.SetActive(false);
    }

    public void onAttack()
    {
        if (enemy is Monkey)
            enemy.transform.forward = ZeroY(Random.insideUnitSphere);
        if (enemy != null && (enemy.pos - pos).magnitude < size * .6f && Vector3.Angle(_Player.pos - pos, forward) < 45)
            enemy.SetLife();
    }
    
    public void CrossFade(AnimationClip a, Action o = null, float speed = 1)
    {
        if (animationClips.Contains(a))
            attackOrGetHit = true;
        if (o != null)
            animEvent = o;
        if (speed != 1)
            anim[a.name].speed = speed;
        if (a.wrapMode == WrapMode.ClampForever)
            anim[a.name].time = 0;
        anim.CrossFade(a.name);
    }
    public void CrossFade(List<AnimationClip> a, Action o = null)
    {
        if (o != null)
            animEvent = o;
        CrossFade(a[Random.Range(0, a.Count)]);
    }
    public bool IsPlaying(List<AnimationClip> a)
    {
        foreach (var b in a)
            if(b!=null)
            if (anim[b.name].enabled && (anim[b.name].normalizedTime < 1 || b.wrapMode != WrapMode.ClampForever))
                return true;
        return false;
    }

    private Dictionary<string, bool> playingAnims = new Dictionary<string, bool>();
    public bool IsPlaying(AnimationClip a, float time = 1,Action act = null)
    {
        if (a == null) return false;
        var s = a.name;
        var isPlaying = anim[s].enabled && anim[s].normalizedTime < time;
        if (act != null && playingAnims.ContainsKey(s) && playingAnims[s] != isPlaying && !isPlaying)
            act();

        playingAnims[s] = isPlaying;
        return isPlaying;
    }
    private bool attackOrGetHit; 
    private Vector3 forward;
    private Vector3 forwardSmooth;
    private Vector3 posToTargetFoward;
    internal float dancet = float.MinValue;
    public override Vector3 pos { get; set; }
    
    private bool crawl;
    private float lastAttack = float.MinValue;
    private void UpdateMoveAnimation()
    {
        float sldown = Time.time - slowDownTime < 3 ? .4f : 1;

        if (Frame(5,true))
        {
            attackOrGetHit = IsPlaying(animationClips);
            isAttacking = IsPlaying(attackAnim);
        }
        anim[runAnim.name].speed = anim[walkAnim.name].speed = this.animSpeed * sldown;
        foreach (AnimationClip a in attackAnim)
            anim[a.name].speed = this.animSpeed * sldown;

        if (attackOrGetHit && !boar)
            forward = posToTargetFoward;
        if (!attackOrGetHit || boar)
        {
            var danc = Time.time - dancet < 14;
            //if(crawl)print("crawl");
            if (Frame(2) && !attackOrGetHit )
                CrossFade(danc ? dance : crawl ? crawlAnim : (run || boar) ? runAnim : walkAnim);
            if (AnimEnabled(dance)) return;
            var vector3 = boar ? ZeroY(targetPos + _Player.controller.velocity * .5f - pos).normalized : Quaternion.Euler(0, Mathf.Min(70, randomRot * _Game.moveToPointCurve.Evaluate(posToTargetFoward.magnitude)), 0) * posToTargetFoward.normalized;

            var clamp01 = posToTargetFoward.magnitude > 15 ? 10 : posToTargetFoward.magnitude > 5 ? .1f : .5f;
            forward = Vector3.Slerp(forward, vector3, Time.deltaTime * (boar ? clamp01 : 1)).normalized;

            var vector4 = forward*Time.deltaTime*speed*(boar || run ? 2f : 1)*sldown;

            if (boar)
            {
                vel = Vector3.Lerp(vel, vector4, Time.deltaTime * 5);
                pos += vel;
                RaycastHit h;
                modelCollider.enabled = false;
                if (Physics.SphereCast(pos, .7f, forward, out h, 1, 1 << (int)Layer.Enemy | 1 << (int)Layer.Player))
                {
                    var z = h.transform.root.GetComponent<Destroyable>();
                    if (z != null && z.alive)
                        if (z.haveRigs)
                        {
                            if(z is Player)
                                z.SetLife(10, h.point);
                            z.Die(h.point, 0);
                        }
                        else
                            z.pos += ZeroY(h.point - pos).normalized*3;
                }
                modelCollider.enabled = true;
            }
            else
                pos += vector4;

        }
        forwardSmooth = Vector3.Slerp(forwardSmooth, forward, Time.deltaTime * 20 * sldown);

        if (Frame(2) && !boar)
        {
            Spos = Vector3.Lerp(Spos, pos, Time.deltaTime * 10);
            transform.position = Spos;
        }
        if (Frame(3))
            model.forward = forwardSmooth;

    }
    public void FixedUpdate()
    {
        if (boar)
            rigidbody.MovePosition(pos); 
    }
    private Vector3 vel;
    public bool boar;
    private bool Frame(int i,bool forse = false)
    {
        return !_Loader.FrameOptimization && !forse || (Time.frameCount + rand) % (i * _Loader.frameFactor) == 0;
    }

    internal float lastTimeHit = float.MinValue;
    
    

    private Vector3 lastTimeHitPos ;
    public override void SetLife(float damage, Vector3 h, float forse = 100, bool knockOut = false)
    {
        lastTimeHitPos = _Player.pos;
        lastTimeHit = Time.time;
        
        if (life > 0 && damage > 3)
            _Game.Text(((int)damage).ToString(), hips.position, Color.white);
        if (!_Loader.Immortal)
            life -= damage;

        if (deadMaterial != null && life - damage < 0)
            skinnedMeshRenderer.material.mainTexture = deadMaterial.deadTexture;
        if (alive)
        {
            var v = (maxlife + life) / 2;
            
            if (damage > v * .3f)
            {
                if (hitAnim != null)
                    CrossFade(damage > v*.5 ? hitCAnim : hitAnim); //todRo Zombie.SetLife:534 
            }
            if (damage > 3)
            {
                if (enemy == null && Random.value > damage / maxlife)
                {
                    run = true;
                    enemy = _Player;
                }
            }
            if (crawlAnim != null && (torsoCut.cutted || legCut.cutted) && !crawl)
            {
                foreach (var a in attackAnim)
                    anim[a.name].AddMixingTransform(spine);
                if (life < 0)
                    life = 10;
                Die(h, forse);
                crawl = true;
            }
            else
                if (this.life < 0 || knockOut)
                    Die(h, forse);
        }
    }

    

    public override void Die(Vector3 h, float forse)
    {
        if (!alive) return;
        if (rigidBodies.Length == 0 && life > 0) return;
        //if (rigidBodies.Length == 0 && life > 0) return;
        alive = false;
        dieTime = Time.time;

      

        if (_Loader.rigOpt == OptState.saverig)
            Load();
        else if (_Loader.rigOpt == OptState.rig || _Loader.rigOpt == OptState.hardRig)
        {
            foreach (var a in rigidBodies)
                if (a != null)
                    a.isKinematic = false;
            foreach (var a in colliders)
                if (a != null)
                {
                    a.enabled = a != modelCollider;
                    if (a != modelCollider)
                        a.gameObject.layer = (int)(life < 0 ? Layer.Dead : Layer.bones);
                }
        }
        foreach (var a in rigidBodies)
            if (a != null)
                AddExplosionForce(a,  forse, h, 100);
        if (fire)
        {
            foreach (var a in fire.GetComponentsInChildren<ParticleEmitter>())
                a.emit = false;
            fire.GetComponentInChildren<Light>().enabled = false;
        }
        anim.enabled = false;

        if (life < 0)
        {
            if (deathObj != null)
            {
                deathObj.SetActive(true);
                deathObj.transform.parent = null;
                deathObj.transform.rotation = model.transform.rotation;
                _Game.StartCoroutine(AddMethod(2, delegate
                {
                    #if !UNITY_FLASH || UNITY_EDITOR
                    StaticBatchingUtility.Combine(deathObj);
                    #endif
                    Destroy(deathObj.animation);
                }));
                gameObject.SetActive(false);
            }
            else if (rigidBodies.Length == 0 || _Loader.rigOpt == OptState.disableRig)
            {
                anim.enabled = true;
                CrossFade(deathAnim);
                StartCoroutine(FreezeCor());
            }
            else
            {
                //anim.enabled = false;
                Cut(h, forse, true, skeleton, !skeleton);
                StartCoroutine(FreezeCor());
            }
        }
        else if (rigidBodies.Length > 0)
            StartCoroutine(FreezeCor(.5f));
    }
    public IEnumerator MaterialFade(Action a, float fade = .5f)
    {
        Color color = skinnedMeshRenderer.material.color;
        Color color2 = skinnedMeshRenderer.material.color * fade;
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(.3f);
            foreach (var m in skinnedMeshRenderer.materials)
                m.color = Color.Lerp(color, color2, i / 10f);
        }
        a();
    }
    
    public IEnumerator FreezeCor(float maxvel = .05f)
    {
        while (hips.rigidbody != null && hips.rigidbody.velocity.magnitude > maxvel || Time.time - dieTime < (life > 0 ? 1 : 5))
            yield return null;
        if (life > 0)
        {
            StartCoroutine(StandUp());
            yield break;
        }        
        alive = false;
        dead = new GameObject("dead");
        dead.transform.position = skinnedMeshRenderer.transform.position;
        dead.transform.rotation = skinnedMeshRenderer.transform.rotation;
        var filter = dead.AddComponent<MeshFilter>();
        filter.mesh = skinnedMeshRenderer.sharedMesh;
        var meshRenderer = dead.AddComponent<MeshRenderer>();
        meshRenderer.materials = skinnedMeshRenderer.sharedMaterials;
        skinnedMeshRenderer.BakeMesh(filter.mesh);
        anim.cullingType = AnimationCullingType.AlwaysAnimate;
        dead.layer = (int)Layer.Dead;
        if (_Loader.hideEffects)
            dead.hideFlags = HideFlags.HideInHierarchy;
        //if (_Loader.EnableDeadColliders)
        //{
        //    var deadcol = new GameObject("deadCollider");
        //    deadcol.transform.position = rootBone.position;
        //    deadcol.transform.rotation = rootBone.rotation;
        //    deadcol.transform.parent = dead.transform;
        //    MeshCollider col2 = deadcol.AddComponent<MeshCollider>();
        //    col2.sharedMesh = filter.mesh;
        //    col2.convex = true;
        //    deadcol.transform.position += Vector3.down * (col2.bounds.size.y / 3);
        //}
        Destroy(fire);
        gameObject.SetActive(false);
    }
    public string CalculateTransformPath(Transform t, Transform r)
    {
        string s = "";
        while (t != null && t != r)
        {
            s = t.name + "/" + s;
            t = t.parent;
        }
        return s.Substring(0, Math.Max(0, s.Length - 1));
    }
    private IEnumerator StandUp()
    {
        Reset();
        Destroy(dead);
        AnimationClip ancl = new AnimationClip();
        foreach (var t in transforms)
            if (t != null)
            {
                AddKey("localPosition.x", t.localPosition.x, t, ancl);
                AddKey("localPosition.y", t.localPosition.y, t, ancl);
                AddKey("localPosition.z", t.localPosition.z, t, ancl);
                AddKey("localRotation.x", t.localRotation.x, t, ancl);
                AddKey("localRotation.y", t.localRotation.y, t, ancl);
                AddKey("localRotation.z", t.localRotation.z, t, ancl);
                AddKey("localRotation.w", t.localRotation.w, t, ancl);
            }
        anim.AddClip(ancl, "dead");
        anim.Play("dead");
        if (!crawl && standUp != null)
            CrossFade(standUp);
        yield return new WaitForSeconds(1);
        alive = true;
    }
    private void AddKey(string s, float f, Transform t, AnimationClip ancl)
    {
        var animationCurve = new AnimationCurve(new Keyframe(0, f), new Keyframe(5, f));
        ancl.SetCurve(CalculateTransformPath(t, model), typeof(Transform), s, animationCurve);
    }
    
    public IEnumerator ThrowZombie(Vector3 vel)
    {
        foreach (Rigidbody a in this.rigidBodies)
            if (a != null)
            {
                a.drag = 0;
                a.velocity = vel;
                a.mass *= 15;
            }
        yield return new WaitForSeconds(3);
        foreach (Rigidbody a in this.rigidBodies)
            if (a != null)
            {
                a.mass /= 15;
            }
    }
    public override void SetFire()
    {
        if (!alive) return;
        if (this.fire == null)
        {
            var f = (GameObject)Instantiate(Bs._Game.firePrefab, this.hips.position, this.rot);
            f.transform.parent = this.hips;
            this.fire = f;
            this.run = true;
            StartCoroutine(AddMethod(1, SetOtherOnFire));
            this.StartCoroutine(this.MaterialFade(delegate { Destroy(fire); }, .3f));
        }
    }
    private void SetOtherOnFire()
    {
        if (fire && alive)
            foreach (var z in _Game.zombies)
                if (z.fire == null && (z.pos - pos).magnitude < 2)
                    z.SetFire();
    }
    private void printAnims()
    {
        foreach (AnimationState a in anim)
            _Loader.Log(a.name, a.enabled, a.time);
    }
    public void Save(Transform t)
    {
        CRC cr = new CRC();
        cr.t = t;
        var j = t.GetComponent<CharacterJoint>();
        if (j != null)
            cr.CharacterJoint2 = new CharacterJoint2() { connectedBody = j.connectedBody.name, anchor = j.anchor, axis = j.axis, highTwistLimit = j.highTwistLimit, lowTwistLimit = j.lowTwistLimit, rotationDrive = j.rotationDrive, swing1Limit = j.swing1Limit, swing2Limit = j.swing2Limit, swingAxis = j.swingAxis, targetAngularVelocity = j.targetAngularVelocity, targetRotation = j.targetRotation };
        Destroy(j);
        var r = t.rigidbody;
        if (r != null)
        {
            var rigidbody2 = new Rigidbody2() { mass = r.mass };
            cr.Rigidbody2 = rigidbody2;
        }
        Destroy(r);
        var c = t.collider;
        var bc = c as BoxCollider;
        if (bc != null)
            cr.BoxCollider2 = new BoxCollider2() { center = bc.center, extents = bc.extents };
        var sc = c as SphereCollider;
        if (sc != null)
            cr.SphereCollider2 = new SphereCollider2() { center = sc.center, radius = sc.radius };
        var cc = c as CapsuleCollider;
        if (cc != null)
            cr.CapsuleCollider2 = new CapsuleCollider2() { center = cc.center, direction = cc.direction, height = cc.height, radius = cc.radius };
        Destroy(c);
        crcs.Add(cr);
    }
    //public void OnCollisionEnter(Collision collision)
    //{
    //    print(name);
    //}
    public void OnCollision(Forward fv)
    {
        //print(fv.name);
        if (fv.rigidbody == null) return;
        var g = fv.collision.transform.root.gameObject;
        var vel = fv.collision.impactForceSum.magnitude;
        SetLife(Mathf.Max(0, vel - 10));
        if (vel < 6) return;
        if (g.tag == "Destroyable")
        {
            Game.Destruct(g.transform, fv.transform.position, 300);
        }
        if (g.tag == "Enemy")
        {
            var z = g.GetComponent<Destroyable>();
            if (z != null && z.alive)
                z.Die(fv.collision.contacts[0].point, 500);

        }
    }
    public void Load()
    {
        rgs = new Dictionary<string, Rigidbody>();
        foreach (var cr in crcs)
        {
            var g = cr.t.gameObject;
            if (cr.Rigidbody2 != null)
            {
                var r = g.AddComponent<Rigidbody>();
                g.AddComponent<Forward>().receiver = transform;
                r.mass = cr.Rigidbody2.mass;
                rgs.Add(g.name, r);
            }
            if (cr.CharacterJoint2 != null)
            {
                var a = g.AddComponent<CharacterJoint>();
                var b = cr.CharacterJoint2;
                a.anchor = b.anchor;
                a.axis = b.axis;
                a.connectedBody = rgs[b.connectedBody];
                a.highTwistLimit = b.highTwistLimit;
                a.lowTwistLimit = b.lowTwistLimit;
                a.rotationDrive = b.rotationDrive;
                a.swing1Limit = b.swing1Limit;
                a.swing2Limit = b.swing2Limit;
                a.swingAxis = b.swingAxis;
                a.targetAngularVelocity = b.targetAngularVelocity;
                a.targetRotation = b.targetRotation;
            }
            if (cr.BoxCollider2 != null)
            {
                var a = g.AddComponent<BoxCollider>();
                a.center = cr.BoxCollider2.center;
                a.extents = cr.BoxCollider2.extents;
            }
            if (cr.SphereCollider2 != null)
            {
                var a = g.AddComponent<SphereCollider>();
                a.center = cr.SphereCollider2.center;
                a.radius = cr.SphereCollider2.radius;
            }
            if (cr.CapsuleCollider2 != null)
            {
                var a = g.AddComponent<CapsuleCollider>();
                a.center = cr.CapsuleCollider2.center;
                a.direction = cr.CapsuleCollider2.direction;
                a.height = cr.CapsuleCollider2.height;
                a.radius = cr.CapsuleCollider2.radius;
            }
        }
    }
}
