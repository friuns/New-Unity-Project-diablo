using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum StatEnum //ARCANEHit, COLDHit, FIREHit, HOLYHit, LIGHTNINGHit, POISONHit, 
{
    Strength = 0, Dexterity = 1, Intelligence = 2, Vitality = 3, Armor = 4, Damage = 5, Knockback = 6, Critical = 7, SlowDown = 8
}

[Serializable]
public class Stat
{
    internal float[] stats = new float[types.Length];
    internal int Strength { get { return (int)stats[0]; } set { stats[0] = value; } }
    internal int Dexterity { get { return (int)stats[1]; } set { stats[1] = value; } }
    internal int Intelligence { get { return (int)stats[2]; } set { stats[2] = value; } }
    internal int Vitality { get { return (int)stats[3]; } set { stats[3] = value; } }
    internal int Armor { get { return (int)stats[4]; } set { stats[4] = value; } }
    internal float Damage { get { return stats[5]; } set { stats[5] = value; } }
    internal int Knockback { get { return (int)stats[6]; } set { stats[6] = value; } }
    internal int Crtitical { get { return (int)stats[7]; } set { stats[7] = value; } }
    internal int SlowDown { get { return (int)stats[8]; } set { stats[8] = value; } }
    internal string[] texts = new string[types.Length];
    internal bool[] noRand = new bool[types.Length];
    internal bool[] wepOnly = new bool[types.Length];
    internal bool[] handWepOnly = new bool[types.Length];
    private static StatEnum[] types = (StatEnum[])Enum.GetValues(typeof(StatEnum));

    public Stat()
    {
        texts[0] = "{0} К Силе";
        texts[1] = "{0} К Ловкости";
        texts[2] = "{0} К Интеллекту";
        texts[3] = "{0} К Живучести";
        texts[4] = "{0} К Броне";
        texts[5] = "{0} К Урону";
        texts[6] = "{0}% шанса Отталкивания";
        texts[7] = "{0}% шанса Критического удара";
        texts[8] = "{0}% шанса Замедлению";

        noRand[5] = noRand[4] = true;
        handWepOnly[8] = handWepOnly[6] = true;
    }
    public void Rand(int points, Item item)
    {
        while (points > 0)
        {
            var r = Mathf.Max(points, Random.Range(1, points * 2));
            int st = 0;
            st = Random.Range(0, stats.Length);
            if (!noRand[st] && (!wepOnly[st] || item.isWeapon) && (!handWepOnly[st] || item.isWeapon && !(item.weaponSettings.weaponType == WeaponType.Bow)))
            {
                stats[st] += r;
                points -= r;
            }
        }
    }
}