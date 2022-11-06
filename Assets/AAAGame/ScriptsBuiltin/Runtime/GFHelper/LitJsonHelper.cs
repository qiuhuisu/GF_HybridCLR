using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework;
using System;
public class LitJsonHelper : Utility.Json.IJsonHelper
{
    public string ToJson(object obj)
    {
        return LitJson.JsonMapper.ToJson(obj);
    }

    public T ToObject<T>(string json)
    {
        return LitJson.JsonMapper.ToObject<T>(json);
    }

    public object ToObject(Type objectType, string json)
    {
        throw new NotSupportedException("ToObject(Type objectType, string json)");
    }
}
