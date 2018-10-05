using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    /// <summary>
    /// 所有子系统的父类。
    /// </summary>
    /// <typeparam name="SubSetting"></typeparam>
    public abstract class SubSystem<SubSetting> where SubSetting : ScriptableObject
    {
        private static SubSetting _Setting;
        protected static SubSetting Setting
        {
            get
            {
                if (_Setting == null)
                {
                    _Setting = Resources.Load<SubSetting>("System/" + typeof(SubSetting).ToString().Substring(26));
                }
                return _Setting;
            }
        }
    }
}