using UnityEngine;

namespace EtherDomes.Data
{
    /// <summary>
    /// ScriptableObject for defining world regions in the Unity Editor.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRegion", menuName = "EtherDomes/Region")]
    public class RegionDataSO : ScriptableObject
    {
        [Header("Basic Info")]
        public RegionId Id;
        public string DisplayName;
        [TextArea(2, 4)]
        public string Description;

        [Header("Level Range")]
        public int MinLevel = 1;
        public int MaxLevel = 15;

        [Header("Scene")]
        public string SceneName;

        [Header("Spawn Points")]
        public Vector3[] PlayerSpawnPoints;
        public Vector3 GraveyardLocation;

        [Header("Dungeons")]
        public string[] DungeonIds;

        [Header("Connections")]
        [Tooltip("Regions that can be accessed from this region")]
        public RegionId[] ConnectedRegions;

        public RegionData ToRegionData()
        {
            return new RegionData
            {
                Id = Id,
                DisplayName = DisplayName,
                MinLevel = MinLevel,
                MaxLevel = MaxLevel,
                SpawnPoints = PlayerSpawnPoints ?? new Vector3[0],
                DungeonIds = DungeonIds ?? new string[0]
            };
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(DisplayName))
            {
                DisplayName = Id.ToString();
            }
        }
    }

    /// <summary>
    /// Runtime region data.
    /// </summary>
    [System.Serializable]
    public class RegionData
    {
        public RegionId Id;
        public string DisplayName;
        public int MinLevel;
        public int MaxLevel;
        public Vector3[] SpawnPoints;
        public string[] DungeonIds;
    }
}
