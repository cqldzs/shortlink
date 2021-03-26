using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shortlink
{
    public class Helper
    {
        public static Dictionary<string, string> ParseQueryString(string url,string requestType="post")
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException("url");
            }
            var uri = new Uri(url);
            if (string.IsNullOrWhiteSpace(uri.Query))
            {
                return new Dictionary<string, string>();
            }

            var dic = new Dictionary<string, string>();
            if (requestType.ToLower()=="get" && uri.Query.Substring(1).ToLower().StartsWith("url=http"))
            {
                string urlstr = uri.Query.Substring(1);
                dic["url"] = urlstr.Substring(urlstr.IndexOf("url=") + 4);
            }
            else if (requestType.ToLower() == "get")
            {
                //1.去除第一个前导?字符
                dic = uri.Query.Substring(1)
                        //2.通过&划分各个参数
                        .Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                        //3.通过=划分参数key和value,且保证只分割第一个=字符
                        .Select(param => param.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries))
                        //4.通过相同的参数key进行分组
                        .GroupBy(part => part[0], part => part.Length > 1 ? part[1] : string.Empty)
                        //5.将相同key的value以,拼接
                        .ToDictionary(group => group.Key, group => string.Join(",", group));


                #region 判断url是否是最后一个参数
                //将Enum枚举转换成字符串数组
                string[] arrNames = Enum.GetNames(typeof(OperationType));

                string urlstr = uri.Query.Substring(1);
                int i = urlstr.IndexOf("url=") + 4;
                bool isLastUrl = true;

                //遍历字符串数组
                foreach (string strName in arrNames)
                {
                    if (i < urlstr.IndexOf(strName + "="))
                    {
                        isLastUrl = false;
                    }
                }
                if (isLastUrl && dic.ContainsKey("url"))
                {
                    dic["url"] = urlstr.Substring(urlstr.IndexOf("url=") + 4);

                } 
                #endregion
            }
            else
            {
                //1.去除第一个前导?字符
                dic = uri.Query.Substring(1)
                        //2.通过&划分各个参数
                        .Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                        //3.通过=划分参数key和value,且保证只分割第一个=字符
                        .Select(param => param.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries))
                        //4.通过相同的参数key进行分组
                        .GroupBy(part => part[0], part => part.Length > 1 ? part[1] : string.Empty)
                        //5.将相同key的value以,拼接
                        .ToDictionary(group => group.Key, group => string.Join(",", group));
            }
            return dic;
        }

        public static string IntToi32(long xx)
        {
            string a = "";
            while (xx >= 1)
            {
                int index = Convert.ToInt16(xx - (xx / 32) * 32);
                a = Base64Code[index] + a;
                xx = xx / 32;
            }
            return a;
        }

        public static long i32ToInt(string xx)
        {
            long a = 0;
            int power = xx.Length - 1;

            for (int i = 0; i <= power; i++)
            {
                a += _Base64Code[xx[power - i].ToString()] * Convert.ToInt64(Math.Pow(32, i));
            }

            return a;
        }

        public static string GetRandomStr()
        {
            string str = Helper.Str(1) + Helper.IntToi64(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()).Insert(2, Helper.Str(1)) + Helper.Str(1);
            return str;
        }

        /// <summary>
        /// 将字典类型序列化为json字符串
        /// </summary>
        /// <typeparam name="TKey">字典key</typeparam>
        /// <typeparam name="TValue">字典value</typeparam>
        /// <param name="dict">要序列化的字典数据</param>
        /// <returns>json字符串</returns>
        public static string SerializeDictionaryToJsonString<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            if (dict.Count == 0)
                return "";

            string jsonStr = JsonConvert.SerializeObject(dict);
            return jsonStr;
        }

        /// <summary>
        /// 将json字符串反序列化为字典类型
        /// </summary>
        /// <typeparam name="TKey">字典key</typeparam>
        /// <typeparam name="TValue">字典value</typeparam>
        /// <param name="jsonStr">json字符串</param>
        /// <returns>字典数据</returns>
        public static Dictionary<TKey, TValue> DeserializeStringToDictionary<TKey, TValue>(string jsonStr)
        {
            if (string.IsNullOrEmpty(jsonStr))
                return new Dictionary<TKey, TValue>();

            Dictionary<TKey, TValue> jsonDict = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(jsonStr);

            return jsonDict;

        }

        public static string IntToi64(long xx)
        {
            string a = "";
            while (xx >= 1)
            {
                int index = Convert.ToInt16(xx - (xx / 64) * 64);
                a = Base64Code[index] + a;
                xx = xx / 64;
            }
            return a;
        }

        public static long i64ToInt(string xx)
        {
            long a = 0;
            int power = xx.Length - 1;

            for (int i = 0; i <= power; i++)
            {
                a += _Base64Code[xx[power - i].ToString()] * Convert.ToInt64(Math.Pow(64, i));
            }

            return a;
        }

        public static Dictionary<int, string> Base64Code = new Dictionary<int, string>() {
            {   0  ,"z"}, {   1  ,"1"}, {   2  ,"2"}, {   3  ,"3"}, {   4  ,"4"}, {   5  ,"5"}, {   6  ,"6"}, {   7  ,"7"}, {   8  ,"8"}, {   9  ,"9"},
            {   10  ,"a"}, {   11  ,"b"}, {   12  ,"c"}, {   13  ,"d"}, {   14  ,"e"}, {   15  ,"f"}, {   16  ,"g"}, {   17  ,"h"}, {   18  ,"i"}, {   19  ,"j"},
            {   20  ,"k"}, {   21  ,"x"}, {   22  ,"m"}, {   23  ,"n"}, {   24  ,"y"}, {   25  ,"p"}, {   26  ,"q"}, {   27  ,"r"}, {   28  ,"s"}, {   29  ,"t"},
            {   30  ,"u"}, {   31  ,"v"}, {   32  ,"w"}, {   33  ,"x"}, {   34  ,"y"}, {   35  ,"z"}, {   36  ,"A"}, {   37  ,"B"}, {   38  ,"C"}, {   39  ,"D"},
            {   40  ,"E"}, {   41  ,"F"}, {   42  ,"G"}, {   43  ,"H"}, {   44  ,"I"}, {   45  ,"J"}, {   46  ,"K"}, {   47  ,"L"}, {   48  ,"M"}, {   49  ,"N"},
            {   50  ,"O"}, {   51  ,"P"}, {   52  ,"Q"}, {   53  ,"R"}, {   54  ,"S"}, {   55  ,"T"}, {   56  ,"U"}, {   57  ,"V"}, {   58  ,"W"}, {   59  ,"X"},
            {   60  ,"Y"}, {   61  ,"Z"}, {   62  ,"-"}, {   63  ,"_"},
        };

        public static Dictionary<string, int> _Base64Code
        {
            get
            {
                return Enumerable.Range(0, Base64Code.Count()).ToDictionary(i => Base64Code[i], i => i);
            }
        }

        /// <summary>
        /// 生成随机字母与数字
        /// </summary>
        /// <param name="IntStr">生成长度</param>
        /// <returns></returns>
        public static string Str(int Length)
        {
            return Str(Length, false);
        }
        /// <summary>
        /// 生成随机字母与数字
        /// </summary>
        /// <param name="Length">生成长度</param>
        /// <param name="Sleep">是否要在生成前将当前线程阻止以避免重复</param>
        /// <returns></returns>
        public static string Str(int Length, bool Sleep)
        {
            if (Sleep)
                System.Threading.Thread.Sleep(3);
            char[] Pattern = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            string result = "";
            int n = Pattern.Length;
            System.Random random = new Random(~unchecked((int)DateTime.Now.Ticks));
            for (int i = 0; i < Length; i++)
            {
                int rnd = random.Next(0, n);
                result += Pattern[rnd];
            }
            return result;
        }

    }
}
