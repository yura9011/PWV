using UnityEngine;

namespace EtherDomes.Data
{
    /// <summary>
    /// ScriptableObject for defining dungeons in the Unity Editor.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDungeon", menuName = "EtherDomes/Dungeon")]
    public class DungeonDataSO : ScriptableObject
    {
        [Header("Basic Info")]
        public string DungeonId;
        public string DungeonName;
        [TextArea(2, 4)]
        public string Description;

        [Header("Properties")]
        public DungeonSize Size = DungeonSize.Small;
        public int MinLevel = 15;
        public int MaxLevel = 20;
        public RegionId Region;

        [Header("Scene")]
        public string SceneName;

        [Header("Bosses")]
        public BossDataSO[] Bosses;

        [Header("Spawn")]
        public Vector3 EntranceSpawnPoint;
        public Vector3[] CheckpointLocations;

        public int BossCount => Bosses?.Length ?? 0;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(DungeonId))
            {
                DungeonId = System.Guid.NewGuid().ToString();
            }

            // Validate boss count matches dungeon size
            int expectedBosses = Size == DungeonSize.Small ? 3 : 5;
            if (Bosses != null && Bosses.Length != expectedBosses)
            {
                Debug.LogWarning($"Dungeon {DungeonName}: Expected {expectedBosses} bosses for {Size} dungeon, but has {Bosses.Length}");
            }
        }
    }
}
