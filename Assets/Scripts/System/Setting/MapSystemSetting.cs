using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameSystem
{
    namespace PresentSetting
    {
        [CreateAssetMenu(fileName = "MapSystemSetting", menuName = "系统配置文件/Map System Setting")]
        public class MapSystemSetting : ScriptableObject
        {
            /// <summary>
            /// 单个组的生成方案
            /// </summary>
            [System.Serializable]
            public struct GroupGenerationPlan
            {
                public MapGroupAsset groupAsset;
                public float weight;
            }

            /// <summary>
            /// 地图生成方案
            /// </summary>
            [System.Serializable]
            public class MapGernerationPlan
            {
                public List<GroupGenerationPlan> planList = new List<GroupGenerationPlan>();

                public GroupGenerationPlan this[int i] { get { return planList[i]; } }
            }

            /// <summary>
            /// 地图生成控制参数
            /// </summary>
            public List<MapGernerationPlan> mapGernerationPlans;

            /// <summary>
            /// 所有地图组预设
            /// </summary>
            public Dictionary<string, MapGroupAsset> mapGroupAssets = new Dictionary<string, MapGroupAsset>();
        }
    }
}
