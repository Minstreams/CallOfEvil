using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 支持双击/长按的按钮类
/// 抄代码就完事了
/// </summary>
public class UIButton : Button
{
    protected UIButton()
    {
        m_onDoubleClick = new ButtonClickedEvent();
        m_onLongPress = new ButtonClickedEvent();
    }

    private ButtonClickedEvent m_onLongPress;
    public ButtonClickedEvent OnLongPress
    {
        get { return m_onLongPress; }
        set { m_onLongPress = value; }
    }

    private ButtonClickedEvent m_onDoubleClick;
    public ButtonClickedEvent OnDoubleClick
    {
        get { return m_onDoubleClick; }
        set { m_onDoubleClick = value; }
    }

    private bool m_isStartPress = false;

    private float m_curPointDownTime = 0f;

    private float m_longPressTime = 1f;

    private bool m_longPressTrigger = false;

    void Update()
    {
        if (m_isStartPress && !m_longPressTrigger)
        {
            if (Time.time > m_curPointDownTime + m_longPressTime)
            {
                m_longPressTrigger = true;
                m_isStartPress = false;
                if (m_onLongPress != null)
                {
                    m_onLongPress.Invoke();
                }
            }
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        m_curPointDownTime = Time.time;
        m_isStartPress = true;
        m_longPressTrigger = false;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        m_isStartPress = false;
        m_longPressTrigger = false;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        m_isStartPress = false;
        m_longPressTrigger = false;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        //base.OnPointerClick(eventData);
        if (!m_longPressTrigger)
        {
            if (eventData.clickCount == 1)
            {
                onClick.Invoke();
            }
            else if (eventData.clickCount == 2)
            {
                if (m_onDoubleClick != null)
                {
                    m_onDoubleClick.Invoke();
                }
            }
        }
    }
}