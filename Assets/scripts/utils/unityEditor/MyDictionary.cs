using System.Collections.Generic;
using System;

using UnityEditor;

using UnityEngine;

[Serializable]
public class MyDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{

    [Serializable]
    struct SerializableKeyValuePair
    {
        public TKey Key;
        public TValue Value;
        public SerializableKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }

    [SerializeField]
    private List<SerializableKeyValuePair> keyValuePairs = new List<SerializableKeyValuePair>();

    public void OnBeforeSerialize()
    {
        keyValuePairs.Clear();

        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keyValuePairs.Add(new SerializableKeyValuePair(pair.Key, pair.Value));
        }
    }

    public void OnAfterDeserialize()
    {
        this.Clear();

        foreach (SerializableKeyValuePair keyValuePair in keyValuePairs)
        {
            this.Add(keyValuePair.Key, keyValuePair.Value);
        }
    }
}
