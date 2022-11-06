using GameFramework;
using GameFramework.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using DG.Tweening;

public class PlayerEntity : SampleEntity
{
    public virtual bool IsAIPlayer { get => false; }
    private Vector3 joystickForward;
    private float moveSpeed = 10f;
    private float rotationSpeed = 10f;
    CharacterController characterCtrl;
    private Vector3 playerVelocity;
    private float jumpHeight = 3f;
    private bool isGrounded;
    private Vector3 moveStep;

    private bool mCtrlable;
    public bool Ctrlable
    {
        get => mCtrlable;
        set
        {
            mCtrlable = value;
            if (!IsAIPlayer) GF.StaticUI.JoystickEnable = mCtrlable;
        }
    }
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        characterCtrl = GetComponent<CharacterController>();
    }
    protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        if (!Ctrlable) return;
        isGrounded = characterCtrl.isGrounded;

        Move();//ÒÆ¶¯

        Jump();//ÌøÔ¾
    }
    private void Move()
    {
        float movePower = GF.StaticUI.Joystick.GetDistance();
        joystickForward.Set(GF.StaticUI.Joystick.GetHorizontalAxis(), 0, GF.StaticUI.Joystick.GetVerticalAxis());
        if (movePower > 0.001f)
        {
            characterCtrl.transform.forward = Vector3.Slerp(characterCtrl.transform.forward, joystickForward, Time.deltaTime * rotationSpeed);
        }

        if (isGrounded)
        {
            if (playerVelocity.y < 0) playerVelocity.y = 0;
            moveStep = characterCtrl.transform.forward * moveSpeed * movePower;
        }

        characterCtrl.Move(moveStep * Time.deltaTime);
    }

    private void Jump()
    {
        if (isGrounded && (Input.GetMouseButtonDown(0) && !GF.UI.IsPointerOverUIObject(Input.mousePosition) || Input.GetButtonDown("Jump")))
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3f * Physics.gravity.y);
        }
        playerVelocity.y += Physics.gravity.y * Time.deltaTime;
        characterCtrl.Move(playerVelocity * Time.deltaTime);
    }
}
