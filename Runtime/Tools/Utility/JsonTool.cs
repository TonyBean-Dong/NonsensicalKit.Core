using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace NonsensicalKit.Tools
{
    public static class JsonTool
    {
        public static MethodInfo DESERIALIZE_METHOD = typeof(JsonConvert).GetMethods().FirstOrDefault(
                        p => p.IsStatic == true && p.IsPublic == true && p.Name == "DeserializeObject" && p.ContainsGenericParameters == true);

        /// <summary>
        /// 动态生成json(使用递归)
        /// 输入：JsonData jd=test(new string[]{"1","2","3"},10086);
        /// 结果：Debug.Log(jd["0"]["1"]["2"]);    //=10086
        /// 从litjson转为NewtonsoftJson,待测试
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static JObject Test(string[] a, int b, int pos = 0)
        {

            if (pos == a.Length)
            {
                return new JObject(b);
            }
            else
            {
                JObject jd = new JObject();

                jd[a[(int)pos]] = Test(a, b, pos + 1);

                return jd;
            }
        }

        /// <summary>
        /// 动态生成json(不使用递归)
        /// 从litjson转为NewtonsoftJson,待测试
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static JObject Test2(string[] a, int b)
        {
            JObject jd = new JObject();

            int pos = a.Length - 1;

            while (pos >= 0)
            {
                if (pos == a.Length - 1)
                {
                    jd[a[pos]] = b;
                }
                else
                {
                    //直接使用jd[a[pos]] = temp;会导致堆栈溢出异常(litjson)
                    JObject temp = jd;
                    jd = new JObject();
                    jd[a[pos]] = temp;
                }

                pos--;
            }

            return jd;
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        public static void SaveFile<T>(string fileName, T data)
        {
            string dataJson = SerializeObject(data);

            FileTool.EnsureDir(Path.Combine(Application.streamingAssetsPath, "SaveJsonFiles"));

            FileTool.WriteTxt(Path.Combine(Application.streamingAssetsPath, "SaveJsonFiles", fileName + ".json"), dataJson);
        }

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static T LoadFile<T>(string fileName)
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, "SaveJsonFiles", fileName + ".json");

            string dataJson = FileTool.ReadAllText(fullPath);
            if (dataJson == null)
            {
                return default(T);
            }

            T data = DeserializeObject<T>(dataJson);

            return data;
        }

        /// <summary>
        /// https://stackoverflow.com/a/78612
        /// Perform a deep Copy of the object, using Json as a serialization method. NOTE: Private members are not cloned using this method.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T CloneByJson<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(source, null)) return default;

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }

        public static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        private static JsonSerializerSettings IgnoreLoop = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        public static string SerializeObjectIgnoreLoop(object obj)
        {
            return JsonConvert.SerializeObject(obj, IgnoreLoop);
        }

        public static T DeserializeObject<T>(string str)
        {
            str = str.TrimBOM();
            return JsonConvert.DeserializeObject<T>(str);
        }
    }
}
