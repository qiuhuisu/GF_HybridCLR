using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class EditorToolBase : EditorWindow
{
    public abstract string ToolName { get; }
    private void Awake()
    {
        this.titleContent = new GUIContent(ToolName);
    }
}
