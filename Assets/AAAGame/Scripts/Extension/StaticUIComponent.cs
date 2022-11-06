using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework;
using UnityGameFramework.Runtime;
using UnityEngine.UI;

public class StaticUIComponent : GameFrameworkComponent
{
    [SerializeField] UltimateJoystick mJoystick;
    public bool JoystickEnable
    {
        get { return mJoystick.gameObject.activeSelf; }
        set
        {
            if (value)
            {
                mJoystick.EnableJoystick();
                StartCoroutine(RefreshJoystickPosition());
            }
            else
            {
                mJoystick.DisableJoystick();
            }
            mJoystick.GetComponent<CanvasGroup>().alpha = value ? 1 : 0;
        }
    }
    public UltimateJoystick Joystick { get { return mJoystick; } }


    private void Start()
    {
        mJoystick.GetComponentInParent<Canvas>().worldCamera = GFBuiltin.UICamera;
        mJoystick.GetComponent<CanvasGroup>().alpha = 0;
        UpdateCanvasScaler();
    }
    public void UpdateCanvasScaler()
    {
        var joystickScaler = mJoystick.GetComponentInParent<CanvasScaler>();
        joystickScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        joystickScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        joystickScaler.matchWidthOrHeight = GFBuiltin.CanvasFitMode == ScreenFitMode.Width ? 0 : 1;

    }
    IEnumerator RefreshJoystickPosition() { yield return new WaitForEndOfFrame(); mJoystick.UpdatePositioning(); }
}
