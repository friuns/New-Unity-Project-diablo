using UnityEngine;

public class Item : MonoBehaviour
{
    internal string Name;
    public Quality quality;
    internal Texture2D Texture;
    public Texture2D[] Textures = new Texture2D[1];
    public string[] Names = new string[1];
    public bool playerHave;
    public float DamagePerSecond = 8;
    public float attacksPerSecond = 1.4f;
    internal int Damage;
    internal int sellValue;
    internal int durability;
    internal int itemLevel;
    public int MagicPoints;
    public int Armor;
    public Stat stats = new Stat();
    public int itemLevelMin = 1;
    public int itemLevelMax = 5;
    
    public string weaponSubName;
    public SlotType slotType;
    public bool TwoHanded;
    public GameObject weaponPrefab;
    public WeaponSettings weaponSettings { get { return this as WeaponSettings; } }
    public bool isWeapon { get { return slotType == SlotType.Hand; } }

    public void Init()
    {
        var r = Random.value;
        quality = r > .6f ? quality : r > .3f ? quality + 1 : r > .1f ? quality + 2 : quality + 3;
        if ((int)quality > 11) quality = Quality.Legendary;
        itemLevel = Random.Range(itemLevelMin, itemLevelMax);
        sellValue = (int)(Random.Range(8, 10) * itemLevel * qualityMultiplier) + (int)DamagePerSecond;
        durability = Random.Range(8, 10) * itemLevel * (int)quality + 1;
        if (isWeapon)
        {
            attacksPerSecond = Mathf.Round(attacksPerSecond + Random.Range(-attacksPerSecond * .2f, attacksPerSecond * .2f));
            DamagePerSecond = DamagePerSecond + Random.Range(-DamagePerSecond * .2f, DamagePerSecond * .2f);
            DamagePerSecond = Mathf.Round(DamagePerSecond * (qualityMultiplier + ((itemLevel - itemLevelMin) * .1f)));
            if (isWeapon)
                stats.stats[(int)StatEnum.Damage] = (int)DamagePerSecond;
            else
                stats.stats[(int)StatEnum.Armor] = Armor;
        }

        Damage = (int)(DamagePerSecond / attacksPerSecond);
        var i = Random.Range(0, Textures.Length);
        Texture = Textures[i];
        Name = Names[i];
        MagicPoints = (int)(MagicPoints * qualityMultiplier);
        stats.Rand(MagicPoints, this);
    }

    public override string ToString()
    {
        return Names[0];
    }
    public float qualityMultiplier
    {
        get
        {
            float mult = 0f;
            switch (quality)
            {
                case Quality.Cursed:
                    mult = 2f;
                    break;
                case Quality.Damaged:
                    mult = 0.25f;
                    break;
                case Quality.Worn:
                    mult = 0.9f;
                    break;
                case Quality.Sturdy:
                    mult = 1f;
                    break;
                case Quality.Polished:
                    mult = 1.1f;
                    break;
                case Quality.Improved:
                    mult = 1.25f;
                    break;
                case Quality.Superior:
                    mult = 1.5f;
                    break;
                case Quality.Elite:
                    mult = 1.75f;
                    break;
                case Quality.Enchanted:
                    mult = 2f;
                    break;
                case Quality.Epic:
                    mult = 2.5f;
                    break;
                case Quality.Legendary:
                    mult = 3f;
                    break;
            }
            return mult;
        }
    }

    public Color color
    {
        get
        {
            Color c = Color.white;
            switch (quality)
            {
                case Quality.Cursed:
                    c = Color.red;
                    break;
                case Quality.Damaged:
                    c = new Color(0.4f, 0.4f, 0.4f);
                    break;
                case Quality.Worn:
                    c = new Color(0.7f, 0.7f, 0.7f);
                    break;
                case Quality.Sturdy:
                    c = new Color(1.0f, 1.0f, 1.0f);
                    break;
                case Quality.Polished:
                    c = NGUIMath.HexToColor(0xe0ffbeff);
                    break;
                case Quality.Improved:
                    c = NGUIMath.HexToColor(0x93d749ff);
                    break;
                case Quality.Elite:
                    c = NGUIMath.HexToColor(0x4eff00ff);
                    break;
                case Quality.Superior:
                    c = NGUIMath.HexToColor(0x00baffff);
                    break;
                case Quality.Enchanted:
                    c = NGUIMath.HexToColor(0x7376fdff);
                    break;
                case Quality.Magic:
                case Quality.Epic:
                    c = NGUIMath.HexToColor(0x9600ffff);
                    break;
                case Quality.Legendary:
                    c = NGUIMath.HexToColor(0xff9000ff);
                    break;
            }
            return c;
        }
    }

}