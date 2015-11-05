using UnityEngine;
using System.Collections;

public class DroppedItem : Bs
{
    public bool coin;
    public UILabel label;
    public Item item;
    public Renderer[] renderers;
    private Shader defaultShader;
    public void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        defaultShader = renderers[0].sharedMaterial.shader;
        base.Awake();
    }
    public void Shine(bool v)
    {
        foreach (var a in renderers)
        {
            if (v)
                a.material.shader = _Database.shader;
            else
                a.material.shader = defaultShader;
        }
    }
}
