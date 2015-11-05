using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Forward : MonoBehaviour
{
    public Transform receiver;
    //public List<BoneWeight2> boneWeights;
    public void Start()
    {
        var an = animation;
        if (an != null && Application.isEditor) an.cullingType = AnimationCullingType.AlwaysAnimate;
        if (receiver == null) receiver = transform.root;
    }
    //public void OnBecameInvisible()
    //{
    //    receiver.SendMessage("OnBecameInvisible",SendMessageOptions.DontRequireReceiver);
    //}
    public void onAttack()
    {
        receiver.SendMessage("onEvent", SendMessageOptions.DontRequireReceiver);
    }
    public void onThrow()
    {
        receiver.SendMessage("onEvent", SendMessageOptions.DontRequireReceiver);
    }
    public void OnEvent()
    {
        receiver.SendMessage("onEvent", SendMessageOptions.DontRequireReceiver);
    }
    //public void AnimEvent2()
    //{
    //    receiver.SendMessage("AnimEvent2", SendMessageOptions.DontRequireReceiver);
    //}
    //public void AnimEvent3()
    //{
    //    receiver.SendMessage("AnimEvent3", SendMessageOptions.DontRequireReceiver);
    //}

    internal Collision collision;
    public void OnCollisionEnter(Collision collision)
    {
        this.collision = collision;
        receiver.SendMessage("OnCollision", this,SendMessageOptions.DontRequireReceiver);
    }
}
