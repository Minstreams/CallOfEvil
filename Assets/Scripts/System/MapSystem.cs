﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameSystem.PresentSetting;

namespace GameSystem
{
    /// <summary>
    /// 地图系统，地图生成，地图动态管理，地图物体网络同步
    /// </summary>
    public class MapSystem : SubSystem<MapSystemSetting>
    {
        //常量-----------------------------------------------------------
        /// <summary>
        /// 每圈组数
        /// </summary>
        public const int GroupCountPerCircle = 3;
        /// <summary>
        /// 每组角度
        /// </summary>
        public const float AnglePerGroup = 360 / GroupCountPerCircle;


        //引用
        public static MapSystemComponent mapSystemComponent = null;


        /// <summary>
        /// 是否所有Group都已经加载好？(场景加载时向左移一位，加载完毕右移，这意味着支持最多同时31个场景加载，足够用了)
        /// </summary>
        public static int loaded = 1;
        /// <summary>
        /// 系统是否激活（可工作）
        /// </summary>
        public static bool Active { get { return mapSystemComponent != null && loaded == 1; } }





        //地图生成控制---------------------------------------------------
        /// <summary>
        /// 当前最大圈数,游戏流程推进的时候，更改这个值并重新生成地图
        /// </summary>
        public static int CircleCount { get { if (Active) return mapSystemComponent.circleCount; else return 1; } set { if (Active) mapSystemComponent.circleCount = value; } }

#if UNITY_EDITOR
        //显示在Setting编辑器上的说明，放在这个位置好改
        public const string 地图生成方案挑选方式说明 = "挑选第 【最大玩家数-当前玩家数】 套 地图生成方案";
        public const string 地图生成逻辑说明 = "每个组生成概率和权值成正比";
#endif
        /// <summary>
        /// 挑选地图生成方案
        /// </summary>
        public static MapSystemSetting.MapGernerationPlan ChooseMapGenerationInfo(int seed)
        {
            //随机数可以考虑利用seed自定义算法
            return Setting.mapGernerationPlans[GameLevelSystem.MaxPlayerCount - CircleCount];
        }
        public static Dictionary<string, MapGroupAsset> MapGroupAssets { get { return Setting.mapGroupAssets; } }

        /// <summary>
        /// 使用一个seed生成地图，调用后会自动用seed初始化随机数生成器
        /// </summary>
        public static void GenerateMap(MapSystemSetting.MapGernerationPlan plan, int seed)
        {
            Random.InitState(seed);

            //地图生成逻辑：
            ClearMap();

            float fullWeight = 0;
            foreach (MapSystemSetting.GroupGenerationPlan p in plan.planList)
            {
                fullWeight += p.weight;
            }

            for (int i = 0; i < GroupCount; i++)
            {
                float rand = Random.value;
                groupList.Add(null);

                foreach (MapSystemSetting.GroupGenerationPlan p in plan.planList)
                {
                    if (p.weight > rand)
                    {
                        LoadGroup(p.groupAsset, i);
                        break;
                    }
                    else
                    {
                        rand -= p.weight;
                    }
                }
            }

        }

        /// <summary>
        /// 清空场景，并清空Group表。调用该方法后请立即重建Group表！否则可能会有未知问题
        /// </summary>
        public static void ClearMap()
        {
            foreach (MapGroup group in groupList)
            {
                if (group != null) UnLoadGroup(group);
            }
        }

        /// <summary>
        /// 加载Group，返回加载进度
        /// </summary>
        /// <returns>返回加载进度</returns>
        public static AsyncOperation LoadGroup(MapGroupAsset group, int index)
        {
#if UNITY_EDITOR
            if (groupList[index] != null)
            {
                Debug.LogError("LoadGroup前请先UnloadGroup！");
                UnLoadGroup(groupList[index]);
            }
#endif

            loaded <<= 1;
            string sceneName = group.groupName + index % 3;
            Scene activeScene = SceneManager.GetActiveScene();

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            StartCoroutine(loadGroup(operation, index, sceneName, activeScene));

            return operation;
        }
        private static IEnumerator loadGroup(AsyncOperation operation, int index, string sceneName, Scene activeScene)
        {
            while (!operation.isDone) yield return 0;
            groupList[index] = SceneManager.GetSceneByName(sceneName).GetRootGameObjects()[0].GetComponent<MapGroup>();
            groupList[index].index = index;
            SceneManager.SetActiveScene(activeScene);
            loaded >>= 1;
        }

        /// <summary>
        /// 卸载Group
        /// </summary>
        public static void UnLoadGroup(MapGroup group)
        {
#if UNITY_EDITOR
            if (group == null)
            {
                Debug.LogError("不能卸载空组！");
                return;
            }
#endif

            int index = group.index;

            //卸载多余场景
            if (group.gameObject.scene != SceneManager.GetActiveScene())
            {
#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying)
                    UnityEditor.SceneManagement.EditorSceneManager.CloseScene(group.gameObject.scene, true);
                else
#endif
                    SceneManager.UnloadSceneAsync(group.gameObject.scene);
            }
            else
            {
                GameObject.Destroy(group.gameObject);
            }

            //引用置为空
            groupList[index] = null;
        }








        //现有地图管理---------------------------------------------------
        /// <summary>
        /// 组记录表
        /// </summary>
        public static List<MapGroup> groupList
        {
            get
            {
                if (Active) return mapSystemComponent.groupList;
                else
                {
                    Debug.LogError("地图系统未激活！");
                    return null;
                }
            }
        }
        /// <summary>
        /// 当前组序号
        /// </summary>
        public static int currentGroupIndex;
        /// <summary>
        /// 当前角度
        /// </summary>
        public static float currentAngle;
        /// <summary>
        /// 当前圈数(0~n-1)
        /// </summary>
        public static int CurrentCircle { get { return currentGroupIndex / GroupCountPerCircle; } }

        //Angle计算
        public static float MaxAngle { get { return CircleCount * 360; } }
        private static float HalfMaxAngle { get { return MaxAngle / 2; } }

        /// <summary>
        /// 根据位置获取Unit角度，用于排序
        /// </summary>
        /// <param name="unitPos">位置</param>
        /// <returns>角度</returns>
        public static float GetAngle(Vector3 unitPos)
        {
            Vector2 pos = new Vector2(unitPos.x, unitPos.z);
            float angle = Rotate(CurrentCircle * 360, Vector2.SignedAngle(pos, Vector2.right));
            //用Group位置信息辅助计算
            if (SubSigned(angle, Sub(currentGroupIndex * AnglePerGroup, AnglePerGroup)) < 0)
            {
                angle = Add(angle, 360);
            }
            return angle;
        }
        /// <summary>
        /// 角度相减，返回差值
        /// </summary>
        public static float SubSigned(float angle1, float angle2)
        {
            float result = angle1 - angle2;
            if (result > HalfMaxAngle) return result - MaxAngle;
            if (result < -HalfMaxAngle) return result + MaxAngle;
            return result;
        }
        /// <summary>
        /// 角度加增量，返回角度
        /// </summary>
        /// <param name="angle">角度</param>
        /// <param name="increment">增量(-360~360)</param>
        private static float Rotate(float angle, float increment)
        {
            if (increment > 0) return Add(angle, increment);
            else return Sub(angle, -increment);
        }
        /// <summary>
        /// 角度相减，返回角度
        /// </summary>
        private static float Sub(float angle1, float angle2)
        {
            float result = angle1 - angle2;
            if (result < 0) return result + MaxAngle;
            return result;
        }
        /// <summary>
        /// 角度相加，返回角度
        /// </summary>
        private static float Add(float angle1, float angle2)
        {
            float result = angle1 + angle2;
            float m = MaxAngle;
            if (result >= m) return result - m;
            return result;
        }

        //Group Index计算
        public static int GroupCount { get { return GroupCountPerCircle * CircleCount; } }

        /// <summary>
        /// 环向搜索的下一个位置
        /// </summary>
        private static int GetNext(int ptr)
        {
            if (ptr == GroupCount - 1) return 0;
            else return ptr + 1;
        }
        /// <summary>
        /// 环向搜索的上一个
        /// </summary>
        private static int GetPrevious(int ptr)
        {
            if (ptr == 0) return GroupCount - 1;
            else return ptr - 1;
        }

        /// <summary>
        /// 记录当前所在角度，并根据新角度动态刷新Group
        /// </summary>
        public static void SetCurrentAngle(float angle)
        {
            if (!Active) return;

            int newGroupIndex = (int)(angle / AnglePerGroup);
            currentAngle = angle;

            //用一种没有可读性的方式，实现了Group的动态刷新
            int distance = newGroupIndex - currentGroupIndex;
            if (distance > 0) for (int i = 0, oldLeft = GetPrevious(currentGroupIndex), newLeft = GetPrevious(newGroupIndex), newRight = GetNext(newGroupIndex); i < 3 && oldLeft != newLeft; i++, oldLeft = GetNext(oldLeft), newRight = GetPrevious(newRight))
                {
                    groupList[oldLeft].Active = false;
                    groupList[newRight].Active = true;
                }
            else if (distance < 0) for (int i = 0, oldRight = GetNext(currentGroupIndex), newLeft = GetPrevious(newGroupIndex), newRight = GetNext(newGroupIndex); i < 3 && oldRight != newRight; i++, oldRight = GetPrevious(oldRight), newLeft = GetNext(newLeft))
                {
                    groupList[oldRight].Active = false;
                    groupList[newLeft].Active = true;
                }

            currentGroupIndex = newGroupIndex;
        }

        public static void InitGroupActiveState()
        {
            currentGroupIndex = (int)(currentAngle / AnglePerGroup); ;

            int loopEnd = GetPrevious(currentGroupIndex);
            int ptr = GetNext(currentGroupIndex);

            groupList[loopEnd].Active = true;
            groupList[currentGroupIndex].Active = true;
            groupList[ptr].Active = true;

            ptr = GetNext(ptr);

            while (ptr != loopEnd)
            {
                groupList[ptr].Active = false;
                ptr = GetNext(ptr);
            }
        }






        //网络同步管理---------------------------------------------------
        //TODO
    }
}