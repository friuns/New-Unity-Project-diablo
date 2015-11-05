using UnityEngine;
using System.Collections;

public class Cursor : Bs
{
    
    public UITexture cursorRenderer;
    private Transform cursor;
    internal Camera uiCamera;
    void Start()
    {
        cursor = transform;
        uiCamera = NGUITools.FindCameraForLayer(cursor.gameObject.layer);
        cursorRenderer.depth = 100;
	}
	
	void Update () {
        Vector3 pos = Input.mousePosition;
        pos.x = Mathf.Clamp01(pos.x / Screen.width);
        pos.y = Mathf.Clamp01(pos.y / Screen.height);
        cursor.position = uiCamera.ViewportToWorldPoint(pos) + Vector3.forward * 0.1f;
        cursor.localPosition = NGUIMath.ApplyHalfPixelOffset(cursor.localPosition, cursor.localScale);


        if (slot == null)
            cursorRenderer.enabled = false;
	    else
	    {
            cursorRenderer.enabled = true; 
            cursorRenderer.mainTexture = slot.item.Texture;
            cursorRenderer.pivot = UIWidget.Pivot.Center;
            cursorRenderer.transform.localScale = new Vector3(slot.item.Texture.width, slot.item.Texture.height, 0);            
            //transform.localScale = new Vector3(inner.width, inner.height); 
	    }

        //Rect inner = cursorRenderer.sprite.inner;   
        //transform.localScale = new Vector3(inner.width, inner.height); 
	    //cursorRenderer.sprite = _Loader.atlas.GetSprite(a);
        if (!Input.GetMouseButton(0) && slot != null)
        {
            if (!ongui)
            {
                Vector3 p = _Player.pos;
                RaycastHit h;
                if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out h, 100, 1 << (int) Layer.Level))
                    p = h.point;
                var g = (GameObject)Instantiate(_Database.DropAnimation, p, Quaternion.identity);
                g.GetComponent<DroppedItem>().item = slot.item;
                var g2 = (GameObject)Instantiate(slot.item.weaponPrefab, p + slot.item.weaponPrefab.transform.position, Quaternion.identity);
                g2.AddComponent<BoxCollider>();
                g2.gameObject.layer = (int) Layer.Coin;
                g2.transform.parent = g.transform;
                g2.name = "Rifle_PH";
                slot.item = null;
                slot.dragging = false;
                slot.Refresh(); 
                slot = null;
            }
            else
            {
                
                var sl = UICamera.hoveredObject.GetComponent<Slot>();
                
                if (sl != null && (slot.item.slotType == sl.slotType || sl.slotType == SlotType.Inventory))
                {
                    _Cursor.slot.dragging = false;
                    Switch(ref _Cursor.slot.item, ref sl.item);
                    slot.Refresh();
                    sl.Refresh();
                    slot = null;
                }
                else
                {
                    slot.dragging = false;                    
                    slot = null;
                }
            }
        }

	}
    private void Switch(ref Item a, ref Item b)
    {
        var tmp = a;
        a = b;
        b = tmp;
    }

    internal Slot slot;
    //internal Item_ item;
    //public Texture2D cursorEnum;
    //public Texture2D cursorNormal;
    //public Texture2D cursorAttack;

}
