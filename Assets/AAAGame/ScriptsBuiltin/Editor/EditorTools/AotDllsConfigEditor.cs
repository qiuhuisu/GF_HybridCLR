using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[EditorToolMenu("»»∏¸/AOT∑∫–Õ≤π≥‰≈‰÷√", 3)]
public class AotDllsConfigEditor : StripLinkConfigEditor
{
    public override string ToolName => "AOT∑∫–Õ≤π≥‰≈‰÷√";
    protected override void InitEditorMode()
    {
        this.SetEditorMode(ConfigEditorMode.AotDllConfig);
    }
}
