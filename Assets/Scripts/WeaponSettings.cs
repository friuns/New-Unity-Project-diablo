using System;
using UnityEngine;

public class WeaponSettings : Item
{
    [LabelAttribute("WeaponSettings")]
    public string asd;
    public int damage = 10;
    public float lenth = 3;
    public float ShootInterval = -1;
    public float radius = 70;
    public float rotationOffset;
    //public WeaponID Name;
    public string Layer;
    public AnimLayer layer
    {
        get
        {
            return (AnimLayer)Enum.Parse(typeof(AnimLayer), Layer);
        }
    }
    //public WepGroupEnum WeaponGroupEnum;
    //public WeaponGroup sets;
    public GameObject bullet;
    //public string weaponPrefab;
    public WeaponType weaponType;
    public WeaponType weaponType2;
    public float accuraty;
    public int[] combos;
    public Combo combosSpecial = Combo.a9;
    public AudioClip[] damageSound;
    public WeaponIdle WeaponIdle = WeaponIdle.Sword;
    public bool hitAndRun = true;
    public bool hitAndRunSpecial = true;
}
public enum WeaponType { melee, Bow, thrown, Laser, physx }