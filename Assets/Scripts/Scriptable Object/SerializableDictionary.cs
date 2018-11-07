using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 可序列化的Dictionary，暂不支持在Inspector中编辑
/// </summary>
[System.Serializable]
public class SerializableDictionary<Key, Value>
{
    [SerializeField]
    private List<Key> keyList = new List<Key>();
    [SerializeField]
    private List<Value> valueList = new List<Value>();
    [SerializeField]
    private int count = 0;

    private Dictionary<Key, Value> _dictionary = null;
    private Dictionary<Key, Value> dictionary
    {
        get
        {
            if (_dictionary == null)
            {
                _dictionary = new Dictionary<Key, Value>();
                for (int i = 0; i < count; i++)
                {
                    _dictionary.Add(keyList[i], valueList[i]);
                }
            }
            return _dictionary;
        }
    }

    public Value this[Key key]
    {
        get { return dictionary[key]; }
        set
        {
            valueList[keyList.IndexOf(key)] = value;
            dictionary[key] = value;
        }
    }

    public int Count { get { return count; } }

    public bool ContainsKey(Key key)
    {
        if (key == null) return false;
        return dictionary.ContainsKey(key);
    }

    public void Add(Key key, Value value)
    {
        dictionary.Add(key, value);
        keyList.Add(key);
        valueList.Add(value);
        count++;
    }

    public bool Remove(Key key)
    {
        if (!dictionary.ContainsKey(key)) return false;
        Value value = dictionary[key];
        dictionary.Remove(key);
        keyList.Remove(key);
        valueList.Remove(value);
        count--;
        return true;
    }

    public void Clear()
    {
        dictionary.Clear();
        keyList.Clear();
        valueList.Clear();
        count = 0;
    }

    public void ClearNullptr()
    {
        if (_dictionary != null)
        {
            _dictionary.Clear();
            _dictionary = null;
        }
        for (int i = 0; i < count; i++)
        {
            if (keyList[i] == null || keyList[i].ToString() == "null"/*直接判断 == null 会判断错误*/)
            {
                keyList.RemoveAt(i);
                valueList.RemoveAt(i);
                i--;
                count--;
            }
        }
    }
}
