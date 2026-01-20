using UnityEngine;
using System.Collections.Generic;

namespace EtherDomes.Data
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "EtherDomes/Data/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        public List<ItemData> AllItems = new List<ItemData>();

        private Dictionary<string, ItemData> _lookup;

        public void Initialize()
        {
            _lookup = new Dictionary<string, ItemData>();
            foreach(var item in AllItems)
            {
                if(item != null && !string.IsNullOrEmpty(item.ItemId))
                {
                    if(!_lookup.ContainsKey(item.ItemId))
                        _lookup.Add(item.ItemId, item);
                }
            }
        }

        public ItemData GetItem(string id)
        {
            if(_lookup == null) Initialize();
            
            if(_lookup.TryGetValue(id, out var item)) return item;
            
            // Lazy re-init if not found, just in case added at runtime or inspector
            Initialize();
            if(_lookup.TryGetValue(id, out var itemRetry)) return itemRetry;

            return null;
        }
    }
}
