using System;
using System.Text;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using EtherDomes.Data;
using EtherDomes.Persistence;
using EtherDomes.Core;
using Newtonsoft.Json;

namespace EtherDomes.Network
{
    /// <summary>
    /// NGO Connection Approval Manager.
    /// Hooks into NetworkManager to validate incoming connections and payloads.
    /// Includes mathematical validation of character stats (Sanitizer).
    /// </summary>
    public class ConnectionApprovalManager : MonoBehaviour
    {
        [Header("Validation Settings")]
        [SerializeField] private bool _skipAuthenticationForTesting = false;
        [SerializeField] private bool _skipStatsValidation = false;
        [SerializeField] private Transform _defaultSpawnPoint;

        [Header("Class Definitions (Para Validación)")]
        [SerializeField] private ClassDefinition[] _classDefinitions;
        
        [Header("Item Database (Para Validación)")]
        [SerializeField] private ItemDatabase _itemDatabase;

        private IConnectionApprovalHandler _approvalHandler;
        
        // Cache de datos de jugadores pendientes de sanitización
        private Dictionary<ulong, CharacterData> _pendingSanitization = new Dictionary<ulong, CharacterData>();
        private Dictionary<ulong, CharacterData> _connectedPlayersData = new Dictionary<ulong, CharacterData>();

        private void Start()
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[ConnectionApprovalManager] NetworkManager missing!");
                return;
            }

            _approvalHandler = new ConnectionApprovalHandler();
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            
            Debug.Log("[ConnectionApprovalManager] Hooked into NetworkManager ConnectionApprovalCallback");
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            // Si hay datos pendientes de sanitización, enviar corrección al cliente
            if (_pendingSanitization.TryGetValue(clientId, out var sanitizedData))
            {
                Debug.Log($"[ConnectionApprovalManager] Sending sanitized data to client {clientId}");
                SendSanitizedDataToClient(clientId, sanitizedData);
                _pendingSanitization.Remove(clientId);
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            _connectedPlayersData.Remove(clientId);
            _pendingSanitization.Remove(clientId);
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            try
            {
                Debug.Log($"[ConnectionApprovalManager] Validating connection from ClientID: {request.ClientNetworkId}");

                response.Approved = false;
                response.CreatePlayerObject = false;
                response.PlayerPrefabHash = null;

                // Host siempre se aprueba automáticamente (ClientNetworkId 0 es el host)
                bool isHost = request.ClientNetworkId == NetworkManager.Singleton.LocalClientId && 
                              NetworkManager.Singleton.IsHost;
                
                if (isHost || request.ClientNetworkId == 0)
                {
                    Debug.Log("[ConnectionApprovalManager] Host connection - auto approved");
                    response.Approved = true;
                    
                    // Solo crear jugador si NO es servidor dedicado
                    response.CreatePlayerObject = !ServerPasswordHolder.IsDedicatedServer;
                    
                    if (ServerPasswordHolder.IsDedicatedServer)
                    {
                        Debug.Log("[ConnectionApprovalManager] Dedicated server mode - no player created");
                    }
                    
                    response.Position = (_defaultSpawnPoint != null) ? _defaultSpawnPoint.position : Vector3.zero;
                    response.Rotation = Quaternion.identity;
                    return;
                }

                if (_skipAuthenticationForTesting)
                {
                    Debug.Log("[ConnectionApprovalManager] Skipping auth (Testing Mode)");
                    response.Approved = true;
                    response.CreatePlayerObject = true;
                    response.Position = (_defaultSpawnPoint != null) ? _defaultSpawnPoint.position : Vector3.zero;
                    response.Rotation = Quaternion.identity;
                    return;
                }

                // 1. Validate Payload
                byte[] payloadBytes = request.Payload;
                if (payloadBytes == null || payloadBytes.Length == 0)
                {
                    Debug.LogWarning("[ConnectionApprovalManager] Connection rejected: Empty Payload");
                    response.Reason = "Empty Payload";
                    return;
                }

                // 2. Decode Payload
                string payloadJson = Encoding.UTF8.GetString(payloadBytes);
                ConnectionPayloadMessage payloadMsg;
                
                try 
                {
                    payloadMsg = JsonUtility.FromJson<ConnectionPayloadMessage>(payloadJson);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ConnectionApprovalManager] Payload parsing error: {e.Message}");
                    response.Reason = "Invalid Payload Format";
                    return;
                }

                // 3. Validate Password
                if (!ValidatePassword(payloadMsg.PasswordHash))
                {
                    Debug.LogWarning("[ConnectionApprovalManager] Connection rejected: Invalid Password");
                    response.Reason = "InvalidPassword";
                    return;
                }

                // 4. Validate and Sanitize Character Data
                CharacterData characterData = null;
                bool needsSanitization = false;
                
                if (!string.IsNullOrEmpty(payloadMsg.CharacterDataJson))
                {
                    try
                    {
                        characterData = JsonConvert.DeserializeObject<CharacterData>(payloadMsg.CharacterDataJson);
                        
                        if (characterData != null && !_skipStatsValidation)
                        {
                            var validationResult = ValidateAndSanitizeCharacter(characterData);
                            needsSanitization = validationResult.WasSanitized;
                            characterData = validationResult.SanitizedData;
                            
                            if (validationResult.WasSanitized)
                            {
                                Debug.LogWarning($"[ConnectionApprovalManager] Character data was sanitized for client {request.ClientNetworkId}");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[ConnectionApprovalManager] CharacterData parsing error: {e.Message}");
                    }
                }

                // 5. Approve
                response.Approved = true;
                response.CreatePlayerObject = true;
                
                // 6. Set Spawn Position from Payload
                Vector3 spawnPos = Vector3.zero;
                if (payloadMsg.HasSavedPosition)
                {
                    spawnPos = payloadMsg.LastPosition;
                }
                else
                {
                    spawnPos = (_defaultSpawnPoint != null) ? _defaultSpawnPoint.position : Vector3.zero;
                }

                response.Position = spawnPos;
                response.Rotation = Quaternion.identity;

                Debug.Log($"[ConnectionApprovalManager] Approved {request.ClientNetworkId}. Class: {payloadMsg.ClassID}, Spawn: {spawnPos}");
                
                // Store player data and mark for sanitization if needed
                if (characterData != null)
                {
                    _connectedPlayersData[request.ClientNetworkId] = characterData;
                    
                    if (needsSanitization)
                    {
                        _pendingSanitization[request.ClientNetworkId] = characterData;
                    }
                }
                
                StorePlayerData(request.ClientNetworkId, payloadMsg);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ConnectionApprovalManager] CRASH: {e.Message}\n{e.StackTrace}");
                response.Approved = false;
                response.Reason = "ServerInternalError";
            }
        }

        /// <summary>
        /// Valida la contraseña del cliente contra la del servidor
        /// </summary>
        private bool ValidatePassword(string clientPasswordHash)
        {
            // Obtener la contraseña del servidor desde ServerPasswordHolder
            string serverPasswordHash = ServerPasswordHolder.CurrentPasswordHash;
            
            // Si el servidor no tiene contraseña, aceptar cualquier cliente
            if (string.IsNullOrEmpty(serverPasswordHash))
            {
                Debug.Log("[ConnectionApprovalManager] Server has no password, accepting client");
                return true;
            }
            
            // Si el servidor tiene contraseña, el cliente debe enviar la correcta
            if (string.IsNullOrEmpty(clientPasswordHash))
            {
                Debug.LogWarning("[ConnectionApprovalManager] Server requires password but client sent none");
                return false;
            }
            
            bool isValid = serverPasswordHash == clientPasswordHash;
            if (!isValid)
            {
                Debug.LogWarning("[ConnectionApprovalManager] Password mismatch");
            }
            else
            {
                Debug.Log("[ConnectionApprovalManager] Password validated successfully");
            }
            return isValid;
        }

        private Vector3 GetSpawnPosition(ConnectionPayloadMessage? msg)
        {
            if (msg.HasValue && msg.Value.HasSavedPosition)
            {
                return msg.Value.LastPosition;
            }
            return _defaultSpawnPoint != null ? _defaultSpawnPoint.position : Vector3.zero;
        }

        private void StorePlayerData(ulong clientId, ConnectionPayloadMessage startData)
        {
            Debug.Log($"[ConnectionApprovalManager] Storing data for Client {clientId}: Class {startData.ClassID}");
        }

        #region Character Validation & Sanitization

        /// <summary>
        /// Resultado de la validación de personaje.
        /// </summary>
        public struct ValidationResult
        {
            public bool WasSanitized;
            public CharacterData SanitizedData;
            public List<string> Discrepancies;
        }

        /// <summary>
        /// Valida y sanitiza los datos del personaje comparando con los valores esperados.
        /// El Host recalcula lo que el jugador debería tener basándose en ClassDefinition e ItemDatabase.
        /// </summary>
        private ValidationResult ValidateAndSanitizeCharacter(CharacterData clientData)
        {
            var result = new ValidationResult
            {
                WasSanitized = false,
                SanitizedData = clientData,
                Discrepancies = new List<string>()
            };

            if (clientData == null)
                return result;

            // Obtener ClassDefinition para esta clase
            var classDef = GetClassDefinition((CharacterClass)clientData.ClassID);
            if (classDef == null)
            {
                Debug.LogWarning($"[ConnectionApprovalManager] ClassDefinition not found for ClassID {clientData.ClassID}");
                return result;
            }

            // Recalcular stats esperados
            var expectedStats = classDef.GetStatsForLevel(clientData.Level);
            
            // Calcular bonuses de items equipados
            int itemStr = 0, itemAgi = 0, itemInt = 0, itemSta = 0, itemArmor = 0, itemAP = 0, itemSP = 0;
            
            if (_itemDatabase != null && clientData.EquippedItemIDs != null)
            {
                foreach (var itemId in clientData.EquippedItemIDs)
                {
                    var item = _itemDatabase.GetItem(itemId);
                    if (item?.Stats != null)
                    {
                        // Validar que la clase puede usar este item
                        if (!CanClassUseItem(classDef, item))
                        {
                            result.Discrepancies.Add($"Item {itemId} not allowed for class {classDef.ClassName}");
                            continue; // No sumar stats de items no permitidos
                        }
                        
                        itemStr += item.Stats.Strength;
                        itemAgi += item.Stats.Agility;
                        itemInt += item.Stats.Intellect;
                        itemSta += item.Stats.Stamina;
                        itemArmor += item.Stats.Armor;
                        itemAP += item.Stats.AttackPower;
                        itemSP += item.Stats.SpellPower;
                    }
                }
            }

            // Stats esperados totales
            int expectedStr = expectedStats.Strength + itemStr;
            int expectedAgi = expectedStats.Agility + itemAgi;
            int expectedInt = expectedStats.Intellect + itemInt;
            int expectedSta = expectedStats.Stamina + itemSta;
            int expectedArmor = expectedStats.BaseArmor + itemArmor;
            int expectedMaxHP = expectedStats.MaxHealth + (expectedSta * 10);
            int expectedMaxMana = expectedStats.MaxMana + (expectedInt * 5);

            // Comparar y sanitizar
            if (clientData.TotalStrength != expectedStr)
            {
                result.Discrepancies.Add($"Strength: client={clientData.TotalStrength}, expected={expectedStr}");
                clientData.TotalStrength = expectedStr;
                result.WasSanitized = true;
            }

            if (clientData.TotalAgility != expectedAgi)
            {
                result.Discrepancies.Add($"Agility: client={clientData.TotalAgility}, expected={expectedAgi}");
                clientData.TotalAgility = expectedAgi;
                result.WasSanitized = true;
            }

            if (clientData.TotalIntellect != expectedInt)
            {
                result.Discrepancies.Add($"Intellect: client={clientData.TotalIntellect}, expected={expectedInt}");
                clientData.TotalIntellect = expectedInt;
                result.WasSanitized = true;
            }

            if (clientData.TotalStamina != expectedSta)
            {
                result.Discrepancies.Add($"Stamina: client={clientData.TotalStamina}, expected={expectedSta}");
                clientData.TotalStamina = expectedSta;
                result.WasSanitized = true;
            }

            if (clientData.TotalArmor != expectedArmor)
            {
                result.Discrepancies.Add($"Armor: client={clientData.TotalArmor}, expected={expectedArmor}");
                clientData.TotalArmor = expectedArmor;
                result.WasSanitized = true;
            }

            if (clientData.MaxHP != expectedMaxHP)
            {
                result.Discrepancies.Add($"MaxHP: client={clientData.MaxHP}, expected={expectedMaxHP}");
                clientData.MaxHP = expectedMaxHP;
                result.WasSanitized = true;
            }

            if (clientData.MaxMana != expectedMaxMana)
            {
                result.Discrepancies.Add($"MaxMana: client={clientData.MaxMana}, expected={expectedMaxMana}");
                clientData.MaxMana = expectedMaxMana;
                result.WasSanitized = true;
            }

            // Validar que CurrentHP/Mana no excedan máximos
            if (clientData.CurrentHP > clientData.MaxHP)
            {
                result.Discrepancies.Add($"CurrentHP exceeds MaxHP: {clientData.CurrentHP} > {clientData.MaxHP}");
                clientData.CurrentHP = clientData.MaxHP;
                result.WasSanitized = true;
            }

            if (clientData.CurrentMana > clientData.MaxMana)
            {
                result.Discrepancies.Add($"CurrentMana exceeds MaxMana: {clientData.CurrentMana} > {clientData.MaxMana}");
                clientData.CurrentMana = clientData.MaxMana;
                result.WasSanitized = true;
            }

            // Validar nivel (no puede ser negativo o excesivo)
            if (clientData.Level < 1 || clientData.Level > 100)
            {
                result.Discrepancies.Add($"Invalid level: {clientData.Level}");
                clientData.Level = Mathf.Clamp(clientData.Level, 1, 100);
                result.WasSanitized = true;
            }

            // Log discrepancias
            if (result.Discrepancies.Count > 0)
            {
                Debug.LogWarning($"[ConnectionApprovalManager] Validation found {result.Discrepancies.Count} discrepancies:");
                foreach (var d in result.Discrepancies)
                {
                    Debug.LogWarning($"  - {d}");
                }
            }

            result.SanitizedData = clientData;
            return result;
        }

        /// <summary>
        /// Verifica si una clase puede usar un item específico.
        /// </summary>
        private bool CanClassUseItem(ClassDefinition classDef, ItemData item)
        {
            if (classDef == null || item == null)
                return false;

            // Verificar tipo de armadura
            if (item.ArmorType != ArmorType.None && item.ArmorType != ArmorType.Universal)
            {
                if (!classDef.CanUseArmor(item.ArmorType))
                    return false;
            }

            // Verificar tipo de arma
            if (item.WeaponType != WeaponType.None)
            {
                if (!classDef.CanUseWeapon(item.WeaponType))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Obtiene la ClassDefinition para una clase específica.
        /// </summary>
        private ClassDefinition GetClassDefinition(CharacterClass characterClass)
        {
            if (_classDefinitions == null || _classDefinitions.Length == 0)
            {
                Debug.LogWarning("[ConnectionApprovalManager] No ClassDefinitions assigned!");
                return null;
            }

            foreach (var def in _classDefinitions)
            {
                if (def != null && def.ClassType == characterClass)
                    return def;
            }

            return null;
        }

        /// <summary>
        /// Envía los datos sanitizados al cliente para que actualice su guardado local.
        /// </summary>
        private void SendSanitizedDataToClient(ulong clientId, CharacterData sanitizedData)
        {
            // TODO: Implementar ClientRPC para enviar datos corregidos
            // Por ahora solo logueamos
            Debug.Log($"[ConnectionApprovalManager] Would send sanitized data to client {clientId}");
            
            // En una implementación completa:
            // 1. Serializar sanitizedData a JSON
            // 2. Enviar via ClientRPC al cliente específico
            // 3. El cliente recibe y sobrescribe su guardado local
        }

        /// <summary>
        /// Obtiene los datos del personaje de un cliente conectado.
        /// </summary>
        public CharacterData GetConnectedPlayerData(ulong clientId)
        {
            _connectedPlayersData.TryGetValue(clientId, out var data);
            return data;
        }

        /// <summary>
        /// Obtiene todos los datos de jugadores conectados.
        /// </summary>
        public IReadOnlyDictionary<ulong, CharacterData> GetAllConnectedPlayersData()
        {
            return _connectedPlayersData;
        }

        #endregion
    }
}
