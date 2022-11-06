//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.IO;

namespace GameFramework.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class JsonDataProcessor : GenericDataProcessor<LitJson.JsonData>
        {
            public override bool IsSystem
            {
                get
                {
                    return false;
                }
            }

            public override string LanguageKeyword
            {
                get
                {
                    return "JsonData";
                }
            }

            public override string[] GetTypeStrings()
            {
                return new string[]
                {
                    "jsondata",
                    "litjson.jsondata"
                };
            }

            public override LitJson.JsonData Parse(string value)
            {
                return LitJson.JsonMapper.ToObject(value);
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write(value);
            }
        }
    }
}
