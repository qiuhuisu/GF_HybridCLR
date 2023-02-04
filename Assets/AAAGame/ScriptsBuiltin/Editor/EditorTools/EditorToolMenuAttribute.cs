using System;
[AttributeUsage(AttributeTargets.Class)]
public class EditorToolMenuAttribute : Attribute
{
    public string ToolMenuPath { get; private set; }
    public int MenuOrder { get; private set; }
    public EditorToolMenuAttribute(string menu, int menuOrder = 0)
    {
        this.ToolMenuPath = menu;
        MenuOrder = menuOrder;
    }
}
