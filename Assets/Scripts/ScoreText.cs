using UnityEngine;
using System.Collections;

public class ScoreText : MonoBehaviour
{
    private Vector3 r;
    private Color c;
    void Start()
    {
        c = renderer.material.color;
        r = Bs.ZeroY(Random.insideUnitSphere)*rand;
    }

    public float rand;
    public Vector3 vel;
    public float time=1;
    void Update()
    {
        time -= Time.deltaTime;
        transform.position += r * Time.deltaTime * 10 + vel * Time.deltaTime;
        vel += Physics.gravity * Time.deltaTime;
        renderer.material.color = c * time;
        //transform.LookAt(Bs._ObsCamera.camera.transform.position);
    }
}
