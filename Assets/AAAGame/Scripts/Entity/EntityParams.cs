#pragma warning disable IDE1006 // 命名样式
using GameFramework;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityGameFramework.Runtime;

public class EntityParams : RefParams
{
    const string KeyLocalPosition = "localPosition";
    const string KeyPosition = "position";
    const string KeyLocalEulerAngles = "localEulerAngles";
    const string KeyEulerAngles = "eulerAngles";
    const string KeyLocalScale = "localScale";
    const string KeyLayer = "layer";
    public static EntityParams Acquire(Vector3? position = null, Vector3? eulerAngles = null, Vector3? localScale = null)
    {
        var eParams = ReferencePool.Acquire<EntityParams>();
        eParams.CreateRoot();
        if (position != null) eParams.position = (Vector3)position;
        if (eulerAngles != null) eParams.eulerAngles = (Vector3)eulerAngles;
        if (localScale != null) eParams.localScale = (Vector3)localScale;
        return eParams;
    }
    public Vector3? position
    {
        get
        {
            if (Has(KeyPosition))
                return Get<VarVector3>(KeyPosition, Vector3.zero);
            else
                return null;
        }
        set
        {
            Set<VarVector3>(KeyPosition, value);
        }
    }
    public Vector3? localPosition
    {
        get
        {
            if (Has(KeyLocalPosition))
                return Get<VarVector3>(KeyLocalPosition, Vector3.zero);
            else
                return null;
        }
        set
        {
            Set<VarVector3>(KeyLocalPosition, value);
        }
    }
    public Vector3? localEulerAngles
    {
        get
        {
            if (Has(KeyLocalEulerAngles))
                return Get<VarVector3>(KeyLocalEulerAngles, Vector3.zero);
            else
                return null;
        }
        set
        {
            Set<VarVector3>(KeyLocalEulerAngles, value);
        }
    }
    public Vector3? eulerAngles
    {
        get
        {
            if (Has(KeyEulerAngles))
                return Get<VarVector3>(KeyEulerAngles, Vector3.zero);
            else
                return null;
        }
        set
        {
            Set<VarVector3>(KeyEulerAngles, value);
        }
    }

    public Vector3? localScale
    {
        get
        {
            if (Has(KeyLocalScale))
                return Get<VarVector3>(KeyLocalScale, Vector3.zero);
            else
                return null;
        }
        set
        {
            Set<VarVector3>(KeyLocalScale, value);
        }
    }
    public string layer
    {
        get
        {
            if (Has(KeyLayer))
                return Get<VarString>(KeyLayer, "Default");
            else
                return null;
        }
        set
        {
            Set<VarString>(KeyLayer, value);
        }
    }
}
#pragma warning restore IDE1006 // 命名样式
