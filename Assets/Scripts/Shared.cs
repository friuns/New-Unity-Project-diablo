using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Shared : Destroyable
{
    internal GameObject fire;
    public bool isAttacking;
    //internal FS_ShadowSimple fsShadowSimple;
    public Transform LeftHand;
    public Transform RightHand;
    //internal List<Rigidbody> rigidBodies = new List<Rigidbody>();
    //internal Transform[] transforms;
    internal Transform model;
    public float life;
    public override bool haveRigs { get { return rigidBodies.Length > 0; } }
    

    internal bool visible = true;
    public enum ModelGroup { undead }
    internal float maxlife;
    public ModelGroup modelGroup;
    public Transform hips;
    public Vector3 hipsPos { get { return hips == null ? pos : hips.position; } }
    internal Transform head;
    public Transform spine;
    public Transform spine2;

    protected Rigidbody[] m_rigidBodies2;
    internal Rigidbody[] rigidBodies { get { return m_rigidBodies2 == null || (m_rigidBodies2.Length > 0 && m_rigidBodies2[0] == null) ? (m_rigidBodies2 = model.   GetComponentsInChildren<Rigidbody>()) : m_rigidBodies2; } }
    protected Collider[] m_colliders;
    internal Collider[] colliders { get { return m_colliders == null || (m_colliders.Length > 0 && m_colliders[0] == null) ? (m_colliders = model.GetComponentsInChildren<Collider>()) : m_colliders; } }

    private Transform[] m_transforms;
    internal Transform[] transforms { get { return m_transforms == null || m_transforms[0] == null ? (m_transforms = model.GetComponentsInChildren<Transform>()) : m_transforms; } }
    internal  Collider modelCollider { get { return model.collider != null ? model.collider : collider != null ? collider : skinnedMeshRenderer.collider; } }
    internal SkinnedMeshRenderer skinnedMeshRenderer;
    public float dieTime;
    public Vector3 Spos { get; set; }
    public void setPos(Vector3 p)
    {
        transform.position = pos = Spos = p;
    }

    internal void Reset()
    {
        setPos(ZeroY(hips.position));
        hips.localPosition = new Vector3(0, hips.localPosition.y, 0);
        gameObject.SetActive(true);
        foreach (Rigidbody a in rigidBodies)
            if (a != null)
            {
                a.isKinematic = true;
                a.drag = 0;
            }
        if (_Loader.rigOpt != OptState.hardRig)
            foreach (Collider a in colliders)
                if (a != null)
                    a.enabled = false;
        modelCollider.enabled = true;
        if (this is Zombie)
            asZombie.anim.enabled = true;
    }

    protected void InitOptimize()
    {
        //if (_Loader.disableRigidBodies)
        //{
        //    foreach (var a in this.GetComponentsInChildren<Joint>())
        //        Destroy(a);
        //    foreach (Rigidbody a in this.GetComponentsInChildren<Rigidbody>())
        //        Destroy(a);
        //}

        //if (_Loader.rigOpt != OptState.saverig)
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        foreach (Collider a in colliders)
        {
            if (a != modelCollider)
            {
                a.material = _Database.bonchy;
                a.gameObject.layer = (int)Layer.bones;
                if (_Loader.rigOpt == OptState.rig)
                    a.enabled = false;
            }
        }
        foreach (var a in rigidBodies)
        {
            if (_Loader.rigOpt == OptState.saverig && this is Zombie)
                asZombie.Save(a.transform);
            else if (_Loader.rigOpt == OptState.hardRig || _Loader.rigOpt == OptState.rig)
            {
                a.isKinematic = true;
                a.gameObject.AddComponent<Forward>().receiver = transform;
            }
        }
        modelCollider.enabled = _Loader.rigOpt != OptState.hardRig;
    }

    private Zombie asZombie
    {
        get { return (this as Zombie); }
    }


    public void CreateBloodSplatter()
    {
        RaycastHit h2;
        if (Physics.Raycast(hips.position + Vector3.up, Vector3.down, out h2, 10, 1 << (int)Layer.Level))
        {
            var bl = (GameObject)Instantiate(_Game.bloodSplatter, h2.point + Vector3.up * .02f, Quaternion.Euler(0, Random.Range(0, 360), 0));
            bl.transform.localScale *= Random.Range(1, 2);
            bl.transform.renderer.sharedMaterial = _Game.bloodTextures[Random.Range(0, _Game.bloodTextures.Length - 1)]; //todo Shared.CreateBloodSplatter:48 
        }
    }
    public virtual void SetFire()
    {
    }

    public void SetLife(float damage = 5)
    {
        SetLife(damage, Vector3.zero, 0);
    }
    public override void SetLife(float damage, Vector3 h, float forse = 100, bool knockout = false)
    {
        base.SetLife(damage, h, forse, knockout);
    }
}
