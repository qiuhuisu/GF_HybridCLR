using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework;
using UnityGameFramework.Runtime;
public class EntityBase : EntityLogic
{
    public int Id { get; private set; }
    public EntityParams Params { get; private set; }
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        if (userData != null) Params = userData as EntityParams;
        Id = this.Entity.Id;
    }

    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        Id = this.Entity.Id;
        if (userData != null)
        {
            Params = userData as EntityParams;
            if (Params.position != null)
            {
                this.transform.position = (Vector3)Params.position;
            }
            if (Params.eulerAngles != null)
            {
                this.transform.eulerAngles = (Vector3)Params.eulerAngles;
            }
            if (Params.localScale != null)
            {
                this.transform.localScale = (Vector3)Params.localScale;
            }
            if (!string.IsNullOrEmpty(Params.layer))
            {
                var layerId = LayerMask.NameToLayer(Params.layer);
                gameObject.SetLayerRecursively(layerId);
            }
        }
    }
    protected override void OnHide(bool isShutdown, object userData)
    {
        if (!isShutdown && Params != null)
        {
            ReferencePool.Release(Params);
        }
        base.OnHide(isShutdown, userData);
    }
}
