using System;
using System.Collections.Generic;
using UnityEngine;

namespace EtherDomes.UI
{
    /// <summary>
    /// Manages aggro visual indicators.
    /// 
    /// Features:
    /// - Icon over player with aggro
    /// - Color coding in party frames by threat level
    /// - Visual line between enemy and target
    /// 
    /// Requirements: 6.2
    /// </summary>
    public class AggroIndicator : MonoBehaviour, IAggroIndicator
    {
        [Header("Aggro Icon")]
        [SerializeField] private GameObject _aggroIconPrefab;
        [SerializeField] private Vector3 _iconOffset = new Vector3(0, 2.5f, 0);

        [Header("Aggro Line")]
        [SerializeField] private Material _aggroLineMaterial;
        [SerializeField] private float _lineWidth = 0.1f;
        [SerializeField] private Color _aggroLineColor = Color.red;

        [Header("Threat Colors")]
        [SerializeField] private Color _noThreatColor = Color.gray;
        [SerializeField] private Color _lowThreatColor = Color.green;
        [SerializeField] private Color _mediumThreatColor = Color.yellow;
        [SerializeField] private Color _highThreatColor = new Color(1f, 0.5f, 0f); // Orange
        [SerializeField] private Color _aggroColor = Color.red;

        private readonly Dictionary<ulong, GameObject> _aggroIcons = new();
        private readonly Dictionary<ulong, LineRenderer> _aggroLines = new();
        private readonly Dictionary<ulong, ThreatLevel> _playerThreatLevels = new();
        private readonly Dictionary<ulong, bool> _playerAggroStates = new();

        public event Action<ulong, bool> OnAggroStateChanged;

        public void ShowAggroIcon(ulong playerId, bool hasAggro)
        {
            bool previousState = _playerAggroStates.TryGetValue(playerId, out bool prev) && prev;
            _playerAggroStates[playerId] = hasAggro;

            if (hasAggro)
            {
                if (!_aggroIcons.ContainsKey(playerId))
                {
                    CreateAggroIcon(playerId);
                }
                
                if (_aggroIcons.TryGetValue(playerId, out var icon))
                {
                    icon.SetActive(true);
                }
            }
            else
            {
                if (_aggroIcons.TryGetValue(playerId, out var icon))
                {
                    icon.SetActive(false);
                }
            }

            if (previousState != hasAggro)
            {
                OnAggroStateChanged?.Invoke(playerId, hasAggro);
                UnityEngine.Debug.Log($"[AggroIndicator] Player {playerId} aggro state: {hasAggro}");
            }
        }

        public void UpdatePartyFrameColor(ulong playerId, ThreatLevel threatLevel)
        {
            _playerThreatLevels[playerId] = threatLevel;
            
            Color color = GetThreatColor(threatLevel);
            
            // In a real implementation, this would update the actual party frame UI
            // For now, we just track the state and log
            UnityEngine.Debug.Log($"[AggroIndicator] Player {playerId} threat level: {threatLevel} (Color: {color})");
        }

        public void DrawAggroLine(ulong enemyId, Vector3 enemyPosition, Vector3 targetPosition)
        {
            if (!_aggroLines.TryGetValue(enemyId, out var lineRenderer))
            {
                lineRenderer = CreateAggroLine(enemyId);
            }

            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, enemyPosition);
                lineRenderer.SetPosition(1, targetPosition);
            }
        }

        public void HideAggroLine(ulong enemyId)
        {
            if (_aggroLines.TryGetValue(enemyId, out var lineRenderer))
            {
                lineRenderer.enabled = false;
            }
        }

        public void ClearAll()
        {
            foreach (var icon in _aggroIcons.Values)
            {
                if (icon != null)
                    Destroy(icon);
            }
            _aggroIcons.Clear();

            foreach (var line in _aggroLines.Values)
            {
                if (line != null)
                    Destroy(line.gameObject);
            }
            _aggroLines.Clear();

            _playerThreatLevels.Clear();
            _playerAggroStates.Clear();

            UnityEngine.Debug.Log("[AggroIndicator] Cleared all indicators");
        }

        public Color GetThreatColor(ThreatLevel level)
        {
            return level switch
            {
                ThreatLevel.None => _noThreatColor,
                ThreatLevel.Low => _lowThreatColor,
                ThreatLevel.Medium => _mediumThreatColor,
                ThreatLevel.High => _highThreatColor,
                ThreatLevel.Aggro => _aggroColor,
                _ => _noThreatColor
            };
        }

        public ThreatLevel GetPlayerThreatLevel(ulong playerId)
        {
            return _playerThreatLevels.TryGetValue(playerId, out var level) ? level : ThreatLevel.None;
        }

        public bool HasAggro(ulong playerId)
        {
            return _playerAggroStates.TryGetValue(playerId, out bool hasAggro) && hasAggro;
        }

        /// <summary>
        /// Calculate threat level based on percentage of tank's threat.
        /// </summary>
        public static ThreatLevel CalculateThreatLevel(float playerThreat, float tankThreat, bool hasAggro)
        {
            if (hasAggro)
                return ThreatLevel.Aggro;

            if (tankThreat <= 0 || playerThreat <= 0)
                return ThreatLevel.None;

            float percentage = playerThreat / tankThreat;

            if (percentage >= 0.8f)
                return ThreatLevel.High;
            if (percentage >= 0.5f)
                return ThreatLevel.Medium;
            if (percentage > 0)
                return ThreatLevel.Low;

            return ThreatLevel.None;
        }

        private void CreateAggroIcon(ulong playerId)
        {
            if (_aggroIconPrefab == null)
            {
                UnityEngine.Debug.LogWarning("[AggroIndicator] No aggro icon prefab assigned");
                return;
            }

            var icon = Instantiate(_aggroIconPrefab, transform);
            icon.name = $"AggroIcon_{playerId}";
            _aggroIcons[playerId] = icon;
        }

        private LineRenderer CreateAggroLine(ulong enemyId)
        {
            var lineGO = new GameObject($"AggroLine_{enemyId}");
            lineGO.transform.SetParent(transform);

            var lineRenderer = lineGO.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = _lineWidth;
            lineRenderer.endWidth = _lineWidth;
            lineRenderer.startColor = _aggroLineColor;
            lineRenderer.endColor = _aggroLineColor;

            if (_aggroLineMaterial != null)
            {
                lineRenderer.material = _aggroLineMaterial;
            }

            _aggroLines[enemyId] = lineRenderer;
            return lineRenderer;
        }

        /// <summary>
        /// Update icon position to follow player.
        /// Call this from Update if icons need to track moving players.
        /// </summary>
        public void UpdateIconPosition(ulong playerId, Vector3 playerPosition)
        {
            if (_aggroIcons.TryGetValue(playerId, out var icon) && icon != null)
            {
                icon.transform.position = playerPosition + _iconOffset;
            }
        }

        private void OnDestroy()
        {
            ClearAll();
        }
    }
}
