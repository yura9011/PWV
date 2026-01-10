using System;
using EtherDomes.Combat;
using EtherDomes.Data;
using UnityEngine;
using UnityEngine.UI;

namespace EtherDomes.UI
{
    /// <summary>
    /// UI component for the ability action bar.
    /// Displays 9 ability slots with cooldowns and GCD.
    /// </summary>
    public class ActionBar : MonoBehaviour
    {
        [Header("Slots")]
        [SerializeField] private ActionBarSlot[] _slots = new ActionBarSlot[9];

        [Header("References")]
        [SerializeField] private IAbilitySystem _abilitySystem;

        private void Update()
        {
            if (_abilitySystem == null) return;

            UpdateSlots();
            ProcessInput();
        }

        private void UpdateSlots()
        {
            var abilities = _abilitySystem.GetAbilities();
            bool isOnGCD = _abilitySystem.IsOnGCD;
            float gcdRemaining = _abilitySystem.GCDRemaining;

            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] == null) continue;

                if (i < abilities.Length && abilities[i]?.Data != null)
                {
                    var abilityState = abilities[i];
                    var abilityData = abilityState.Data;
                    float cooldown = _abilitySystem.GetCooldownRemaining(i);
                    
                    // Show GCD if ability is affected by it
                    float displayCooldown = abilityData.AffectedByGCD && isOnGCD 
                        ? Mathf.Max(cooldown, gcdRemaining) 
                        : cooldown;

                    _slots[i].UpdateSlot(abilityData, displayCooldown);
                }
                else
                {
                    _slots[i].ClearSlot();
                }
            }
        }

        private void ProcessInput()
        {
            // Check for ability key presses (1-9)
            for (int i = 0; i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    _abilitySystem.TryExecuteAbility(i);
                }
            }
        }

        /// <summary>
        /// Set the ability system reference.
        /// </summary>
        public void Initialize(IAbilitySystem abilitySystem)
        {
            _abilitySystem = abilitySystem;
        }
    }

    /// <summary>
    /// Individual slot in the action bar.
    /// </summary>
    [Serializable]
    public class ActionBarSlot : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _cooldownOverlay;
        [SerializeField] private TMPro.TextMeshProUGUI _cooldownText;
        [SerializeField] private TMPro.TextMeshProUGUI _keyBindText;

        private AbilityData _currentAbility;

        public void UpdateSlot(AbilityData ability, float cooldownRemaining)
        {
            _currentAbility = ability;

            if (_iconImage != null)
            {
                _iconImage.sprite = ability.Icon;
                _iconImage.enabled = ability.Icon != null;
            }

            // Update cooldown display
            bool onCooldown = cooldownRemaining > 0;

            if (_cooldownOverlay != null)
            {
                _cooldownOverlay.enabled = onCooldown;
                if (onCooldown && ability.Cooldown > 0)
                {
                    _cooldownOverlay.fillAmount = cooldownRemaining / ability.Cooldown;
                }
            }

            if (_cooldownText != null)
            {
                _cooldownText.enabled = onCooldown;
                if (onCooldown)
                {
                    _cooldownText.text = cooldownRemaining > 1 
                        ? Mathf.CeilToInt(cooldownRemaining).ToString() 
                        : cooldownRemaining.ToString("F1");
                }
            }
        }

        public void ClearSlot()
        {
            _currentAbility = null;

            if (_iconImage != null)
                _iconImage.enabled = false;

            if (_cooldownOverlay != null)
                _cooldownOverlay.enabled = false;

            if (_cooldownText != null)
                _cooldownText.enabled = false;
        }

        public void SetKeyBind(string keyBind)
        {
            if (_keyBindText != null)
                _keyBindText.text = keyBind;
        }
    }
}
