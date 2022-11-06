using System;
using System.Collections.Generic;
using GameFramework;
using GameFramework.DataNode;
using UnityGameFramework.Runtime;
/// <summary>
/// �������Ͳ���, ����Entity/UI���ݲ���,���new
/// </summary>
public class RefParams : IReference
{
    static int _instanceId = 0;
    protected IDataNode RootNode { get; private set; }
    protected string RootNodeName { get; private set; }
    public static RefParams Acquire()
    {
        var parms = ReferencePool.Acquire<RefParams>();
        parms.CreateRoot();
        return parms;
    }
    /// <summary>
    /// �������ݸ��ڵ�
    /// </summary>
    protected void CreateRoot()
    {
        RootNode = GF.DataNode.GetOrAddNode(Utility.Text.Format("PDN_{0}", ++_instanceId));
        RootNodeName = RootNode.FullName;
    }

    public void Set<T>(string key, T value) where T : Variable
    {
        var node = RootNode.GetOrAddChild(key);
        node.SetData(value);
    }
    public void Set(string key, object value)
    {
        var varObj = ReferencePool.Acquire<VarObject>();
        varObj.Value = value;
        Set<VarObject>(key, varObj);
    }
    /// <summary>
    /// ��ȡ�������͵Ĳ���
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T>(string key, T defaultValue = null) where T : Variable
    {
        var node = RootNode.GetChild(key);
        if (node == null) return defaultValue;

        return node.GetData<T>();
    }

    /// <summary>
    /// �Ƿ���ڲ���
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Has(string key)
    {
        return RootNode.HasChild(key);
    }
    public void Clear()
    {
        RootNode.Clear();
        GF.DataNode.RemoveNode(RootNode.Name);
    }
}
