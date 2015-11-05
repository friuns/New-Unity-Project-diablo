using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Collections;

//public enum SpriteNames { None, _2DInventoryMightyWeapon_1H,_2DInventoryCrossbows }
public class Database : Bs
{
    public GameObject explosionPrefab;
    public Shader[] lodShaders;
    public PhysicMaterial bonchy;
    public PhysicMaterial ice;
    public WeaponSettings defWep;
    public Shader shader;
    public GameObject DropAnimation;
    public List<Item> items = new List<Item>();
    public GameObject pouringBlood;
    public GameObject heal;
    public GameObject heal2;
    public GameObject deathExplosionBase;
    public GameObject Hit_Poison_Chest;
    public Monkey monkey;
    public ParticleSystem moveEffect;
    public void Start()
    {
    }
}
public enum Combo { a0, a1, a2, a3, a4, a5, a6, a7, a8, a9, sword = 10, hammer = 11, spellCast = 12, hammer2 = 13, thrown = 14, spellCastUpper = 15, standUp = 16 }
public enum Quality
{
    Damaged,
    Worn,
    Sturdy,
    Polished,
    Improved,
    Cursed,
    Superior,
    Elite,
    Enchanted,
    Magic,
    Epic,
    Legendary
}

public enum Layer
{
    Enemy = 8, Level = 9, Dead = 10, Coin = 11, CursorPlane = 14,Destroyed=16,bones=17,Player=15
}
public enum AnimLayer
{
    Base, Run, damage, Attack, FistAttack, RifleAttack, gunAttack, Hammer, Special, wand, SpecialUpper
}
//public enum Sleep
//{
//    Sit, Dance
//}
public enum WeaponIdle
{
    Sword, Fist, Sword2
}
//public enum ZombieModel { zombie, skeleton, fastZombie }
//[Serializable]
//public class ZombieSettings
//{
    
//    public float animSpeed = 1;
//    public float speed;
//    public int life;
//    public bool runing;
//    public GameObject modelSkin;
//    public GameObject[] weapons;
//    public enum ModelGroup { undead }
//    //internal ModelGroup modelGroup;    
//}
//public enum WeaponID { sword, fist, crossBow, gun }
//public enum WeaponName { sword, fist, crossBow, gun, Chest_Armor }

//public enum WepGroupEnum { Sword, Fist, crossBow }




public enum SlotType { Inventory, Head, Torso, Wrists, Hands, lFinger, rFinger, Waist, Feet, Legs, Hand, Offhand, Shoulders, Neck }


public class LabelAttribute :
#if UNITY_EDITOR
 PropertyAttribute
#else
    Attribute
#endif
{
    public string d = "";
    public LabelAttribute(string s)
    {
        d = s;
    }
}
