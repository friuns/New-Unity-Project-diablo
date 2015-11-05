using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class Dark : Bs{

    private int cur;
    internal DayState fr;
    internal DayState to;
    public DayState[] dayStates;
    public List<GameObject> graves;
    public GameObject lightningPrefab;
	public void Start ()
	{
        foreach (DayState a in dayStates)
        {
            foreach (var light in a.lights)
                a.lightdefs.Add(light.intensity);
        }
        if (_Loader.enableGraves)
            CreateGraves();
    }

    private void CreateGraves()
    {
        for (int j = 0; j < 100; j++)
        {
            var original = graves[Random.Range(0, graves.Count)];
            var g = Instantiate(original, ZeroY(Random.insideUnitSphere) * Zombie.AvatarRange * 2 + original.transform.position, Quaternion.Euler(0, Random.Range(0, 360), 0));
            if (_Loader.hideEffects)
                g.hideFlags = HideFlags.HideInHierarchy;
        }
    }

    public Light lightning;
    public bool night { get { return dayStates[cur].night; } }
    IEnumerator Lighning()
    {
        //var zeroY = ZeroY(Random.insideUnitSphere);
        //if (zeroY.z > 0) zeroY.z *= 2;
        
        if (_Game.visibleZombies.Count == 0) yield break;
        var z = _Game.visibleZombies[Random.Range(0, _Game.visibleZombies.Count)];
        z.SetLife(10, z.pos, 700,true);
        //var position = _Player.pos + zeroY.normalized + zeroY*5;
        Instantiate(lightningPrefab, z.pos, Quaternion.identity);
        print("Lightning");
        lightning.transform.forward = Vector3.down + -(z.pos - _Player.pos).normalized * 4;
        lightning.gameObject.SetActive(true);
        yield return new WaitForSeconds(.05f);

        if (Random.value > .2f)
        {
            lightning.gameObject.SetActive(false);
            yield return new WaitForSeconds(.1f);
            lightning.gameObject.SetActive(true);
            yield return new WaitForSeconds(.05f);
            lightning.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(.05f);
            lightning.gameObject.SetActive(false);
        }
    }

    private float nextLightning;
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            CreateGraves();
        if (cur == 1 || Input.GetKeyDown(KeyCode.L))
        {
            if (Input.GetKeyDown(KeyCode.L) || Time.time - nextLightning > 0)
            {
                if (nextLightning != 0)
                    StartCoroutine(Lighning());
                
                nextLightning = Time.time + Random.Range(5, 60);
            }
        }
        if (Input.GetKeyDown(KeyCode.C))
            Switch();
        if (fr != null)
        {
            bool progress = false;
            for (int i = 0; i < fr.lights.Count; i++)
            {
                fr.lights[i].intensity = Mathf.Lerp(fr.lights[i].intensity, 0, Time.deltaTime * 3);
                if (fr.lights[i].intensity < .01)
                    fr.lights[i].intensity = 0;
                else
                    progress = true;
            }
            if (!progress)
            {
                foreach (var a in fr.lights)
                    a.enabled = false;                
                fr = null;
            }
        }

        if (to != null)
        {
            bool progress = false;
            for (int i = 0; i < to.lights.Count; i++)
            {
                to.lights[i].intensity = Mathf.Lerp(to.lights[i].intensity, to.lightdefs[i], Time.deltaTime * 3);
                if (to.lightdefs[i] - to.lights[i].intensity < .1f)
                    to.lights[i].intensity = to.lightdefs[i];
                else
                    progress = true;
            }
            if (!progress)
                to = null;
        }
    }

    private void Switch()
    {
        fr = dayStates[cur];
        int tmp = cur;


        to = dayStates[(tmp + 1)%dayStates.Length];
        foreach (var a in to.lights)
        {
            a.intensity = 0;
            a.enabled = true;
        }
        cur = (cur + 1)%dayStates.Length;


        //StartCoroutine(Bs.AddMethod(.2f, delegate
        //{
        foreach (var a in fr.objs)
            a.SetActive(false);
        foreach (var a in to.objs)
            a.SetActive(true);
        //}));
        RenderSettings.fog = _ObsCamera.spec && night;
    }

    [Serializable]
    public class DayState
    {
        public string name;
        public bool night;
        //public Transform obj;
        public List<Light> lights = new List<Light>();

        internal List<float> lightdefs=new List<float>();
        public GameObject[] objs;
    }
}
