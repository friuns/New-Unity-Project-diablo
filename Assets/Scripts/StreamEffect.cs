using UnityEngine;
using System.Collections;

public class StreamEffect : MonoBehaviour
{
    public float speed = .5f;
    public float scale = 5;
	void Update ()
	{
        if (renderer.enabled)
            foreach (var m in renderer.materials)
            {
                var mainTextureOffset = m.mainTextureOffset;
                mainTextureOffset.y += Time.deltaTime * speed;
                m.mainTextureOffset = mainTextureOffset;
                var f = transform.lossyScale.y / scale;
                renderer.material.mainTextureScale = new Vector2(1, f);
            }
    }
}
