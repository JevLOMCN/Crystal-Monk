using System.IO;
using System;
using System.Windows.Forms;
using System.Reflection;

namespace Admin
{
    class IniAttribute : Attribute
    {
        public string Section;
        public IniAttribute(string section)
        {
            this.Section = section;
        }
    }


    class Settings
    {
        [Ini("Admin")]
        public static string Name = "localhost";

        [Ini("Admin")]
        public static string ServerIP = "127.0.0.1";

        [Ini("Admin")]
        public static short ServerPort = 7000;

        [Ini("Admin")]
        public static string UserName = "wang505";

        [Ini("Admin")]
        public static string Password = "5058379";

        [Ini("Admin")]
        public static int TimeOut = 5000;

        [Ini("Admin")]
        public static int SyncInvertal = 5;

        public static void ReadIni<T>(InIReader reader) where T : class, new()
        {
            Type objType = typeof(T);
            //取属性上的自定义特性
            foreach (FieldInfo fieldInfo in objType.GetFields())
            {
                object[] objAttrs = fieldInfo.GetCustomAttributes(typeof(IniAttribute), true);
                if (objAttrs.Length <= 0)
                    continue;

                IniAttribute attr = objAttrs[0] as IniAttribute;
                if (attr == null)
                    continue;

                if (reader.FindValue(attr.Section, fieldInfo.Name) == null)
                    continue;

                if (fieldInfo.FieldType == typeof(bool))
                {
                    bool value = reader.ReadBoolean(attr.Section, fieldInfo.Name, false);
                    fieldInfo.SetValue(null, value);
                }
                else if (fieldInfo.FieldType == typeof(int))
                {
                    int value = reader.ReadInt32(attr.Section, fieldInfo.Name, 0);
                    fieldInfo.SetValue(null, value);
                }
                else if (fieldInfo.FieldType == typeof(string))
                {
                    string value = reader.ReadString(attr.Section, fieldInfo.Name, "");
                    fieldInfo.SetValue(null, value);
                }
                else if (fieldInfo.FieldType == typeof(short))
                {
                    short value = reader.ReadInt16(attr.Section, fieldInfo.Name, 0);
                    fieldInfo.SetValue(null, value);
                }
            }
        }

        public static void Save(string name)
        {
            InIReader Reader = new InIReader(name + ".ini");
            WriteIni<Settings>(Reader);
        }

        public static void WriteIni<T>(InIReader reader) where T : class, new()
        {
            Type objType = typeof(T);
            //取属性上的自定义特性
            foreach (FieldInfo fieldInfo in objType.GetFields())
            {
                object[] objAttrs = fieldInfo.GetCustomAttributes(typeof(IniAttribute), true);

                if (objAttrs.Length <= 0)
                    continue;

                IniAttribute attr = objAttrs[0] as IniAttribute;
                if (attr == null)
                    continue;

                if (fieldInfo.FieldType == typeof(bool))
                    reader.Write(attr.Section, fieldInfo.Name, (bool)fieldInfo.GetValue(null));
                else if (fieldInfo.FieldType == typeof(int))
                    reader.Write(attr.Section, fieldInfo.Name, (int)fieldInfo.GetValue(null));
                else if (fieldInfo.FieldType == typeof(string))
                    reader.Write(attr.Section, fieldInfo.Name, (string)fieldInfo.GetValue(null));
                else if (fieldInfo.FieldType == typeof(short))
                    reader.Write(attr.Section, fieldInfo.Name, (short)fieldInfo.GetValue(null));
            }
        }
    }
}
