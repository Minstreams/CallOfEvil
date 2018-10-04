using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameSystem
{
    /// <summary>
    /// 母体，游戏流程控制与消息处理
    /// </summary>
    [DisallowMultipleComponent]
    public class TheMatrix : MonoBehaviour
    {
        //流程--------------------------------
        private IEnumerator start()
        {
            //这里写游戏流程控制代码
            throw new System.NotImplementedException();
        }
















#if UNITY_EDITOR
        public bool test;
#endif
        private static TheMatrix instance;
        private static TheMatrix Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError("没有加载TheMatrix！");
                }
                return instance;
            }
        }

        //游戏控制----------------------------
        /// <summary>
        /// 记录游戏控制信息
        /// </summary>
        private static bool[] gameMessageReciver = new bool[System.Enum.GetValues(typeof(GameMessage)).Length];
        /// <summary>
        /// 检查游戏控制信息，收到则返回true
        /// </summary>
        /// <param name="message">要检查的信息</param>
        /// <param name="reset">是否在接收后重置</param>
        /// <returns>检查按钮信息，收到则返回true</returns>
        public static bool GetGameMessage(GameMessage message, bool reset = true)
        {
            if (gameMessageReciver[(int)message])
            {
                if (reset)
                    gameMessageReciver[(int)message] = false;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 发送 游戏控制信息
        /// </summary>
        /// <param name="message">信息</param>
        public static void SendGameMessage(GameMessage message)
        {
            gameMessageReciver[(int)message] = true;
        }
        /// <summary>
        /// 重置
        /// </summary>
        public static void ResetGameMessage()
        {
            gameMessageReciver.Initialize();
        }


        //协程控制----------------------------
        private static Dictionary<System.Type, LinkedList<Coroutine>> routineDictionaty = new Dictionary<System.Type, LinkedList<Coroutine>>();

        public static LinkedListNode<Coroutine> StartCoroutine(IEnumerator routine, System.Type type)
        {
            LinkedList<Coroutine> linkedList;
            if (routineDictionaty.ContainsKey(type))
            {
                linkedList = routineDictionaty[type];
            }
            else
            {
                linkedList = new LinkedList<Coroutine>();
                routineDictionaty.Add(type, linkedList);
            }
            LinkedListNode<Coroutine> node = new LinkedListNode<Coroutine>(null);
            node.Value = Instance.StartCoroutine(SubCoroutine(routine, node));
            linkedList.AddLast(node);

            return node;
        }
        public static void StopAllCoroutines(System.Type type)
        {
            if (!routineDictionaty.ContainsKey(type)) return;
            LinkedList<Coroutine> linkedList = routineDictionaty[type];

            foreach (Coroutine c in linkedList)
            {
                Instance.StopCoroutine(c);
            }

            linkedList.Clear();
        }
        public static void StopCoroutine(LinkedListNode<Coroutine> node)
        {
            if (node == null || node.List == null) return;
            Instance.StopCoroutine(node.Value);
            node.List.Remove(node);
        }
        private static IEnumerator SubCoroutine(IEnumerator inCoroutine, LinkedListNode<Coroutine> node)
        {
            yield return inCoroutine;
            node.List.Remove(node);
        }


        //存档控制----------------------------
        [SerializeField]
        private SavableObject[] dataToSave;

        private static void SaveTemporary(SavableObject data)
        {
            string stream = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(data.ToString(), stream);
            data.saved = true;
            Debug.Log(data.name + " \tsaved!");
        }
        public static void Save(SavableObject data)
        {
            SaveTemporary(data);
            PlayerPrefs.Save();
            Debug.Log("Data saved to disc.");
        }
        public static void Load(SavableObject data)
        {
            if (!PlayerPrefs.HasKey(data.ToString()))
            {
                Debug.Log("No data found for " + data.name);
                return;
            }
            string stream = PlayerPrefs.GetString(data.ToString());
            JsonUtility.FromJsonOverwrite(stream, data);
            data.saved = true;
            Debug.Log(data.name + " \tloaded!");
        }

        [ContextMenu("Save All Data")]
        public void SaveAll()
        {
            foreach (SavableObject so in dataToSave)
            {
                if (so.saved) continue;
                SaveTemporary(so);
            }
            PlayerPrefs.Save();
            Debug.Log("Data saved to disc.");
        }
        public void LoadAll()
        {
            foreach (SavableObject so in dataToSave)
            {
                Load(so);
            }
        }
        [ContextMenu("Delete All Data")]
        public void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("All saved data deleted!");
        }
        
        
        //游戏启动----------------------------
        private void Awake()
        {
            instance = this;
        }
        private void Start()
        {
#if UNITY_EDITOR
            if (test)
#endif
                StartCoroutine(start());
            LoadAll();
        }
        private void OnDestroy()
        {
            SaveAll();
        }
    }

    /// <summary>
    /// 控制信息枚举
    /// </summary>
    public enum GameMessage
    {
        Start,
        Win,
        Lose,
        Exit
    }
}
