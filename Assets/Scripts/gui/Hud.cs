using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;



public class Hud : Bs
{
    public GameObject ngui;
    public UISlider progressBar;
    public UILabel progressBarTitle;
    public UILabel progressBarTitle2;
    public GUIText text2;
    public UILabel LevelField;
    public int Level;
    public int oldLevel;
    internal int Level1 { get { return Level + 1; } }
    public UILabel StrengthField;

    public Stat stats = new Stat();

    public UILabel DexterityField;
    public UILabel IntelligenceField;
    public UILabel VitalityField;
    public UILabel ArmorField;
    public int MaxLife = 36;
    public UILabel moneyField;
    public UISlider LifeSlider;
    public UISlider ManaSlider;
    public float Mana;
    public UILabel DamageField;
    //internal float Damage = 8;
    public GameObject Inventory;
    public InvInfo InvInfo1;
    public InvInfo InvInfo2;

    private int[] expNeeded = new int[] { 1200, 2700, 4500, 6600, 9000, 11700, 14700, 18750, 23200, 28050 };
    public int xp;

    public void Start()
    {
        ToolTip(LevelField, () => string.Format("[C7B377]Опыт на уровне {0}:[-]{1}/{2}", Level1, xp, expNeeded[Level]));
        ToolTip(StrengthField, () => string.Format("[C7B377]Сила :[-][00DD00]{0}[-]\n#Основная xарактеристика для варваров.\n#Увеличивает наносимый етими героями урон на [00DD00]{0}%[-]\n#Повышает показатель брони на [00DD00]{0}[-] ед.", stats.Strength));
        ToolTip(DexterityField, () => string.Format("[C7B377]Ловкость: [-][00DD00]{0}[-]\n#Основная xарактеристика для оxотников на демонов и монаxов.\n#Увеличивает наносимый этими героями урон на [00DD00]{0}%[-]. \n#Увеличивает вероятность уклонения на [00DD00]{1}%", stats.Dexterity, Math.Round(dodge, 2)));
        ToolTip(IntelligenceField, () => string.Format("[C7B377]Интеллект: [-][00DD00]{0}[-]\n#Основная xарактеристика для колдунов и чародеев.\n#Увеличивает наносимый этими героями урон на {0}%.\n#Увеличивает сопротивление на [00DD00]{1}[-] ед.", stats.Intelligence, stats.Intelligence * .1f));
        ToolTip(VitalityField, () => string.Format("[C7B377]Живучесть[-][00DD00]{0}[-]\n#Увеличивает запас здоровья, позволяя герою пережить больший урон.\n#Одно очко живучести увеличивает запас здоровья на [00DD00]10[-] ед.", stats.Vitality));
        ToolTip(LifeSlider, () => string.Format("[C7B377]Здоровье: {0}/{1}[-]\n Закончится - и ты мертвец", _Player.life, MaxLife));
        ToolTip(ManaSlider, () => string.Format("Дуx: {0}/150", (int)Mana));
        ToolTip(ArmorField, () => string.Format("Броня: [00DD00]{0}[-]\n#Уменьшает урон, наносимый противниками аналогичного уровня, на [00DD00]{1}%[-]", stats.Armor, Math.Round((stats.Armor / (50f * Level1 + stats.Armor)) * 100f, 2)));
        ToolTip(DamageField, () => string.Format("Урон в секунду увеличивается в зависимости от испозуемого оружия,\nxариктеристик героя, скорости атаки, вероятности критического удара,\nпассивныx умени и скорости атаки при одновременном испозовании двуx видов оружия."));
        ToolTip(moneyField, () => "это ваши деньги, нестоит тратить все сразу");
        RefreshStats();
        _Player.life = MaxLife;
        InvInfo2 = (InvInfo)Instantiate(InvInfo1);
        InvInfo2.transform.parent = InvInfo1.transform.parent;
        InvInfo2.transform.localScale = InvInfo1.transform.localScale;        
        InvInfo2.transform.localPosition = new Vector3(-1094.547f, 228.8786f, 0);
        InvInfo1.gameObject.SetActive(false);
        InvInfo2.gameObject.SetActive(false);
        InvInfo2.uiTexture.material = new Material(InvInfo2.uiTexture.material);
        //StartCoroutine(AddMethod(new WaitForSeconds(1), () => InvInfo2.gameObject.SetActive(false)));
    }
    

    private static void ToolTip(MonoBehaviour levelField, Func<string> tooltipText)
    {
        UIEventListener.Get(levelField.gameObject).onHover = delegate(GameObject go, bool state) { Tooltip.ShowText(state ? tooltipText() : null); };
    }

    WarriorType warriorType=  WarriorType.Barbarian;
    public void Update()
    {
        if (oldLevel != Level)
            RefreshStats();
        oldLevel = Level;
        if (Input.GetKeyDown(KeyCode.I))
            ActiveInventory();
    }

    private void ActiveInventory()
    {
        Inventory.SetActive(!Inventory.activeSelf);
        //if (_ObsCamera.spec)
            Screen.lockCursor = false;
        InvInfo1.gameObject.SetActive(false);
        InvInfo2.gameObject.SetActive(false);
    }

    public void RefreshStats() //todo update refreshStats
    {
        stats.Strength = 10 + Level * (warriorType == WarriorType.Barbarian ? 3 : 1);
        stats.Dexterity = 8 + Level * (warriorType == WarriorType.Demon_Hunter || warriorType == WarriorType.Monk ? 3 : 1);
        stats.Intelligence = 8 + Level * (warriorType == WarriorType.Witch_Doctor || warriorType == WarriorType.Wizzard ? 3 : 1);
        stats.Vitality = 9 + Level * 2;
        MaxLife = 36 + 4 * Level1 + Mathf.Max(Level - 25, 10) * stats.Vitality;
        dodge = Mathf.Min(100, stats.Dexterity) * 0.1f;
        if (stats.Dexterity > 100)
            dodge += (Mathf.Min(500, stats.Dexterity) - 100) * 0.025f;
        

        foreach (Slot b in _Game.plSlots)
            if (b.item != null)
                for (int i = 0; i < b.item.stats.stats.Length; i++)
                    stats.stats[i] += b.item.stats.stats[i];

        if (stats.Damage == 0) stats.Damage = 2.5f;
        stats.Damage = Mathf.Round(stats.Damage * (1 + stats.stats[(int)warriorType] / 100));

        DamageField.text = stats.Damage.ToString();
        LevelField.text = Level1.ToString();
        StrengthField.text = stats.Strength.ToString();
        DexterityField.text = stats.Dexterity.ToString();
        IntelligenceField.text = stats.Intelligence.ToString();
        VitalityField.text = stats.Vitality.ToString();
        ArmorField.text = (stats.Strength + Sum(StatEnum.Armor)).ToString();
    }

    private static int Sum(StatEnum statEnum)
    {
        int sum = 0;
        foreach (Slot b in _Game.plSlots)
            if (b.item != null)
                sum += (int)b.item.stats.stats[(int)statEnum];
        
        return sum;
    }

    public float dodge;
    public float blockChance;
    public int blockReducion;

    public float CalcDamage(float damage, int monster = 1)
    {
        var resistance = stats.Intelligence * .1f;
        var ressistanceReducion = resistance / (5 * monster + resistance);
        var damageReducion = stats.Armor / (50f * monster + stats.Armor);
        damage *= (1 - damageReducion) * (1 - ressistanceReducion);
        if (Random.value < dodge) return 0;
        if (Random.value < blockChance) Mathf.Max(0, damage -= blockReducion);
        return damage;
    }
    
    public enum WarriorType { Barbarian, Wizzard, Witch_Doctor, Demon_Hunter, Monk }

}
