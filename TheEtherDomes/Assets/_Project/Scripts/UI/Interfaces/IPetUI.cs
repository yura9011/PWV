using System;
using EtherDomes.Data;

namespace EtherDomes.UI
{
    /// <summary>
    /// Interface for the pet UI component.
    /// Displays pet health bar near the player frame.
    /// Requirements: 3.8
    /// </summary>
    public interface IPetUI
    {
        /// <summary>
        /// Initialize the pet UI with the owner's entity ID.
        /// </summary>
        /// <param name="ownerId">Entity ID of the pet owner</param>
        void Initialize(ulong ownerId);

        /// <summary>
        /// Update the pet health display.
        /// </summary>
        /// <param name="currentHealth">Current pet health</param>
        /// <param name="maxHealth">Maximum pet health</param>
        void UpdateHealth(float currentHealth, float maxHealth);

        /// <summary>
        /// Update the pet name display.
        /// </summary>
        /// <param name="petName">Name of the pet</param>
        void UpdatePetName(string petName);

        /// <summary>
        /// Update the pet state display.
        /// </summary>
        /// <param name="state">Current pet state</param>
        void UpdatePetState(PetState state);

        /// <summary>
        /// Show the pet UI (when pet is summoned).
        /// </summary>
        void Show();

        /// <summary>
        /// Hide the pet UI (when pet is dismissed or dead).
        /// </summary>
        void Hide();

        /// <summary>
        /// Check if the pet UI is currently visible.
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Event fired when the pet frame is clicked.
        /// </summary>
        event Action OnPetFrameClicked;

        /// <summary>
        /// Event fired when the attack command button is clicked.
        /// </summary>
        event Action OnAttackCommandClicked;

        /// <summary>
        /// Event fired when the follow command button is clicked.
        /// </summary>
        event Action OnFollowCommandClicked;
    }
}
