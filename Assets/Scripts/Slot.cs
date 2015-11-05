using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class Slot : Bs
{
    internal Item item;
    public UITexture sprite;
    public UISprite background;
    public SlotType slotType;
    public bool dragging;
    public void Start()
    {
        if(slotType!= SlotType.Inventory)
            _Game.plSlots.Add(this);
    }
    private void OnPress(bool isPressed)
    {
        if (isPressed && item != null)
        {
            _Cursor.slot = this;
            dragging = true;
        } 
    }
    void OnHover(bool show)
    {
        if (item != null)
        {
            Item bitem = null;
            if (!OnPlayer)
            {
                var b = FirstOrDefault(_Game.plSlots, a => a.slotType == item.slotType);
                if (b != null && b.item != null)
                {
                    bitem = b.item;
                    SetInv(show, _Hud.InvInfo2, b.item);
                }
            }
            _Hud.InvInfo1.header.SetActive(false);
            SetInv(show, _Hud.InvInfo1, item, bitem);

        }
        //_Tooltip.ShowText(show?"asddsa":null);
    }

    private void SetInv(bool show, InvInfo inv, Item it, Item compare = null)
    {
        inv.gameObject.SetActive(show);
        if (show)
        {
            if (it.slotType == SlotType.Hand)
            {
                inv.damage.text = it.DamagePerSecond.ToString();                
                inv.info.text = string.Format("[6D6D6D]ед. урона в секунду[-]\n\n{0} урон\n{1} Атак в секунду", it.Damage, it.attacksPerSecond);
                inv.durability.text = string.Format("[AF8346]прочность[-]:{0}/{0}", it.durability);
                inv.handType.text = string.Format("[6D6D6D]{0}[-]\n[AF8346]", it.TwoHanded ? "Двуxручное Оружее" : "Одноручное оружее");
            }
            else
            {
                inv.damage.text = it.Armor.ToString();                
                inv.sellValue.text = string.Format("цена продажи:[-]{0}$", it.sellValue);
                inv.handType.text = TR(it.slotType.ToString());
                inv.info.text = "[6D6D6D]Броня[-]";
            }
            inv.compare.text = "";
            inv.Background.color = it.color;
            inv.type.text = TR(it.quality.ToString()) + " " + TR(it.weaponSubName.ToString());
            inv.uiTexture.mainTexture = it.Texture;
            inv.Title.text = it.Name.ToUpper();
            inv.type.color = it.color;

            inv.level.text = "Требуемый уровень: " + it.itemLevel;

            if (compare != null)
                Compare(inv, it, compare);

            var stats = it.stats;
            inv.magicSettings.text = "";
            for (int i = 0; i < stats.stats.Length; i++)
            {
                if (stats.stats[i] > 0 && !stats.noRand[i])
                    inv.magicSettings.text += "#[5454CC]" + string.Format(stats.texts[i], (int)stats.stats[i]) + "[-]\r\n";                
            }
        }
    }

    private static void Compare(InvInfo inv, Item it, Item compare)
    {
        var a = compare.stats;
        var b = it.stats;
        inv.compare.text = "Наденув вы получете:\r\n";
        for (int i = 0; i < a.stats.Length; i++)
        {
            var dif = b.stats[i] - a.stats[i];
            if (dif != 0 )
                inv.compare.text += (dif > 0 ? "#[00DD00]" : "#[DD0000]") + string.Format(a.texts[i], dif.ToString("+#;-#;0")) + "[-]\r\n";
        }
    }

    public bool OnPlayer = true;
    void Update()
    {
        if (item != null && item.Texture == null) item = null;
        sprite.enabled = item != null && !dragging;
        if (OnPlayer)
            background.enabled = sprite.enabled;
        if (item != null)
        {
            sprite.mainTexture = item.Texture;
            if (!OnPlayer)
                sprite.transform.localScale = new Vector3(sprite.mainTexture.width/ (float)sprite.mainTexture.height, 1, 0);             
        }
    }

    internal bool isWeapon { get { return slotType == SlotType.Hand; } }

    private Item oldItem;
    public void Refresh()
    {
        if (item != oldItem)
        {
            if (OnPlayer && isWeapon)
                _Player.SelectWeapon(item);    
        }
        oldItem = item;
        if(OnPlayer)
            _Hud.RefreshStats();
    }
}


