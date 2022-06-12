using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace GameBase
{
    /// <summary>
    /// 恋庭で使用するLogクラス
    /// Debug.Logの代わりに使用する。ここも戻す
    /// </summary>
    public static class CommonLogger
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static ILogger _logger = UnityEngine.Debug.unityLogger;//ここは戻す
#endif

        private const int MAX_LOG_COUNT = 10;
        private const int MAX_LOG_TEXT_LENGTH = 300;
        private static Queue<string> logHolder = new Queue<string>();

        public static int collectedLogCount
        {
            get
            {
                return logHolder.Count;
            }
        }

        /// <summary>
        ///    LogType
        ///    Error = 0,
        ///    Assert = 1,
        ///    Warning = 2,
        ///    Log = 3,
        ///    Exception = 4
        /// </summary>
        public static int LogOutBit = 0;

        public static void Log(System.Object obj, string tag = "", LogType type = LogType.Log)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (tag != "")
            {
                _logger.Log(type, tag, obj);
            }
            else
            {
                _logger.Log(type, obj);
            }
#else
        Application_logMessageReceivedThreaded(obj.ToString(), UnityEngine.StackTraceUtility.ExtractStackTrace(), type);
#endif
        }

        public static void LogWarning(System.Object obj, string tag = "")
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _logger.LogWarning(tag, obj);
#else
            Application_logMessageReceivedThreaded(obj.ToString(), UnityEngine.StackTraceUtility.ExtractStackTrace(), LogType.Warning);
#endif
        }

        public static void LogError(System.Object obj, string tag = "")
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _logger.LogError(tag, obj);
#else
            Application_logMessageReceivedThreaded(obj.ToString(), UnityEngine.StackTraceUtility.ExtractStackTrace(), LogType.Error);
#endif
        }

        public static void LogException(System.Exception ex)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _logger.LogException(ex);
#else
            Application_logMessageReceivedThreaded(ex.Message, UnityEngine.StackTraceUtility.ExtractStringFromException(ex), LogType.Exception);
#endif
        }

        [Conditional("DEVELOPMENT_BUILD")]
        public static void CreateGUIConsole()
        {
        }

        public static void Application_logMessageReceivedThreaded(string condition, string stackTrace, LogType type)
        {
            if ((LogOutBit & (1 << (int)type)) != 0)
            {
                while (logHolder.Count > MAX_LOG_COUNT)
                {
                    logHolder.Dequeue();
                }

                string str = string.Format("{0}\n{1}\n{2}", DateTime.UtcNow.Ticks, condition, stackTrace);
                logHolder.Enqueue(str.Length <= MAX_LOG_TEXT_LENGTH ? str : str.Substring(0, MAX_LOG_TEXT_LENGTH));
            }
        }

        public static void SaveLog()
        {
            if (logHolder.Count == 0)
            {
                // PlayerPrefsUtil.System.SetCollectedLog(string.Empty);
            }
            else
            {
                string serializedData = JsonConvert.SerializeObject(logHolder);
                // PlayerPrefsUtil.System.SetCollectedLog(serializedData);
            }
        }

        public static void LoadLog()
        {
            /*
            string serializedData = PlayerPrefsUtil.System.GetCollectedLog();

            if (string.IsNullOrEmpty(serializedData) == false)
            {
                PlayerPrefsUtil.System.SetCollectedLog(string.Empty);
                try
                {
                    var deserializedData = JsonConvert.DeserializeObject<IEnumerable<string>>(serializedData);
                    foreach (var data in deserializedData)
                    {
                        while (logHolder.Count > MAX_LOG_COUNT)
                        {
                            logHolder.Dequeue();
                        }
                        logHolder.Enqueue(data);
                    }
                }
                catch (Exception e)
                {
                    LogException(e);
                }
            }
            */
        }

        public static List<string> GetLog()
        {
            return new List<string>(logHolder);
        }

        public static void Clear()
        {
            logHolder.Clear();
        }
    }
}