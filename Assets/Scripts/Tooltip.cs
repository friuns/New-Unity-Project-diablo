using UnityEngine;
using System.Collections.Generic;
public class Tooltip : MonoBehaviour
{
    static Tooltip mInstance;
    public Camera uiCamera;
    public UILabel text;
    public UISlicedSprite background;
    public float appearSpeed = 10f;
    public bool scalingTransitions = true;
    Transform mTrans;
    float mTarget = 0f;
    float mCurrent = 0f;
    Vector3 mPos;
    Vector3 mSize;
    UIWidget[] mWidgets;
    void OnDestroy() { mInstance = null; }
    //private UIPanel panel;
    
    void Awake()
    {
        mInstance = this;
        mTrans = transform;
        mWidgets = GetComponentsInChildren<UIWidget>();
        mPos = mTrans.localPosition;
        mSize = mTrans.localScale;
        if (uiCamera == null) uiCamera = NGUITools.FindCameraForLayer(gameObject.layer);
        //panel = GetComponent<UIPanel>();
        SetAlpha(0f);
        gameObject.SetActive(false);
    }
    
    
    void Update()
    {
        if (mCurrent != mTarget)
        {
            mCurrent = Mathf.Lerp(mCurrent, mTarget, Time.deltaTime * appearSpeed);
            if (Mathf.Abs(mCurrent - mTarget) < 0.001f) mCurrent = mTarget;
            SetAlpha(mCurrent * mCurrent);
            if (scalingTransitions)
            {
                Vector3 offset = mSize * 0.25f;
                offset.y = -offset.y;
                Vector3 size = Vector3.one * (1.5f - mCurrent * 0.5f);
                Vector3 pos = Vector3.Lerp(mPos - offset, mPos, mCurrent);
                mTrans.localPosition = pos;
                mTrans.localScale = size;
            }
        }
    }
    void SetAlpha(float val)
    {
        for (int i = 0, imax = mWidgets.Length; i < imax; ++i)
        {
            UIWidget w = mWidgets[i];
            Color c = w.color;
            c.a = val;
            w.color = c;
        }
        if (val == 0)
            gameObject.SetActive(false);

    }
    void SetText(string tooltipText)
    {
        if (!string.IsNullOrEmpty(tooltipText))
        {
            gameObject.SetActive(true);
            mTarget = 1f;
            if (text != null) text.text = tooltipText;
            mPos = Input.mousePosition;
            if (background != null)
            {
                Transform backgroundTrans = background.transform;
                Transform textTrans = text.transform;
                Vector3 offset = textTrans.localPosition;
                Vector3 textScale = textTrans.localScale;
                mSize = text.relativeSize;
                mSize.x *= textScale.x;
                mSize.y *= textScale.y;
                mSize.x += background.border.x + background.border.z + (offset.x - background.border.x) * 2f;
                mSize.y += background.border.y + background.border.w + (-offset.y - background.border.y) * 2f;
                mSize.z = 1f;
                backgroundTrans.localScale = mSize;
            }
            if (uiCamera != null)
            {
                mPos.x = Mathf.Clamp01(mPos.x / Screen.width);
                mPos.y = Mathf.Clamp01(mPos.y / Screen.height);
                float activeSize = uiCamera.orthographicSize / mTrans.parent.lossyScale.y;
                float ratio = (Screen.height * 0.5f) / activeSize;
                Vector2 max = new Vector2(ratio * mSize.x / Screen.width, ratio * mSize.y / Screen.height);
                mPos.x = Mathf.Min(mPos.x, 1f - max.x);
                mPos.y = Mathf.Max(mPos.y, max.y);
                mTrans.position = uiCamera.ViewportToWorldPoint(mPos);
                mPos = mTrans.localPosition;
                mPos.x = Mathf.Round(mPos.x);
                mPos.y = Mathf.Round(mPos.y);
                mTrans.localPosition = mPos;
            }
            else
            {
                if (mPos.x + mSize.x > Screen.width) mPos.x = Screen.width - mSize.x;
                if (mPos.y - mSize.y < 0f) mPos.y = mSize.y;
                mPos.x -= Screen.width * 0.5f;
                mPos.y -= Screen.height * 0.5f;
            }
        }
        else mTarget = 0f;
    }
    static public void ShowText(string tooltipText)
    {
        if (mInstance != null)
        {
            mInstance.SetText(tooltipText);
        }
    }
}