using GameFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataModelBase : IReference
{
    public int Id { get; private set; } = 0;

    public void Init(int id) { this.Id = id; }
    public void Clear()
    {
        this.Id = 0;
    }

    internal void Shutdown()
    {
        ReferencePool.Release(this);
    }
}
