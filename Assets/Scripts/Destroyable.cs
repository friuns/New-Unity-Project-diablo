using UnityEngine;
using System.Collections;

public class Destroyable : Bs {
    

    public virtual bool haveRigs { get { return true; }}
    internal bool demolish;
    internal bool cut;
    public virtual void SetLife(float damage, Vector3 h, float forse = 100, bool knockout = false)
    {
        
    }
    public float Mass = 1.5f;
    public float size = 1.5f;
    public Vector3 upPos { get { return pos + Vector3.up * Mass; } }
    public virtual bool alive { get { return enabled; } set { enabled = value; } }
    private Rigidbody[] m_rigidBodies;

    public virtual void Die(Vector3 h, float forse)
    {
        
    }
}
