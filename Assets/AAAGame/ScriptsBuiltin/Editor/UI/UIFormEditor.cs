#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using System.Linq;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using System;
using GameFramework;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Events;

[CustomEditor(typeof(UIFormBase), true)]
public class UIFormBaseEditor : Editor
{
    readonly static string[] varPrefixArr = { "private", "protected", "public" };
    const string arrFlag = "Arr";
    const float fieldItemWidth = 100;
    const float fieldPrefixWidth = 100;
    const float fieldTypeWidth = 240;
    const float smallButtonWidth = 25;
    SerializedProperty mFields;
    static bool addToFieldToggle;
    static bool removeToFieldToggle;
    ReorderableList[] mReorderableList;
    UIFormBase uiForm;
    int mCurFieldIdx;

    bool useSerializeMode = false;
    static int varPrefixIndex = -1;
    static bool mShowSelectTypeMenu;

    GUIContent prefixContent;
    GUIContent typeContent;

    #region #右键菜单
    private static GUIStyle normalStyle;
    private static GUIStyle selectedStyle;
    [InitializeOnLoadMethod]
    static void InitEditor()
    {

        normalStyle = new GUIStyle();
        normalStyle.normal.textColor = Color.white;

        selectedStyle = new GUIStyle();
        selectedStyle.normal.textColor = Color.green;
        Selection.selectionChanged = () =>
        {
            addToFieldToggle = false;
            removeToFieldToggle = false;
        };

        EditorApplication.hierarchyWindowItemOnGUI = delegate (int id, Rect rect)
        {
            OpenSelectComponentMenuListener(rect);
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage == null)
            {
                return;
            }
            var uiForm = prefabStage.prefabContentsRoot.GetComponent<UIFormBase>();
            if (uiForm == null)
            {
                return;
            }
            var curDrawNode = EditorUtility.InstanceIDToObject(id) as GameObject;
            if (curDrawNode == null)
            {
                return;
            }
            var fields = uiForm.GetFieldsProperties();
            SerializeFieldData drawItem = null;
            foreach (var item in fields)
            {
                if (item == null) continue;
                if (ArrayUtility.Contains(item.Targets, curDrawNode))
                {
                    drawItem = item;
                    break;
                }
            }
            if (drawItem != null)
            {
                rect.x = rect.xMax - Mathf.Min(300, rect.size.x * 0.35f);
                GUI.Label(rect, string.Format("{0} {1} {2}", drawItem.VarPrefix, drawItem.VarSampleType, drawItem.VarName), selectedStyle);
            }

        };
    }
    [MenuItem("GameObject/UIForm Fields Tool/Add private", false, priority = 2)]
    private static void AddPrivateVariable2UIForm()
    {
        varPrefixIndex = 0;
        mShowSelectTypeMenu = true;
    }
    [MenuItem("GameObject/UIForm Fields Tool/Add protected", false, priority = 3)]
    private static void AddProtectedVariable2UIForm()
    {
        varPrefixIndex = 1;
        mShowSelectTypeMenu = true;
    }
    [MenuItem("GameObject/UIForm Fields Tool/Add public", false, priority = 4)]
    private static void AddPublicVariable2UIForm()
    {
        varPrefixIndex = 2;
        mShowSelectTypeMenu = true;
    }

    [MenuItem("GameObject/UIForm Fields Tool/Remove", false, priority = 5)]
    private static void RemoveUIFormVariable()
    {
        if (removeToFieldToggle)
        {
            return;
        }
        if (Selection.count <= 0) return;

        var uiForm = GetPrefabRootComponent<UIFormBase>();
        if (uiForm == null)
        {
            Debug.LogWarning("UIForm Script is not exist.");
            return;
        }
        var fieldsProperties = uiForm.GetFieldsProperties();
        if (fieldsProperties == null) return;
        Undo.RecordObject(uiForm, uiForm.name);
        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            var itm = Selection.gameObjects[i];
            if (itm == null) continue;

            for (int j = fieldsProperties.Length - 1; j >= 0; j--)
            {
                var fields = fieldsProperties[j];
                if (fields == null || fields.Targets == null || fields.Targets.Length <= 0) continue;
                for (int k = fields.Targets.Length - 1; k >= 0; k--)
                {
                    if (fields.Targets[k] == itm)
                    {
                        if (fields.Targets.Length <= 1)
                        {
                            ArrayUtility.RemoveAt(ref fieldsProperties, j);
                        }
                        else
                        {
                            ArrayUtility.RemoveAt(ref fields.Targets, k);
                        }
                    }
                }
            }
        }
        uiForm.ModifyFieldsProperties(fieldsProperties);
        EditorUtility.SetDirty(uiForm);
        removeToFieldToggle = true;
        addToFieldToggle = false;
    }

    /// <summary>
    /// 不带名字空间的类型名
    /// </summary>
    /// <returns></returns>
    private static Type GetSampleType(string fullName)
    {
        var result = Utility.Assembly.GetType(fullName);
        return result;
    }
    private static T GetPrefabRootComponent<T>() where T : Component
    {
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage == null)
        {
            Debug.LogWarning("GetCurrentPrefabStage is null.");
            return null;
        }
        return prefabStage.prefabContentsRoot.GetComponent<T>();
    }
    private static void AddToFields(string varPrefix, string varType)
    {
        if (addToFieldToggle)
        {
            return;
        }
        if (Selection.count <= 0) return;
        var uiForm = GetPrefabRootComponent<UIFormBase>();
        if (uiForm == null)
        {
            Debug.LogWarning("UIForm Script is not exist.");
            return;
        }
        var targets = GetTargetsFromSelectedNodes(Selection.gameObjects);

        var fieldsProperties = uiForm.GetFieldsProperties();
        if (fieldsProperties == null) fieldsProperties = new SerializeFieldData[0];
        Undo.RecordObject(uiForm, uiForm.name);
        SerializeFieldData field = new SerializeFieldData(GenerateFieldName(fieldsProperties, targets), targets);
        field.VarType = varType;
        field.VarSampleType = GetSampleType(field.VarType).Name;
        field.VarPrefix = varPrefix;

        ArrayUtility.Add(ref fieldsProperties, field);
        uiForm.ModifyFieldsProperties(fieldsProperties);
        EditorUtility.SetDirty(uiForm);
        addToFieldToggle = true;
        removeToFieldToggle = false;
    }
    private static GameObject[] GetTargetsFromSelectedNodes(GameObject[] selectedList)
    {
        GameObject[] targets = new GameObject[selectedList.Length];
        for (int i = 0; i < selectedList.Length; i++)
        {
            targets[i] = selectedList[i];
        }
        targets = targets.OrderBy(go => go.transform.GetSiblingIndex()).ToArray();
        return targets;
    }
    #endregion

    private void OnEnable()
    {
        prefixContent = new GUIContent();
        typeContent = new GUIContent();
        useSerializeMode = EditorPrefs.GetBool("SerializeMode", true);
        varPrefixIndex = 0;
        mShowSelectTypeMenu = false;
        uiForm = (target as UIFormBase);
        if (uiForm.GetFieldsProperties() == null)
        {
            uiForm.ModifyFieldsProperties(new SerializeFieldData[0]);
        }
        mFields = serializedObject.FindProperty("_fields");
        mReorderableList = new ReorderableList[mFields.arraySize];
    }


    private static void OpenSelectComponentMenuListener(Rect rect)
    {
        if (mShowSelectTypeMenu)
        {
            int idx = -1;
            var strArr = GetPopupContents(GetTargetsFromSelectedNodes(Selection.gameObjects));
            var contents = new GUIContent[strArr.Length];
            for (int i = 0; i < strArr.Length; i++)
            {
                contents[i] = new GUIContent(strArr[i]);
            }
            rect.width = 200;
            rect.height = MathF.Max(100, contents.Length * rect.height);
            EditorUtility.DisplayCustomMenu(rect, contents, idx, (userData, contents, selected) =>
            {
                AddToFields(varPrefixArr[varPrefixIndex], contents[selected]);
            }, null);
            mShowSelectTypeMenu = false;
        }
    }

    public override void OnInspectorGUI()
    {
        CheckAndInitFields();
        serializedObject.Update();
        EditorGUILayout.BeginVertical();

        bool disableAct = EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlaying;
        if (disableAct)
        {
            EditorGUILayout.HelpBox("Wiatting for compiling or updating...", MessageType.Warning);
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(disableAct);

        if (GUILayout.Button("1. Generate Script")) //生成脚本
        {
            GenerateUIFormVariables(uiForm, serializedObject);
        }

        if (useSerializeMode && GUILayout.Button("2. Bind Properties")) //绑定变量
        {
            SerializeFieldProperties(serializedObject, uiForm.GetFieldsProperties());
        }
        if (GUILayout.Button("Open Script"))
        {
            var monoScript = MonoScript.FromMonoBehaviour(uiForm);
            string scriptFile = AssetDatabase.GetAssetPath(monoScript);
            InternalEditorUtility.OpenFileAtLineExternal(scriptFile, 0);
        }
        if (GUILayout.Button("Open Bind Script"))
        {
            var uiFormClassName = uiForm.GetType().Name;
            string scriptFile = UtilityBuiltin.ResPath.GetCombinePath(ConstEditor.UISerializeFieldDir, Utility.Text.Format("{0}.Variables.cs", uiFormClassName));
            InternalEditorUtility.OpenFileAtLineExternal(scriptFile, 0);
        }
        if (GUILayout.Button("Clear", GUILayout.MaxWidth(55)))
        {
            mFields.ClearArray();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();

        useSerializeMode = EditorGUILayout.ToggleLeft("SerializeMode", useSerializeMode, GUILayout.MaxWidth(115));
        if (EditorGUI.EndChangeCheck())
        {
            UnityEditor.EditorPrefs.SetBool("SerializeMode", useSerializeMode);
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(EditorGUIUtility.TrIconContent("_Help", "使用说明"), GUIStyle.none))
        {
            EditorUtility.DisplayDialog("使用说明", "1.打开UI界面预制体.\n2.右键节点'[Add/Remove] UI Variable'添加/移除变量.\n3.在Inspector面板点击Generate Script生成代码.", "OK");
            GUIUtility.ExitGUI();
        }
        EditorGUILayout.EndHorizontal();
        for (int i = 0; i < mFields.arraySize; i++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(i.ToString(), GUILayout.Width(30f));
            var item = mFields.GetArrayElementAtIndex(i);
            var varNameProperty = item.FindPropertyRelative("VarName");
            var varTypeProperty = item.FindPropertyRelative("VarType");
            var targetsProperty = item.FindPropertyRelative("Targets");
            var unfoldProperty = item.FindPropertyRelative("Foldout");
            var varPrefixProperty = item.FindPropertyRelative("VarPrefix");
            var varSampleType = item.FindPropertyRelative("VarSampleType");

            int targetsCount = targetsProperty != null ? targetsProperty.arraySize : 0;
            string targetsTitle = $"Size {targetsCount}";

            unfoldProperty.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(unfoldProperty.boolValue, targetsTitle);
            prefixContent.text = varPrefixProperty.stringValue;
            if (EditorGUILayout.DropdownButton(prefixContent, FocusType.Passive, GUILayout.MaxWidth(fieldPrefixWidth)))
            {
                GenericMenu popMenu = new GenericMenu();
                foreach (var varPrefix in varPrefixArr)
                {
                    popMenu.AddItem(new GUIContent(varPrefix), varPrefix.CompareTo(varPrefixProperty.stringValue) == 0, selectObj =>
                    {
                        varPrefixProperty.stringValue = selectObj.ToString();
                        serializedObject.ApplyModifiedProperties();
                    }, varPrefix);
                }
                popMenu.ShowAsContext();
            }
            typeContent.text = varTypeProperty.stringValue;
            if (EditorGUILayout.DropdownButton(typeContent, FocusType.Passive, GUILayout.MaxWidth(fieldTypeWidth)))
            {
                GenericMenu popMenu = new GenericMenu();
                var popContens = GetPopupContents(targetsProperty);
                foreach (var tpName in popContens)
                {
                    popMenu.AddItem(new GUIContent(tpName), tpName.CompareTo(varTypeProperty.stringValue) == 0, selectObj =>
                    {
                        varTypeProperty.stringValue = selectObj.ToString();
                        varSampleType.stringValue = GetSampleType(varTypeProperty.stringValue).Name;
                        serializedObject.ApplyModifiedProperties();
                    }, tpName);
                }
                popMenu.ShowAsContext();
            }

            varNameProperty.stringValue = GUILayout.TextField(varNameProperty.stringValue, GUILayout.MinWidth(fieldItemWidth));

            if (GUILayout.Button("+", GUILayout.Width(smallButtonWidth)))
            {
                InsertField(i + 1);
            }
            if (GUILayout.Button("-", GUILayout.Width(smallButtonWidth)))
            {
                RemoveField(i);
            }
            EditorGUILayout.EndHorizontal();
            if (i < mFields.arraySize && mFields.GetArrayElementAtIndex(i).FindPropertyRelative("Foldout").boolValue)
            {
                item = mFields.GetArrayElementAtIndex(i);
                targetsProperty = item.FindPropertyRelative("Targets");

                mCurFieldIdx = i;
                if (mReorderableList[i] == null)
                {
                    mReorderableList[i] = new ReorderableList(serializedObject, targetsProperty, true, false, true, true);
                    mReorderableList[i].drawElementCallback = DrawTargets;
                }
                else
                {
                    mReorderableList[i].serializedProperty = targetsProperty;
                }
                mReorderableList[i].DoLayoutList();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndVertical();
        base.OnInspectorGUI();
    }
    /// <summary>
    /// 生成UI脚本.cs
    /// </summary>
    /// <param name="uiForm"></param>
    /// <param name="uiFormSerializer"></param>
    private void GenerateUIFormVariables(UIFormBase uiForm, SerializedObject uiFormSerializer)
    {
        if (uiForm == null) return;

        var monoScript = MonoScript.FromMonoBehaviour(uiForm);
        var uiFormClassName = monoScript.GetClass().Name;
        string scriptFile = UtilityBuiltin.ResPath.GetCombinePath(ConstEditor.UISerializeFieldDir, Utility.Text.Format("{0}.Variables.cs", uiFormClassName));
        var fields = uiForm.GetFieldsProperties();
        if (fields == null || fields.Length <= 0)
        {
            if (File.Exists(scriptFile))
            {
                File.Delete(scriptFile);
            }
            var metaFile = scriptFile + ".meta";
            if (File.Exists(metaFile))
            {
                File.Delete(metaFile);
            }
            AssetDatabase.Refresh();
            return;
        }

        var matchResult = Regex.Match(monoScript.text, Utility.Text.Format("partial[\\s]+class[\\s]+{0}", uiFormClassName));
        string scriptPath = AssetDatabase.GetAssetPath(monoScript);
        if (!matchResult.Success)
        {
            EditorUtility.DisplayDialog("生成UI变量失败!", Utility.Text.Format("请先手动为{0}类添加'partial'修饰符!\n{1}", uiFormClassName, scriptPath), "OK");
            return;
        }
        List<string> nameSpaceList = new List<string> { "UnityEngine" };//默认自带的名字空间
        List<string> fieldList = new List<string>();
        foreach (var field in fields)
        {
            if (string.IsNullOrWhiteSpace(field.VarType) || string.IsNullOrWhiteSpace(field.VarName))
            {
                continue;
            }
            var varType = GetSampleType(field.VarType);
            if (varType == null)
            {
                continue;
            }
            if (!string.IsNullOrEmpty(varType.Namespace) && !nameSpaceList.Contains(varType.Namespace))
            {
                nameSpaceList.Add(varType.Namespace);
            }
            bool isArray = field.Targets.Length > 1;

            var varPrefix = field.VarPrefix;// varPrefixArr[field.VarPrefixSelectedIdx];
            string serializeFieldPrefix = useSerializeMode ? "[SerializeField] " : "";
            string fieldLine;
            if (isArray)
            {
                fieldLine = Utility.Text.Format("{0}{1} {2}[] {3} = null;", serializeFieldPrefix, varPrefix, varType.Name, field.VarName);
            }
            else
            {
                fieldLine = Utility.Text.Format("{0}{1} {2} {3} = null;", serializeFieldPrefix, varPrefix, varType.Name, field.VarName);
            }
            fieldList.Add(fieldLine);
        }
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("//---------------------------------");
        stringBuilder.AppendLine("//此文件由工具自动生成,请勿手动修改");
        stringBuilder.AppendLine($"//更新自:{CloudProjectSettings.userName}");
        stringBuilder.AppendLine($"//更新时间:{DateTime.Now}");
        stringBuilder.AppendLine("//---------------------------------");
        foreach (var item in nameSpaceList)
        {
            stringBuilder.AppendLine(Utility.Text.Format("using {0};", item));
        }
        string uiFormClassNameSpace = monoScript.GetClass().Namespace;
        bool hasNameSpace = !string.IsNullOrWhiteSpace(uiFormClassNameSpace);
        if (hasNameSpace)
        {
            stringBuilder.AppendLine(Utility.Text.Format("namespace {0}", uiFormClassNameSpace));
            stringBuilder.AppendLine("{");
        }
        stringBuilder.AppendLine(Utility.Text.Format("public partial class {0}", uiFormClassName));
        stringBuilder.AppendLine("{");
        if (useSerializeMode)
        {
            stringBuilder.AppendLine("\t[Space(10)]");
            stringBuilder.AppendLine("\t[Header(\"Auto Genertate Properties:\")]");
        }
        foreach (var item in fieldList)
        {
            stringBuilder.AppendLine("\t" + item);
        }
        //不使用Serialize模式时直接生成获取组件的代码
        if (!useSerializeMode)
        {
            GeneratePropertiesUseGetComponent(stringBuilder, fields);
        }
        stringBuilder.AppendLine("}");
        if (hasNameSpace) stringBuilder.AppendLine("}");

        File.WriteAllText(scriptFile, stringBuilder.ToString());
        AssetDatabase.Refresh();
        //SerializeFieldProperties(uiFormSerializer, fields);
    }
    private void GeneratePropertiesUseGetComponent(StringBuilder stringBuilder, SerializeFieldData[] fields)
    {
        stringBuilder.AppendLine("\tprotected override void InitUIProperties()");
        stringBuilder.AppendLine("\t{");
        stringBuilder.AppendLine("\t\tvar fields = this.GetFieldsProperties();");
        for (int i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            bool isArray = field.Targets.Length > 1;
            bool isGameObject = field.VarType.CompareTo(typeof(GameObject).FullName) == 0;
            if (isArray)
            {
                if (isGameObject)
                    stringBuilder.AppendLine(Utility.Text.Format("\t\t{0} = fields[{1}].Targets;", field.VarName, i));
                else
                    stringBuilder.AppendLine(Utility.Text.Format("\t\t{0} = fields[{1}].GetComponents<{2}>();", field.VarName, i, field.VarSampleType));
            }
            else
            {
                if (isGameObject)
                    stringBuilder.AppendLine(Utility.Text.Format("\t\t{0} = fields[{1}].Targets[0];", field.VarName, i));
                else
                    stringBuilder.AppendLine(Utility.Text.Format("\t\t{0} = fields[{1}].GetComponent<{2}>(0);", field.VarName, i, field.VarSampleType));
            }
        }
        stringBuilder.AppendLine("\t}");
    }
    private void SerializeFieldProperties(SerializedObject serializedObject, SerializeFieldData[] fields)
    {
        if (serializedObject == null)
        {
            Debug.LogError("生成UI SerializedField失败, serializedObject为null");
            return;
        }

        foreach (var item in fields)
        {
            string varName = item.VarName;
            string varType = item.VarType;
            bool isGameObject = varType.CompareTo(typeof(GameObject).FullName) == 0;
            var property = serializedObject.FindProperty(varName);
            if (property == null) continue;
            if (item.Targets.Length <= 1)
            {
                property.objectReferenceValue = isGameObject ? item.Targets[0] : item.Targets[0]?.GetComponent(GetSampleType(varType));
            }
            else if (property.isArray)
            {
                property.ClearArray();
                for (int i = 0; i < item.Targets.Length; i++)
                {
                    if (i >= property.arraySize)
                    {
                        property.InsertArrayElementAtIndex(i);
                    }
                    property.GetArrayElementAtIndex(i).objectReferenceValue = isGameObject ? item.Targets[i] : item.Targets[i]?.GetComponent(GetSampleType(varType));
                }
                //for (int i = property.arraySize - 1; i >= item.Targets.Length; i--)
                //{
                //    property.DeleteArrayElementAtIndex(i);
                //}
            }
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void CheckAndInitFields()
    {
        if (uiForm.GetFieldsProperties() == null)
        {
            uiForm.ModifyFieldsProperties(new SerializeFieldData[0]);
        }
        if (mFields == null) mFields = serializedObject.FindProperty("_fields");

        if (mFields.arraySize != mReorderableList.Length)
        {
            if (mFields.arraySize > mReorderableList.Length)
            {
                for (int i = mReorderableList.Length; i < mFields.arraySize; i++)
                {
                    ArrayUtility.Insert(ref mReorderableList, mReorderableList.Length, null);
                }
            }
            else
            {
                for (int i = mFields.arraySize; i < mReorderableList.Length; i++)
                {
                    ArrayUtility.RemoveAt(ref mReorderableList, mReorderableList.Length - 1);
                }
            }
        }
    }

    private void InsertField(int idx)
    {
        Undo.RecordObject(uiForm, uiForm.name);
        mFields.InsertArrayElementAtIndex(idx);
        ArrayUtility.Insert(ref mReorderableList, idx, null);
        var lastField = mFields.GetArrayElementAtIndex(idx);
        if (lastField != null)
        {
            var lastVarName = lastField.FindPropertyRelative("VarName");
            if (!string.IsNullOrEmpty(lastVarName.stringValue))
            {
                lastVarName.stringValue += idx.ToString();
            }
        }
    }
    private void RemoveField(int idx)
    {
        Undo.RecordObject(uiForm, uiForm.name);
        mFields.DeleteArrayElementAtIndex(idx);
        ArrayUtility.RemoveAt(ref mReorderableList, idx);
    }
    private void DrawTargets(Rect rect, int index, bool isActive, bool isFocused)
    {
        EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlaying);
        var field = mFields.GetArrayElementAtIndex(mCurFieldIdx);
        var targetsProperty = field.FindPropertyRelative("Targets");
        var targetProperty = targetsProperty.GetArrayElementAtIndex(index);
        var curRect = new Rect(rect.x, rect.y, 20, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(curRect, index.ToString());
        curRect.x += 20;
        curRect.width = rect.width - 20;
        EditorGUI.ObjectField(curRect, targetProperty, GUIContent.none);
        EditorGUI.EndDisabledGroup();
    }
    private static string[] GetPopupContents(GameObject[] targets)
    {
        if (targets == null || targets.Length <= 0)
        {
            return new string[0];
        }
        var typeNames = GetIntersectionComponents(targets);
        if (typeNames == null || typeNames.Length <= 0)
        {
            return new string[0];
        }
        ArrayUtility.Insert(ref typeNames, 0, typeof(GameObject).FullName);
        return typeNames;
    }
    private static string[] GetPopupContents(SerializedProperty targets)
    {
        var goArr = new GameObject[targets.arraySize];
        for (int i = 0; i < targets.arraySize; i++)
        {
            var pp = targets.GetArrayElementAtIndex(i);
            goArr[i] = (pp != null && pp.objectReferenceValue != null) ? (pp.objectReferenceValue as GameObject) : null;
        }
        return GetPopupContents(goArr);
    }
    private static string[] GetIntersectionComponents(GameObject[] targets)
    {
        var firstItm = targets[0];
        if (firstItm == null)
        {
            return new string[0];
        }
        var coms = firstItm.GetComponents(typeof(Component));
        coms = coms.Distinct().ToArray();//去重

        for (int i = coms.Length - 1; i >= 1; i--)
        {
            var comType = coms[i].GetType().FullName;
            bool allContains = true;
            for (int j = 1; j < targets.Length; j++)
            {
                var target = targets[j];
                if (target == null) return new string[0];
                var tComs = target.GetComponents(typeof(Component));
                bool containsType = false;
                for (int k = 0; k < tComs.Length; k++)
                {
                    if (tComs[k].GetType().FullName.CompareTo(comType) == 0)
                    {
                        containsType = true;
                        break;
                    }
                }
                allContains &= containsType;
                if (!allContains) break;
            }
            if (!allContains)
            {
                ArrayUtility.RemoveAt(ref coms, i);
            }
        }
        string[] typesArr = new string[coms.Length];
        for (int i = 0; i < coms.Length; i++)
        {
            typesArr[i] = coms[i].GetType().FullName;
        }
        return typesArr;
    }

    /// <summary>
    /// 生成一个与变量列表里不重名的变量名
    /// </summary>
    /// <param name="fields"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    private static string GenerateFieldName(SerializeFieldData[] fields, GameObject[] targets)
    {
        var go = targets[0];
        string varName = Regex.Replace(go.name, "[^\\w]", string.Empty);
        if (fields == null || fields.Length <= 0)
        {
            return FirstCharToLower(targets.Length > 1 ? varName + arrFlag : varName);
        }
        bool contains = false;

        foreach (SerializeFieldData item in fields)
        {
            if (item != null && item.VarName.CompareTo(varName) == 0)
            {
                contains = true;
            }
        }
        if (targets.Length > 1)
        {
            varName += arrFlag;
        }
        if (contains)
        {
            varName += go.GetInstanceID();
        }

        return FirstCharToLower(varName);
    }
    private static string FirstCharToLower(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return string.Empty;
        }
        return str.Substring(0, 1).ToLower() + str.Substring(1);
    }
}
#endif