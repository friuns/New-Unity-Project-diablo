using UnityEngine;
using System.Collections;

public class Rock : MonoBehaviour
{
    //private Rigidbody r;
    void Start()
    {
        //r = this.rigidbody;
    }
    //void Update ()
    //{
        //if (r != null && r.velocity.magnitude > 10)
    //    transform.position += Bs.ZeroY(Bs._Player.pos - transform.position).normalized * 6 * Time.deltaTime;
    //}
    private bool hit;
    private void OnCollisionEnter(Collision collision)
    {
        if (!hit)
            StartCoroutine(Bs.AddMethod(.2f, delegate
            {
                foreach (Transform t in transform)
                    t.gameObject.SetActive(false);
            }));
        hit = true;

        var s = collision.transform.root.GetComponent<Shared>();
        var z = s as Zombie;
        if (z != null)
            z.SetLife(10);

        var p = s as Player;
        if(p!=null)
            p.SetLife(10);
        
    }

    public IEnumerator MaterialFade(float fade = .5f)
    {
        Color color = renderer.material.color;
        Color color2 = renderer.material.color * fade;
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(.3f);
            foreach (var m in renderer.materials)
                m.color = Color.Lerp(color, color2, i / 10f);
        }
    }

}
