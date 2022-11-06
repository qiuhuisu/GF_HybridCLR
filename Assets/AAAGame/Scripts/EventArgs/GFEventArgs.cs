using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework;
using GameFramework.Event;
using UnityGameFramework.Runtime;

public enum GFEventType
{
    ResourceInitialized //��Ϸ��Դ��ʼ����ɺ�
}
public class GFEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(GFEventArgs).GetHashCode();
    public override int Id => EventId;
    public GFEventType EventType { get; private set; }
    public object UserData { get; private set; }
    public override void Clear()
    {
        UserData = null;
    }
    public GFEventArgs Fill(GFEventType eventType, object userDt = null)
    {
        this.EventType = eventType;
        this.UserData = userDt;
        return this;
    }
}
