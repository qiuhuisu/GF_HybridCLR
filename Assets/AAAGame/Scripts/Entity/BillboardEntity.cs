using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

public enum PivotAxis
{
    // Rotate about all axes.
    Free,
    // Rotate about an individual axis.
    X,
    Y
}
public class BillboardEntity : SampleEntity
{
    PivotAxis PivotAxis = PivotAxis.Free;

    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        if (Params.Has("Axis"))
        {
            PivotAxis = (PivotAxis)Params.Get<VarInt32>("Axis").Value;
        }
        else
        {
            PivotAxis = PivotAxis.Free;
        }
    }
    protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        Vector3 forward;
        Vector3 up;

        switch (PivotAxis)
        {
            case PivotAxis.X:
                Vector3 right = transform.right;
                forward = Vector3.ProjectOnPlane(CameraFollower.Instance.transform.forward, right).normalized;
                up = Vector3.Cross(forward, right);
                break;

            case PivotAxis.Y:
                up = transform.up;
                forward = Vector3.ProjectOnPlane(CameraFollower.Instance.transform.forward, up).normalized;
                break;
            case PivotAxis.Free:
            default:
                forward = CameraFollower.Instance.transform.forward;
                up = CameraFollower.Instance.transform.up;
                break;
        }
        transform.rotation = Quaternion.LookRotation(forward, up);
    }
}
