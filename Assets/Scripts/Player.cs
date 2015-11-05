using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
public class Player : Shared
{
    
    public float speedFactor = 2;
    public AudioClip[] RunSounds;
    public AudioClip[] damageSound;
    public Texture[] bloodTextures;
    public enum Controls { Diablo, HitNRun }
    public AudioClip[] swordSound;
    public Item curWeaponItem;
    public GameObject[] characters;
    public AnimatorStateInfo info { get; set; }
    public AnimatorStateInfo infoSpecial { get; set; }
    public AnimatorStateInfo infoSpecialUpper { get; set; }
    private bool hitAndRun { get { return CurrentCombo == Combo.thrown || (CurrentCombo == CurrentWeapon.combosSpecial ? CurrentWeapon.hitAndRunSpecial : CurrentWeapon.hitAndRun); } }
    internal Animator anim { get; set; }    
    private int swordSoundCurrent { get; set; }
    private Item defWeapon { get; set; }
    private AnimatorStateInfo oldInfo { get; set; }
    public CharacterController controller { get; set; }
    
    private DroppedItem MouseOverItem { get; set; }
    private int currentAttack { get; set; }
    private bool isAiming { get; set; }
    private bool oldRunSound { get; set; }
    private Shared MouseOverEnemy { get; set; }
    private Vector3 moveDir2 { get; set; }
    private Vector3 moveDir { get; set; }
    private Vector3 gravityVel { get; set; }
    internal Vector3 mouseDir { get; set; }
    public Vector3 forward { get; set; }
    private bool oldonAttack { get; set; }
    public new GameObject light;
    private Destroyable holdingZombie;
    public override void Awake()
    {
        base.Awake();
        defWeapon = curWeaponItem;
        if (Game.switchChar != 0)
        {
            foreach (var a in characters)
                a.SetActive(false);
            characters[Game.switchChar % characters.Length].SetActive(true);
        }
    }
    private new SkinnedMeshRenderer renderer;
    public void Start()
    {
        model = GameObject.FindGameObjectWithTag("model").transform;
        InitOptimize();
        spine = FirstOrDefault(transforms, a => a.tag == "spine");
        spine2 = FirstOrDefault(transforms, a => a.tag == "spine2");
        head = FirstOrDefault(transforms, a => a.tag == "head");
        LeftHand = FirstOrDefault(transforms, a => a.tag == "lhand");
        RightHand = FirstOrDefault(transforms, a => a.tag == "rhand"); ;
        plane = GameObject.CreatePrimitive(PrimitiveType.Plane).transform;
        plane.gameObject.layer = (int)Layer.CursorPlane;
        plane.localScale = Vector3.one * 1000;
        plane.renderer.enabled = false;
        plane.collider.isTrigger = true;
        plane.transform.position = upPos;
        anim = this.GetComponentInChildren<Animator>();
        SelectWeapon(curWeaponItem);
        controller = GetComponent<CharacterController>();
        renderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }
    private float sleep = float.MaxValue;
    public void Update()
    {
        if (GetKeyDown(KeyCode.Alpha6))
            Die(pos,0);

        UpdateAnims();
        UpdateNextAttack();
        UpdateInputKeys();
        UpdateCamera();
        UpdateAttackKeys();
        UpdateTargetPos2Move();
        UpdateAttackOnHit();
        UpdatePhysx();
        UpdateLaser();
        UpdateOther();
    }
    private void UpdateAnims()
    {
        info = anim.GetCurrentAnimatorStateInfo((int)CurrentWeapon.layer);
        infoSpecial = anim.GetCurrentAnimatorStateInfo((int)AnimLayer.Special);
        infoSpecialUpper = anim.GetCurrentAnimatorStateInfo((int)AnimLayer.SpecialUpper);
        anim.SetInteger("damage", damageAnim);
        damageAnim = Time.time - _Game.harlemShake < 29 ? 2 : 0;
        anim.SetInteger("Attack", 0);

        isAttacking = IsPlaying(AnimLayer.SpecialUpper) || IsPlaying(AnimLayer.Special) || IsPlaying(CurrentWeapon.layer);
        
        if (isAttacking)
            attackTime = Time.time;
    }
    public bool IsPlaying(AnimLayer SpecialUpper)
    {
        var specialUpper = (int) SpecialUpper;
        return !anim.GetCurrentAnimatorStateInfo(specialUpper).IsTag("idle") || anim.IsInTransition(specialUpper) && !anim.GetNextAnimatorStateInfo(specialUpper).IsTag("idle");
    }
    private void UpdateNextAttack()
    {
        if (info.nameHash != oldInfo.nameHash && isAttacking)
        {
            currentAttack++;
            PlaySound(swordSound[swordSoundCurrent % swordSound.Length]);
            swordSoundCurrent++;
        }
    }
    private void UpdateCamera()
    {
        if (ongui) return;
        RaycastHit h;
        var screenPointToRay = camera.ScreenPointToRay(Input.mousePosition);


        if (_ObsCamera.spec)
        {
            mouseDir = camera.transform.forward;
            if (Physics.Raycast(screenPointToRay, out h, 20, 1 << (int) Layer.Level | 1 << (int) Layer.Enemy))
            {
                mousePos = ZeroY(h.point);
                _ObsCamera.transform.LookAt(h.point);
            }
            else
            {
                mousePos = ZeroY(_ObsCamera.camT.position + mouseDir*10);
            }
        }
        else
        {
            plane.collider.Raycast(screenPointToRay, out h, 1000);
            mousePos = ZeroY(h.point);
            mouseDir = ZeroY(h.point - upPos).normalized;
        }
        MouseOverEnemy = null;
        foreach (var a in _Game.zombies)
            if (a.alive)
            {
                float m = 2;
                if ((mousePos - a.upPos).magnitude < m)
                {
                    m = (mousePos - a.pos).magnitude;
                    MouseOverEnemy = a;
                }
            }
        if (Physics.Raycast(screenPointToRay, out h, 100, 1 << (int)Layer.Coin))
        {
            var coin = h.transform.root.GetComponent<DroppedItem>();
            if (coin != null)
            {
                coin.Shine(true);
                MouseOverItem = coin;
            }
        }
        else
        {
            if (MouseOverItem != null)
            {
                MouseOverItem.Shine(false);
                MouseOverItem = null;
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse0) && MouseOverItem != null)
        {
            oldMouse0 = false;
            foreach (Slot a in _Game.inventorySlots)
                if (a.item == null)
                {
                    a.item = MouseOverItem.item;
                    Destroy(MouseOverItem.gameObject);
                    break;
                }
        }
        _Hud.progressBar.gameObject.SetActive(MouseOverEnemy);
        if (MouseOverEnemy)
        {
            _Hud.progressBarTitle.text = MouseOverEnemy.name;
            _Hud.progressBarTitle2.text = MouseOverEnemy.modelGroup.ToString();
            _Hud.progressBar.sliderValue = (float)MouseOverEnemy.life / MouseOverEnemy.maxlife;
        }
    }
    private Arrow bullet;
    internal Vector3 mousePos;
    private Transform plane;
    private float lastAttack;
    public static bool oldMouse0;
    private void UpdateInputKeys()
    {
        

        moveDir = Vector3.zero;
        mouse1 = mouse0 = mouse = false;
        if (ongui) return;
        mouse1 = Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.Escape);
        oldMouse0 = mouse0 = Input.GetKey(KeyCode.Mouse0) && oldMouse0;
        if (Input.GetKeyDown(KeyCode.Mouse0))
            oldMouse0 = true;
        mouse = mouse0 || mouse1;
        if (Input.GetKey(KeyCode.A))
            moveDir += Vector3.left;
        if (Input.GetKey(KeyCode.D))
            moveDir += Vector3.right;
        if (Input.GetKey(KeyCode.W))
            moveDir += Vector3.forward;
        if (Input.GetKey(KeyCode.S))
            moveDir += Vector3.back;
        if (_ObsCamera.spec)
            moveDir = ZeroY(_ObsCamera.camT.rotation * moveDir);
        moveDir =  moveDir.normalized;
        
    }
    private void UpdateTargetPos2Move()
    {
        var attacking = Time.time - attackTime < .1f;
        if (attacking && !hitAndRun)
        {
            moveDir = Vector3.zero;
            forward = ZeroY(mouseDir);
        }
        else if (controller.velocity.magnitude > .1f)
            forward = controller.velocity;
        else if (isAiming != oldAIm)
            forward = ZeroY(mouseDir);
        forward = forward.normalized;
        oldAIm = isAiming;
        gravityVel += Physics.gravity * Time.deltaTime;
        if (controller.isGrounded) gravityVel = ZeroY(gravityVel);
        moveDir2 = Vector3.Slerp(moveDir2, moveDir, Time.deltaTime * 10);
        veloticy = (moveDir2 * speedFactor * (attacking ? .5f : 1));
        controller.Move(veloticy * Time.deltaTime);
        maxSpeed = Mathf.Max(veloticy.magnitude, maxSpeed);
        anim.SetFloat("Speed", (veloticy.magnitude * (runBackwards ? -1 : 1)) / (maxSpeed * .9f));
    }
    private Vector3 veloticy;
    private float maxSpeed = 1;
    private int currentCombo;
    private Combo CurrentCombo { get { return (Combo)currentCombo; } }
    private bool special;
    private Transform LaserStream;
    private Transform LaserStreamEnd;
    private Vector3 comboStartPos;
    private Vector3 comboEndPos;
    private void UpdateAttackKeys()
    {
        if (Input.GetKey(KeyCode.F) && !isAttacking)
        {
            PlayAnim((int) Combo.thrown);
            foreach (var a in RightHand.GetComponentsInChildren<Renderer>())
                a.enabled = false;
            var g = (Monkey)Instantiate(_Database.monkey, RightHand.position, RightHand.rotation);
            g.transform.parent = RightHand;
            Physics.IgnoreCollision(collider, g.collider);
            g.enabled = false;
            PlayAnim((int)Combo.thrown, delegate
            {
                g.enabled = true;
                g.transform.parent = null;
                g.transform.forward = mouseDir;
                g.vel = mouseDir* 20;
                foreach (var a in RightHand.GetComponentsInChildren<Renderer>())
                    a.enabled = true;
            });
        }
        _Loader.Log(comboStartPos,comboEndPos);
        if (mouse && (!isAttacking || CurrentWeapon.weaponType == WeaponType.Bow))
        {
            PlayAnim(mouse1 ? (int)CurrentWeapon.combosSpecial : CurrentWeapon.combos[(currentAttack % CurrentWeapon.combos.Length)], OnAttack);
            comboStartPos = pos;
            comboEndPos = mousePos;
            if (currentCombo == (int)Combo.hammer2)
                StartCoroutine(AddUpdate(delegate
                {
                    if (!infoSpecial.IsTag("idle"))
                    {
                        if ((comboStartPos - comboEndPos).magnitude > 2)
                        {
                            pos = Vector3.Lerp(comboStartPos, comboEndPos, -.2f + infoSpecial.normalizedTime*2.5f);
                            _Loader.Log(pos);
                        }
                    }
                    return isAttacking;
                }));

            special = mouse1;
        }
        if (isAttacking && CurrentWeapon.ShootInterval > 0)
        {
            lastAttack += Time.deltaTime;
            if (lastAttack > CurrentWeapon.ShootInterval && CurrentWeapon.weaponType == WeaponType.Bow)
            {
                createBullet();
                lastAttack = lastAttack % CurrentWeapon.ShootInterval;
            }
        }
    }
    private void UpdateAttackOnHit()
    {
        bool onAttack = anim.GetFloat("Hit") > .2f;
        
        if (onAttack != oldonAttack && onAttack)
        {
            if (hitAction != null)
                hitAction();
        }
            
        oldonAttack = onAttack;
        oldInfo = info;
    }

    private void OnAttack()
    {
        if (currentCombo == (int) Combo.hammer || currentCombo == (int) Combo.hammer2)
        {
            var hitPos = pos + forward + Vector3.up*.1f;
            Destroy(Instantiate(_Game.hammerFire, hitPos, Quaternion.identity), 2);
            _ObsCamera.shake = 1.5f;
            foreach (var zombie in _Game.destroyables)
            {
                var m = (zombie.pos - hitPos).magnitude;
                if (m < 3)
                {
                    ZombieSetLife(zombie, (CurrentWeapon.damage*(4 - m)), hitPos, 700);
                }
            }
        }
        else if (CurrentWeapon.weaponType == WeaponType.Bow)
        {
            createBullet(special);
            PlaySound(ArrayRandom(CurrentWeapon.damageSound));
        }
        else
        {
            bool oneSound = false;
            int cnt = 1;
            foreach (var zombie in _Game.destroyables)
            {
                var radius = special ? CurrentWeapon.radius*2 : CurrentWeapon.radius;
                if (zombie.alive && Vector3.Angle(zombie.pos - pos, mouseDir) < radius &&
                    Vector3.Distance(pos, zombie.pos) < CurrentWeapon.lenth)
                {
                    if (!oneSound)
                        PlaySound(ArrayRandom(CurrentWeapon.damageSound));
                    RaycastHit h;
                    if (Physics.Linecast(pos + Vector3.up, zombie.pos + Vector3.up, out h, 1 << (int) Layer.Enemy))
                    {
                        var z = h.transform.root.gameObject.GetComponent<Zombie>();
                        if (z != null)
                        {
                            if (CurrentWeapon.layer == AnimLayer.Attack)
                                z.cut = true;
                            if (!z.skeleton)
                            {
                                _Game.CreateBlood(z.upPos, Vector3.up, true);
                                z.CreateBloodSplatter();
                            }
                        }
                        else
                            Debug.Log(h.transform.name + " is null", h.transform);
                    }
                    ZombieSetLife(zombie, CurrentWeapon.damage*(3f/cnt), pos + Vector3.up, 700);
                    if (!special)
                        cnt++;
                    oneSound = true;
                }
            }
        }
    }

    private void ZombieSetLife(Destroyable destroyable, float damage, Vector3 vector3, int forse)
    {
        bool knockout = false;

        var zombie = destroyable as Zombie;
        if (zombie!=null)
        {
            zombie.Cut(vector3, forse, false, false, !zombie.skeleton);
            if (Random.value < _Hud.stats.Knockback / 100f)
            {
                _Game.Text("Отлет", zombie.upPos, Color.yellow, 2);
                knockout = true;
            }
            if (Random.value < _Hud.stats.SlowDown / 100f)
            {
                _Game.Text("Замедление", zombie.upPos, Color.blue, 2);
                zombie.slowDownTime = Time.time;
            }

            if (Random.value < _Hud.stats.Crtitical / 100f)
            {
                _Game.Text("Критический", zombie.upPos, Color.red, 2);
                damage *= 2;
            }
        }
        destroyable.SetLife(damage, vector3, forse, knockout);
    }
    private void UpdateLaser()
    {
        bool hit = false;
        if (isAttacking && CurrentCombo == Combo.spellCast && CurrentWeapon.weaponType2 == WeaponType.Laser)
        {
            if (LaserStream == null)
            {
                LaserStream = _Game.LaserStream;
                LaserStream.gameObject.SetActive(true);
                LaserStream.GetComponentInChildren<ParticleSystem>().Play();
                foreach (var a in LaserStream.GetComponentsInChildren<Animation>())
                    a.Play("Open");
            }
            LaserStream.position = LeftHand.transform.position + forward * .2f;
            LaserStream.forward = forward;
            RaycastHit h;
            float di = 20;
            if (Physics.SphereCast(upPos - forward * .5f, 1, model.forward, out h, 1000, 1 << (int)Layer.Enemy | 1 << (int)Layer.Dead))
            {
                if (h.rigidbody != null && !h.rigidbody.isKinematic)
                    h.rigidbody.AddForceAtPosition(forward * 200, h.point);
             
                foreach (Destroyable z in _Game.destroyables)
                {
                    Debug.DrawLine(z.upPos, h.point);
                    if (z.alive && ZeroY(z.upPos - h.point).magnitude < 2)
                    {
                        z.transform.position = z.pos += forward*6*Time.deltaTime/z.Mass;
                        z.SetLife(50*Time.deltaTime, h.point, 200);
                    }
                }
                h.point -= forward * .5f;
                di = h.distance - .5f;
                if (LaserStreamEnd == null)
                {
                    LaserStreamEnd = _Game.LaserStreamEnd;
                    LaserStreamEnd.gameObject.SetActive(true);
                    LaserStreamEnd.GetComponentInChildren<ParticleSystem>().Play();
                    LaserStreamEnd.GetComponentInChildren<Light>().enabled = true;
                }
                hit = true;
                LaserStreamEnd.position = upPos + forward * (h.distance + .5f);
            }
            LaserStream.transform.Find("line").localScale = new Vector3(1, 1, di);
        }
        else
        {
            if (LaserStream != null)
            {
                LaserStream.GetComponentInChildren<ParticleSystem>().Stop();
                foreach (var a in LaserStream.GetComponentsInChildren<Animation>())
                    a.Play("Close");
                LaserStream = null;
            }
        }
        if (LaserStreamEnd != null && !hit)
        {
            LaserStreamEnd.GetComponentInChildren<ParticleSystem>().Stop();
            LaserStreamEnd.GetComponentInChildren<Light>().enabled = false;
            LaserStreamEnd = null;
        }
    }
    private void UpdatePhysx()
    {
        var an = _Game.PhysxEffectAnim;
        var phys = _Game.PhysxEffect.gameObject;
        _Game.PhysxEffect.position = upPos + mouseDir.normalized * 1.5f;
        if (CurrentWeapon.weaponType2 == WeaponType.physx)
        {
            if (Input.GetMouseButtonDown(1))
            {
                an.PlayQueued("start", QueueMode.PlayNow);
                an.PlayQueued("loop");
                phys.SetActive(true);
            }
            if (Input.GetMouseButtonUp(1))
            {
                an.PlayQueued("release", QueueMode.PlayNow);
                if (holdingZombie != null)
                    StartCoroutine(MoveObject(_Game.PhysxEffect, mouseDir.normalized * 5, .5f));
                StartCoroutine(AddMethod(() => !an.isPlaying, delegate { phys.SetActive(false); }));
            }
        }
        if (isAttacking && CurrentCombo == Combo.spellCastUpper && CurrentWeapon.weaponType2 == WeaponType.physx)
        {
            if (holdingZombie == null)
            {
                Destroyable z = null;
                float oldDist = float.MaxValue;
                foreach (Destroyable z2 in _Game.destroyables)
                    if (z2 is Zombie || z2 is Barrel)
                    {
                        if (z2.alive && z2.haveRigs && ((pos + mouseDir.normalized * 3) - z2.pos).magnitude < 3)
                        {
                            var dist = (pos - z2.pos).magnitude;
                            if (dist < oldDist)
                            {
                                z = z2;
                                oldDist = dist;
                            }
                        }
                    }
                if (z != null)
                {
                    holdingZombie = z;
                    var zombie = z as Zombie;
                    if (zombie != null)
                    {
                        zombie.Die(Vector3.zero, 0);
                        foreach (Rigidbody a in zombie.rigidBodies)
                            if (a != null)
                                a.drag = 6;
                    }
                    else
                    {
                        if (!z.rigidbody)
                            z.gameObject.AddComponent<Rigidbody>();
                        z.rigidbody.drag = 6;
                    }
                }
            }
            else
            {
                var zombie = holdingZombie as Zombie;
                var barrel = holdingZombie as Barrel;
                if (zombie != null)
                {
                    zombie.dieTime = Time.time;
                    var vector3 = (pos + (zombie.Mass * Vector3.up + mouseDir.normalized * 1.5f) - zombie.spine.position);
                    zombie.spine.rigidbody.velocity = ((vector3 * 200 + vector3.normalized * 40) * .1f);
                    if (!Input.GetMouseButton(1))
                        StartCoroutine(zombie.ThrowZombie((_Player.mouseDir.normalized + Vector3.down * .3f) * 15));
                }
                else if (barrel != null && barrel.enabled)
                {
                    var vector3 = (pos + (Mass * Vector3.up + mouseDir.normalized * 1.5f) - barrel.transform.position);
                    barrel.transform.rigidbody.velocity = ((vector3 * 200 + vector3.normalized * 40) * .05f);
                    if (!Input.GetMouseButton(1))
                    {
                        barrel.rigidbody.drag = 0;
                        barrel.rigidbody.velocity = ((_Player.mouseDir.normalized + Vector3.down * .3f) * 15);
                    }
                }
                if (!Input.GetMouseButton(1))
                {
                    currentCombo = 0;
                    holdingZombie = null;
                }
            }
        }
    }
    private void createBullet(bool fire = false)
    {
        var instantiate = (GameObject)Instantiate(CurrentWeapon.bullet, upPos, Quaternion.LookRotation(mouseDir.normalized + ZeroY(Random.insideUnitSphere * CurrentWeapon.accuraty)));
        if (_Loader.hideEffects)
            instantiate.hideFlags = HideFlags.HideInHierarchy;
        var arrow = instantiate.GetComponent<Arrow>();
        arrow.fire = fire;
        arrow.damage = CurrentWeapon.damage;
        this.lastArrow = arrow;
        Destroy(instantiate, 5);
    }
    public override void Die(Vector3 h, float forse)
    {
        dieTime = Time.time;
        foreach (var a in rigidBodies)
            if (a != null)
                a.isKinematic = false;
        foreach (var a in colliders)
            if (a != null)
            {
                a.enabled = a != modelCollider;
                if (a != modelCollider)
                    a.gameObject.layer = (int)(base.life < 0 ? Layer.Dead : Layer.bones);
            }
        
        StartCoroutine(FreezeCor(.5f));

    }

    public IEnumerator FreezeCor(float maxvel = .05f)
    {
        model.GetComponent<Animator>().enabled = false;
        enabled = false;
        while (hips.rigidbody != null && hips.rigidbody.velocity.magnitude > maxvel ||Time.time - dieTime < (life > 0 ? 1 : 5))
            yield return null;

        Reset();
        PlayAnim(Combo.standUp);
        enabled = true;
        model.GetComponent<Animator>().enabled = true;
        //if (life > 0)
        //{
            //StartCoroutine(StandUp());
            //yield break;
        //}
        //alive = false;
    }


    //private IEnumerator StandUp()
    //{
        
    //    yield return new WaitForSeconds(1);
    //    //alive = true;

    //}

    public Arrow lastArrow;
    private Vector3 oldpos;
    private void UpdateOther()
    {
        RaycastHit h;
        //if (oldpos != Vector3.zero && oldpos != pos && Physics.Raycast(oldpos + Vector3.up * .3f, pos, out h, (oldpos - pos).magnitude, 1 << (int)Layer.bones))
        if (oldpos != Vector3.zero && oldpos != pos && Physics.SphereCast(oldpos, .6f, pos - oldpos, out h, (oldpos - pos).magnitude, 1 << (int)Layer.bones | 1 << (int)Layer.Dead))
        {
            if (h.rigidbody == null) Debug.Log(h.transform.name, h.transform);
            if (h.rigidbody != null && !h.rigidbody.isKinematic)
                h.rigidbody.AddForce(ZeroY(pos - oldpos).normalized * 500 * h.rigidbody.mass);
        }
        oldpos = pos;
        _Hud.LifeSlider.sliderValue = life / _Hud.MaxLife;
        bool runSound = anim.GetFloat("RunSound") > .5f;
        if (runSound != oldRunSound)
            PlaySound(RunSounds[Random.Range(0, RunSounds.Length - 1)]);
        oldRunSound = runSound;
        if (Input.GetKeyDown(KeyCode.O))
            sleep = -1000;
        else if (Input.anyKeyDown)
            sleep = Time.time;
        if (Time.time - sleep > 60)
            anim.SetInteger("Sleep", Random.Range(1, 4));
        else
            anim.SetInteger("Sleep", 0);
    }
    private float angle;
    private bool oldAIm;
    private bool runBackwards;
    public void LateUpdate()
    {
        isAiming = Time.time - attackTime < 1.5f && hitAndRun;
        anim.SetBool("aim", isAiming);
        var fw = forward;
        runBackwards = false;
        if (isAiming && hitAndRun)
        {
            angle = AngleSigned(fw, mouseDir, Vector3.up);
            runBackwards = Mathf.Abs(angle) > 90;
            if (runBackwards)
                fw = -fw;
            var angle2 = AngleSigned(fw, mouseDir, Vector3.up) + CurrentWeapon.rotationOffset;
            spine.RotateAround(Vector3.up, angle2 * .5f * Mathf.Deg2Rad);
            spine2.RotateAround(Vector3.up, angle2 * .5f * Mathf.Deg2Rad);
        }
        else
        {
            angle = Mathf.Lerp(isAiming ? angle : Mathf.Clamp(angle, -90, 90), AngleSigned(fw, mouseDir, Vector3.up), 10 * Time.deltaTime);
            spine.RotateAround(Vector3.up, angle * .1f * Mathf.Deg2Rad);
            spine2.RotateAround(Vector3.up, angle * .1f * Mathf.Deg2Rad);
            head.RotateAround(Vector3.up, angle * .3f * Mathf.Deg2Rad);
        }
        anim.SetBool("runBackwards", runBackwards && moveDir.magnitude > .5f);
        anim.SetBool("runForward", !runBackwards && moveDir.magnitude > .5f);
        if (fw != Vector3.zero)
            model.forward = fw;
    }
    public WeaponSettings CurrentWeapon { get { return curWeaponItem.weaponSettings; } }
    public void SelectWeapon(Item ls)
    {
        curWeaponItem = ls ?? defWeapon;
        foreach (Transform a in RightHand.transform)
            Destroy(a.gameObject);
        if (curWeaponItem.weaponPrefab != null)
        {
            GameObject original = curWeaponItem.weaponPrefab;
            var Weapon = (GameObject)Instantiate(original, upPos, RightHand.rotation);
            Weapon.transform.parent = RightHand.transform;
            Weapon.transform.localPosition = original.transform.localPosition;
            Weapon.transform.localRotation = original.transform.rotation;
        }
        anim.SetFloat("idle", (int)CurrentWeapon.WeaponIdle);
        anim.SetLayerWeight((int)AnimLayer.damage, 1);
        anim.SetLayerWeight((int)AnimLayer.Special, 1);
        anim.SetLayerWeight((int)AnimLayer.SpecialUpper, 1);
        foreach (var a in _Game.wepSettings)
            anim.SetLayerWeight((int)a.layer, 0);
        anim.SetLayerWeight((int)CurrentWeapon.layer, 1);
    }
    public float attackTime;
    public float attackTimeStart;
    public override void SetLife(float damage, Vector3 h, float forse = 100, bool knockout = false)
    {
        _Game.Text(((int)damage).ToString(), upPos, Color.red);
        if (!_Loader.Immortal)
            life -= damage;
        var u = life / _Hud.MaxLife;
        if (u < .7f)
            renderer.material.mainTexture = bloodTextures[u > .3 ? 0 : 1];
        damageAnim = 1;
        PlaySound(ArrayRandom(damageSound));
    }
    
    private int damageAnim;
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == (int)Layer.Coin && other.gameObject.GetComponent<DroppedItem>().coin)
            Destroy(other.gameObject);
    }
    public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
    {
        return Mathf.Atan2(
            Vector3.Dot(n, Vector3.Cross(v1, v2)),
            Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }

    private Action hitAction;
    private void PlayAnim(Combo currentCombo, Action a = null)
    {
        PlayAnim((int)currentCombo, a);
    }
    private void PlayAnim(int currentCombo,Action a=null)
    {
        //isAttacking = true;
        hitAction = a;
        attackTimeStart = Time.time;
        this.currentCombo = currentCombo;
        
        anim.SetInteger("Attack", currentCombo);
    }
}
