using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[EditorToolMenu("�ȸ�/AOT���Ͳ�������", 3)]
public class AotDllsConfigEditor : StripLinkConfigEditor
{
    public override string ToolName => "AOT���Ͳ�������";
    protected override void InitEditorMode()
    {
        this.SetEditorMode(ConfigEditorMode.AotDllConfig);
    }
}
