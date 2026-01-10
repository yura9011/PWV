# Project Board Tasks

Generated from Trello CSV export.

## Documento de Especificación Técnica - Fase 10: Lanzamiento, Operaciones y Steam V1.0 Enfoque: Estabilidad de Red, Integración de Plataforma y Mantenibilidad

- [ ] **1. Visión de la Fase**
    > El objetivo es preparar el ejecutable para el consumo público. Se implementarán barreras de entrada (Control de Versiones) para evitar incompatibilidades, se integrará la API de Steam para persistencia y logros, y se optimizará agresivamente el tráfico de red mediante "Interest Management" para que el juego sea jugable incluso con conexiones modestas.

- [ ] **2. Integración con Steamworks**
    > #### 2.1 Autenticación e Identidad
    > Reemplazamos el sistema de IDs de desarrollo por SteamID.
    > - **Inicialización:** Al arrancar, `SteamAPI.Init()`. Si falla, cerrar el juego (DRM básico).
    > - **Save File:** El nombre del archivo de guardado incluirá el SteamID64 (`save_76561198000000000.ted`).
    > - **Cloud Sync:**
    >   - Configuración en Steamworks Dashboard: "Auto-Cloud" apuntando a `Application.persistentDataPath`.
    >   - **Resolución de Conflictos:** Delegamos en la lógica nativa de Steam (Timestamp Check). Si el archivo local es más viejo que el de la nube, Steam lo descarga antes de que el juego arranque.
    > #### 2.2 Logros (Achievements)
    > - **Manager:** `AchievementManager` (Singleton).
    > - **Lógica:** Escucha eventos del juego.
    >   - `OnBossDeath(BossID)` -> `SteamUserStats.SetAchievement("KILL_" + BossID)`.
    >   - `OnLevelUp(100)` -> `SteamUserStats.SetAchievement("MAX_LEVEL")`.
    > - **Sincronización:** Llamar a `SteamUserStats.StoreStats()` al guardar la partida o desbloquear un logro.

- [ ] **3. Optimización de Red (Netcode Tuning)**
    > #### 3.1 Tick Rate (Opción C2)
    > Reducimos la frecuencia de actualización para ahorrar CPU y Ancho de Banda.
    > - **Configuración NGO:** `NetworkTickSystem.TickRate = 30` (30 veces por segundo).
    > - **Interpolación:** El componente `ClientNetworkTransform` interpolará visualmente entre esos 30 ticks para que el movimiento se vea suave a 60/144 FPS.
    > #### 3.2 Interest Management (Opción C1 - Spatial Hashing)
    > El Host no enviará datos de entidades lejanas.
    > - **Implementación:** Usaremos el delegado `CheckObjectVisibility` de Netcode for GameObjects.
    > - **Lógica de Visibilidad:**
    >   1. **Check de Escena:** Si el Jugador A está en "Mazmorra" y el Enemigo B está en "Bosque" -> **Oculto**.
    >   2. **Check de Distancia:** Si están en la misma escena, calcular `Vector3.Distance`.
    >      - Si Distancia > 80m -> **Oculto**.
    >      - Si Distancia <= 80m -> **Visible**.
    > - **Frecuencia:** Este chequeo se ejecuta cada 1.0 segundos (no en cada frame) para ahorrar CPU.

- [ ] **4. Control de Versiones (Handshake)**
    > #### 4.1 Validación Estricta (Opción A1)
    > - **Build Settings:** Definir `PlayerSettings.bundleVersion` (ej. "1.0.2").
    > - **Connection Payload:** Añadir campo `string Version`.
    > - **Lógica de Aprobación (Server-Side):**
    >   codeC#
    >   ```
    >   if (connectionData.Version != Application.version) {
    >       response.Approved = false;
    >       response.Reason = $"Versión Incorrecta. Host: {Application.version}, Tú: {connectionData.Version}";
    >   }
    >   ```

- [ ] **5. Arquitectura de Datos y Parches**
    > #### 5.1 Balanceo Retroactivo (Opción D1)
    > - **Principio:** El archivo de guardado (`.ted`) es "ligero".
    > - **Estructura JSON:**
    >   codeJSON
    >   ```
    >   "Inventory": [
    >       { "ItemID": 105, "Quantity": 1 }, // Espada de Fuego
    >       { "ItemID": 202, "Quantity": 5 }  // Pociones
    >   ]
    >   ```
    > - **Carga:** El juego lee ID 105 -> Busca en `ItemDatabaseSO` -> Lee `Damage: 30`.
    > - **Actualización:** Si lanzas el Parche 1.1 y cambias el daño de la Espada (ID 105) a 25 en el ScriptableObject, todos los jugadores tendrán automáticamente 25 de daño al cargar, sin necesidad de migrar sus archivos de guardado.

- [ ] **6. Roadmap de Implementación - Fase 10**
    > #### Semana 33: Optimización de Red
    > 1. **Tick Rate:** Ajustar configuración y probar suavidad de movimiento.
    > 2. **Visibility Check:** Implementar el script que oculta objetos lejanos. Verificar con el Profiler de Unity que el ancho de banda baja drásticamente al alejarse.
    > #### Semana 34: Steamworks SDK
    > 1. **Import:** Instalar paquete `Steamworks.NET`.
    > 2. **Auth:** Reemplazar el login genérico por `SteamUser.GetSteamID()`.
    > 3. **Achievements:** Conectar los eventos de muerte de jefes a la API.
    > #### Semana 35: Beta Cerrada (Bug Fixing)
    > 1. **Freeze:** No se añade ninguna feature nueva.
    > 2. **Stress Test:** Conectar 10 clientes (simulados o amigos) y saturar el combate para ver si el Host aguanta.
    > 3. **Version Check:** Probar a conectar un cliente viejo a un host nuevo para verificar el rechazo.
    > #### Semana 36: Release Candidate
    > 1. **Build Final:** Compilación en modo "Release" (sin logs de depuración).
    > 2. **Upload:** Subida a Steamworks pipe.
    > ---
    > ### ¡ROADMAP COMPLETO FINALIZADO!
    > Hemos cubierto el ciclo de vida entero de **"THE ETHER DOMES"**.
    > Tienes en tus manos (y en tu historial de chat) la documentación completa para:
    > 1. **Core Loop:** Combate Tab-Targeting y Movimiento.
    > 2. **Meta-Game:** Progresión, Loot y Economía.
    > 3. **Social:** Party, Chat y Roles (Trinidad).
    > 4. **Tech:** Persistencia segura, Netcode optimizado y Steam.


## Documento de Especificación Técnica - Fase 1: Core de Red y Locomoción V1.0 Motor: Unity 6 | Stack: Netcode for GameObjects (NGO)

- [x] **1. Visión de la Fase** ✅ COMPLETADA (2026-01-08)
    > El objetivo es establecer la arquitectura de red y el "Game Feel" del movimiento. Al finalizar, dos o más jugadores podrán conectarse vía internet, elegir una visualización básica (Clase A/B), aparecer en el mundo en su última posición conocida y moverse con controles estilo MMORPG clásico, viéndose mutuamente con fluidez.

- [x] **2. Arquitectura de Red (Netcode)** ✅ COMPLETADA
    > #### 2.1 Configuración General
    > - **Topología:** Host-Server (Un jugador es Servidor + Cliente, los demás son Clientes).
    > - **Transporte:** Unity Transport (UTP).
    > - **Servicios:** Unity Relay (para conexión remota sin puertos) + Unity Lobby (para descubrimiento de partidas).
    > #### 2.2 Autoridad y Sincronización
    > - **Transform Authority:** `ClientNetworkTransform` - IMPLEMENTADO.
    >   - _Decisión:_ El cliente tiene autoridad sobre su posición. Esto elimina el lag de input (movimiento instantáneo al pulsar tecla).
    > - **Interpolación:** Agresiva / Alta.
    >   - _Configuración:_ En el componente `NetworkTransform`, habilitar interpolación. Si hay pérdida de paquetes, el personaje remoto se deslizará suavemente hacia su nueva posición en lugar de teletransportarse, priorizando la estética sobre la precisión milimétrica.
    > #### 2.3 Flujo de Conexión (Payload)
    > Para cumplir con el requisito de "Spawnear donde se desconectó" (Opción C), utilizaremos el **Connection Payload**.
    > 1. El Cliente lee su posición guardada localmente (PlayerPrefs temporalmente).
    > 2. Al solicitar conexión, envía un `byte[] payload` conteniendo: `Vector3 LastPosition` y `int ClassSelectionID`.
    > 3. El Servidor lee el payload y mueve al jugador a esa posición inmediatamente después de instanciarlo.
    > ---

- [x] **3. Controlador de Personaje (Player Controller)** ✅ COMPLETADA - Controles WoW
    > #### 3.1 Inputs (Legacy Input - Pendiente migración)
    > - **Ejes de Movimiento:** `Vector2` (W/S para Z, A/D para X).
    > - **Botones:**
    >   - `Mouse Right (Hold)`: Modo Combate/Cámara bloqueada.
    >   - `Mouse Left`: Rotación de cámara independiente.
    >   - `Q/E`: Strafe siempre.
    > #### 3.2 Lógica de Movimiento "Híbrido WoW" - IMPLEMENTADO
    > El script de movimiento se comporta diferente según el estado del mouse:
    > - **Estado A: Mouse Libre (Clic Derecho NO presionado)**
    >   - **W / S:** Mueve al personaje adelante/atrás respecto a la cámara.
    >   - **A / D:** **Rota** al personaje sobre su eje Y (Giro).
    >   - **Cursor:** Visible y desbloqueado.
    > - **Estado B: Mouse Bloqueado (Clic Derecho MANTENIDO)**
    >   - **Cursor:** Oculto y bloqueado en el centro.
    >   - **Cámara:** La rotación del mouse rota al personaje.
    >   - **A / D:** Se convierten en **Strafe** (paso lateral izquierda/derecha).
    >   - **Q / E:** Strafe directo siempre.

- [x] **4. Sistema de Cámara (ThirdPersonCameraController)** ✅ COMPLETADA
    > - **Componente:** `ThirdPersonCameraController` (Custom).
    > - **Configuración:**
    >   - Zoom con rueda del ratón (0-16m)
    >   - Colisión con suelo y obstáculos
    >   - Click izquierdo: Solo rota cámara
    >   - Click derecho: Rota cámara Y jugador
    > ---

- [x] **5. Flujo de Usuario (UX) - Fase 1** ✅ COMPLETADA
    > #### 5.1 Menú Principal & Selección
    > 1. **UI Inicial:** Flujo multi-panel (Main → Mode → Create/Join → Class).
    > 2. **Selector de Clase (Visual):**
    >    - Dos botones: "Guerrero (Rojo)" / "Mago (Azul)".
    >    - Guarda la selección en `ClassSelectionData`.
    > 3. **Relay:** Código de sala mostrado en UI durante partida.
    > 4. **Background:** Wallpaper en menú principal (Coralwallpaper.png).
    > #### 5.2 Menú de Pausa
    > - ESC abre/cierra menú de pausa
    > - Botón SALIR desconecta y cierra aplicación

- [x] **6. Estructura de Scripts** ✅ COMPLETADA
    > Scripts implementados:
    > 1. `Scripts/Network`
    >    - `RelayManager.cs`: Maneja la creación y unión a Relay.
    >    - `NetworkSessionManager.cs`: Gestión de sesiones.
    >    - `ClientNetworkTransform.cs`: Autoridad de cliente.
    > 2. `Scripts/Player`
    >    - `PlayerController.cs`: Movimiento estilo WoW completo.
    > 3. `Scripts/Camera`
    >    - `ThirdPersonCameraController.cs`: Cámara con zoom y colisiones.
    > 4. `Scripts/UI`
    >    - `MainMenuController.cs`: Menú principal con wallpaper.
    >    - `PauseMenuController.cs`: Menú de pausa.
    >    - `GameSessionUI.cs`: UI de código Relay.


## Documento de Especificación Técnica - Fase 2: Persistencia, Seguridad y Datos V1.0 Enfoque: Seguridad Estricta & Cross-World Universal

- [ ] **1. Visión de la Fase**
    > El objetivo es implementar un sistema de persistencia local seguro. Los jugadores podrán crear hasta 12 personajes, viajar entre mundos (Hosts) llevando todo su inventario y estado actual (vida/buffs). La integridad de estos datos será garantizada por una validación matemática estricta en el servidor (Host) y una clave de encriptación entregada remotamente.

- [ ] **2. Estructura de Datos (Data Model)**
    > Para soportar 12 slots y validación estricta, separaremos los datos estáticos (Configuración) de los dinámicos (Guardado).
    > #### 2.1 Datos Estáticos (ScriptableObjects - "La Verdad Inmutable")
    > Estos datos residen en el build del juego y el Host los usa para validar.
    > - `ClassDefinition`**:** Define los stats base y coeficientes de crecimiento.
    >   - `BaseHealth`, `BaseMana`, `BaseStr`, `BaseInt`.
    >   - `StrPerLevel`, `IntPerLevel`.
    > - `ItemDatabase`**:** Diccionario maestro de todos los items (`ItemID` -> `ItemStats`).
    > #### 2.2 Datos Dinámicos (Clases Serializables)
    > Esta es la estructura que se convertirá a JSON.
    > codeC#
    > ```
    > [Serializable]
    > public class SaveFile
    > {
    >     public string AccountID;
    >     public List<CharacterData> Characters = new List<CharacterData>(12); // Los 12 Slots
    > }
    > [Serializable]
    > public class CharacterData
    > {
    >     public string Name;
    >     public int ClassID; // Referencia a ClassDefinition
    >     public int Level;
    >     public float CurrentXP;
    >     // Estado Snapshot (Opción A)
    >     public float CurrentHP;
    >     public float CurrentMana;
    >     public List<ActiveStatusEffect> StatusEffects; // Venenos, Buffs persistentes
    >     // Inventario Universal (Opción A)
    >     // Incluye llaves de misión y equipo
    >     public List<InventorySlot> Inventory;
    >     public List<int> EquippedItemIDs; // Referencia a ItemDatabase
    > }
    > ```

- [ ] **3. Seguridad y Encriptación (AES + Key Server)**
    > Has elegido la **Opción B (Server Handshake)**. Esto implica una arquitectura de "Inicio de Sesión".
    > #### 3.1 Flujo de Autenticación
    > 1. **Inicio:** El jugador abre el juego.
    > 2. **Request:** El cliente envía una solicitud HTTPS a tu API de Autenticación (puede ser un simple Azure Function, AWS Lambda o servidor web básico).
    > 3. **Response:** Si es válido, la API devuelve la **Clave AES (Session Key)** en memoria RAM.
    >    - _Nota:_ La clave **nunca** se escribe en el disco duro del jugador. Solo vive en la RAM mientras el juego está abierto.
    > 4. **Descifrado:** El `SaveManager` usa esa clave en RAM para leer/escribir el archivo `.ted` (The Ether Domes file).
    > #### 3.2 Implementación AES
    > - **Algoritmo:** AES-256 en modo CBC con PKCS7 Padding.
    > - **Vector de Inicialización (IV):** Se genera aleatoriamente en cada guardado y se pre-pende al inicio del archivo cifrado.

- [ ] **4. Validación y Sanitización (Host Authority)**
    > El Host actúa como "Aduana". No confía en los datos que envía el Cliente.
    > #### 4.1 Proceso de Conexión (Connection Approval)
    > 1. El Cliente envía su `CharacterData` (JSON plano) dentro del payload de conexión de Netcode.
    > 2. El Host recibe los datos y ejecuta el **Integrity Check**.
    > #### 4.2 Algoritmo de Validación Estricta
    > El Host recalcula lo que el jugador _debería_ tener:
    > codeC#
    > ```
    > // Pseudo-código de lógica en el Host
    > int expectedStr = ClassDef.BaseStr + (ClassDef.StrPerLevel * clientData.Level);
    > foreach (int itemId in clientData.EquippedItemIDs) {
    >     expectedStr += ItemDatabase.Get(itemId).BonusStr;
    > }
    > if (clientData.TotalStr != expectedStr) {
    >     // DETECCIÓN DE TRAMPA O CORRUPCIÓN
    >     // Acción: Sanitización Forzada (Opción B)
    >     clientData.TotalStr = expectedStr; // Corregimos el valor
    >     MarkForSaveUpdate(clientId); // Avisamos que hay que sobrescribir el save del cliente
    > }
    > ```
    > #### 4.3 Sanitización
    > Si hubo discrepancia, una vez que el jugador entra al mundo, el Host envía un `ClientRPC` inmediato: "Actualiza tus datos locales con estos valores corregidos y guarda en disco ahora mismo".

- [ ] **5. Lógica Cross-World y Snapshot**
    > #### 5.1 Persistencia de Estado (Snapshot)
    > - Al desconectar, se guarda `CurrentHP`.
    > - Al conectar, en el `OnNetworkSpawn`, el Host aplica ese `CurrentHP`.
    > - _Caso Borde:_ Si `CurrentHP <= 0` al conectar (se desconectó muerto), el sistema debe forzar el estado de "Fantasma" o respawnearlo en el punto de inicio.
    > #### 5.2 Inventario Universal
    > - No hay filtrado de items. Si tienes la "Llave del Rey" del Mundo A, y vas al Mundo B, puedes abrir la puerta del Rey inmediatamente.
    > - _Diseño:_ Esto fomenta el "Carry" (jugadores veteranos ayudando a novatos), lo cual es coherente con un cooperativo.

- [ ] **6. Roadmap de Implementación - Fase 2**
    > #### Semana 3: Sistema de Archivos y Datos
    > 1. **ScriptableObjects:** Crear `CharacterClassSO` y `ItemSO`.
    > 2. **Serialización:** Implementar `SaveManager` que convierta clases a JSON (usando `Newtonsoft.Json`).
    > 3. **UI de Selección:** Crear el menú para visualizar los 12 slots (Vacío / Crear / Jugar).
    > #### Semana 4: Seguridad y Red
    > 1. **Encriptación:** Implementar la envoltura AES en el `SaveManager`.
    >    - _Dev Mode:_ Usar una clave hardcodeada por ahora ("1234567890123456") para probar sin servidor web.
    > 2. **Handshake de Red:** Modificar el `NetworkManager` para enviar el JSON del personaje al conectar.
    > 3. **Script de "Aduana" (Sanitizer):** Escribir la lógica matemática que suma stats y compara.
    > ### ¿Cómo procedemos?
    > Ya tienes la especificación completa para la persistencia.
    > **Sugerencia:**
    > Para no bloquearte configurando un servidor web real para la clave AES ahora mismo, te recomiendo empezar trabajando en **Modo Desarrollo** (Clave local fija) y centrarte en la lógica de **Sanitización Matemática**, que es la parte más compleja del código.
    > ¿Quieres que te genere el esqueleto del script `CharacterData.cs` y el `ValidationService.cs` (la lógica matemática)?


## Documento de Especificación Técnica - Fase 3: Sistema de Combate y Habilidades V1.0 Enfoque: Responsividad (Input Buffering) y Validación de Red

- [ ] **1. Visión de la Fase**
    > El objetivo es implementar un ciclo de combate "Tab-Targeting" profesional. El jugador podrá seleccionar enemigos inteligentemente, encadenar habilidades con fluidez gracias al sistema de cola (Spell Queue) y experimentar mecánicas distintas según su clase (Mago estático vs. Arquero móvil), todo sincronizado vía `Netcode for GameObjects`.

- [ ] **2. Sistema de Selección (Targeting Intelligence)**
    > #### 2.1 Algoritmo "Tab" (Cono de Visión)
    > Al pulsar `Tab`, el sistema no elige al más cercano ciegamente, sino al **más relevante** para la cámara.
    > - **Lógica de Filtro:**
    >   1. `Physics.OverlapSphere`: Obtener todos los enemigos en radio X (ej. 40m).
    >   2. **Culling de Pantalla:** Descartar enemigos que no estén dentro del frustum de la cámara (lo que el jugador no ve).
    >   3. **Prioridad Central:** Calcular el producto punto (Dot Product) entre el vector `Camera.forward` y la dirección al enemigo. Cuanto más cerca del centro de la pantalla (1.0), mayor prioridad.
    >   4. **Line of Sight (LoS):** Raycast rápido para asegurar que no haya paredes en medio.
    > #### 2.2 Cambio Automático (Auto-Switch)
    > - **Evento:** `OnEnemyDeath`.
    > - **Lógica:** Si mi objetivo actual muere:
    >   1. Buscar enemigos en un radio corto (ej. 10m) alrededor del cadáver.
    >   2. Priorizar: Enemigo que ya me esté atacando (Threat Table > 0) > Enemigo más cercano.
    >   3. Si no hay nadie, limpiar target.

- [ ] **3. Arquitectura de Habilidades (ScriptableObjects)**
    > Para soportar el sistema híbrido (Mago Torreta / Arquero Móvil / Off-GCD), la estructura de datos debe ser flexible.
    > #### 3.1 `AbilityDefinitionSO`
    > codeC#
    > ```
    > public class AbilityDefinitionSO : ScriptableObject
    > {
    >     [Header("Identidad")]
    >     public int ID;
    >     public string Name;
    >     public Sprite Icon;
    >     [Header("Costes y Tiempos")]
    >     public float CastTime;       // 0 = Instantáneo
    >     public float Cooldown;
    >     public float Range;
    >     public int ManaCost;
    >     [Header("Comportamiento")]
    >     public bool RequiresStationary; // TRUE: Cancela si mueves. FALSE: Puedes moverte (Arquero).
    >     public bool TriggersGCD;        // TRUE: Activa el CD global. FALSE: Se puede usar siempre (Defensivo).
    >     public bool IsOffensive;        // TRUE: Requiere enemigo. FALSE: Self-cast/Aliado.
    >     [Header("Visual")]
    >     public GameObject ProjectilePrefab; // Si es null, es daño instantáneo (Hitscan)
    >     public float ProjectileSpeed;
    > }
    > ```

- [ ] **4. Lógica de Combate y "Game Feel"**
    > #### 4.1 Máquina de Estados del Jugador (CombatState)
    > - **Idle:** Listo para actuar.
    > - **Casting:** Barra de progreso llenándose.
    >   - Si `RequiresStationary == true` y `Input.Move != 0` -> **Interrumpir**.
    > - **GlobalCooldown (GCD):** Estado breve (ej. 1.5s) donde no se permiten habilidades con `TriggersGCD = true`.
    > - **Locked:** Stunneado/Silenciado.
    > #### 4.2 Sistema de Cola (Spell Queue / Input Buffering)
    > Para mitigar el lag y permitir combos rápidos.
    > - **Ventana de Buffer:** 400ms (Configurable).
    > - **Funcionamiento:** Si el jugador presiona una tecla mientras está en `Casting` o `GCD`, y falta menos de 400ms para terminar, la acción se guarda en `NextAbility`.
    > - **Ejecución:** En el frame exacto en que termina el estado actual, si `NextAbility != null`, se dispara automáticamente el RPC al servidor.

- [ ] **5. Flujo de Red (Networking)**
    > El combate debe ser "Server Authoritative" para el daño, pero "Client Responsive" para la UI.
    > #### 5.1 Secuencia de Disparo (Casting Flow)
    > 1. **Cliente (Input):** Jugador pulsa '1'. Cliente chequea Cooldown local y Maná (Predicción).
    > 2. **Cliente -> Servidor (**`RequestCastServerRPC`**):** Envía `AbilityID` y `TargetNetworkObjectID`.
    > 3. **Servidor (Validación):**
    >    - ¿Tiene maná?
    >    - ¿Está el target vivo y en rango?
    >    - ¿Pasó el Cooldown?
    >    - _Si falla:_ Retorna `ClientRPC` de error ("Fuera de rango").
    > 4. **Servidor (Ejecución):**
    >    - Consume Maná.
    >    - Si `CastTime > 0`: Inicia corrutina de espera.
    >    - Si el jugador se mueve (y la skill es estática): Cancela corrutina.
    > 5. **Servidor -> Clientes (**`BroadcastVisualsClientRPC`**):**
    >    - Todos los clientes reproducen la animación de ataque y muestran la barra de cast (si aplica).
    > #### 5.2 Impacto y Daño
    > 1. **Finalización:** Al terminar el tiempo de cast.
    > 2. **Proyectil (Visual Homing):**
    >    - El servidor instancia un objeto lógico (o calcula tiempo de vuelo).
    >    - Envía RPC para que los clientes spawneen la bola de fuego visual que persigue al target.
    > 3. **Cálculo de Daño:**
    >    - El Servidor calcula: `(BaseDmg * Stats) - Mitigación`.
    >    - Aplica `Target.ReceiveDamage()`.
    >    - Actualiza `NetworkVariable<Health>`.

- [ ] **6. Roadmap de Implementación - Fase 3**
    > #### Semana 5: Cimientos y Selección
    > 1. **Targeting System:** Implementar la lógica de Tab con cono de visión y UI de Target Frame (Vida/Nombre).
    > 2. **Ability Data:** Crear los ScriptableObjects y configurar 3 habilidades de prueba:
    >    - _Bola de Fuego_ (Mago: Cast 2s, Estático, Daño alto).
    >    - _Disparo Rápido_ (Arquero: Instant, Móvil, Daño medio).
    >    - _Escudo de Hielo_ (Defensivo: Instant, Off-GCD, Self-buff).
    > #### Semana 6: Lógica de Casteo Local
    > 1. **Combat Controller:** Implementar la máquina de estados (Casting, GCD).
    > 2. **Input Buffering:** Crear el sistema de cola de 400ms.
    > 3. **Interrupción:** Conectar el `MovementScript` para que avise al `CombatScript` si el jugador camina.
    > #### Semana 7: Networking y Daño
    > 1. **RPCs:** Conectar la petición del cliente con la validación del servidor.
    > 2. **Health System:** Sincronizar la vida de los enemigos y muerte.
    > 3. **Visuales:** Instanciación de proyectiles que siguen al objetivo (`Vector3.MoveTowards`).
    > #### Semana 8: Polish
    > 1. **Auto-Switch:** Implementar lógica al morir un enemigo.
    > 2. **Feedback:** Números de daño flotantes (Floating Combat Text).
    > ### ¿Cómo procedemos?
    > Esta fase es densa en código.
    > **Sugerencia:**
    > Empieza por lo que el jugador "ve". Antes de programar el servidor, asegúrate de que **pulsar TAB seleccione correctamente al enemigo que estás mirando**.


## Documento de Especificación Técnica - Fase 4: IA, Amenaza y la Santa Trinidad V1.0 Enfoque: Estabilidad de Aggro y Roles Definidos

- [ ] **1. Visión de la Fase**
    > El objetivo es transformar los NPCs en enemigos reactivos que respeten la "Santa Trinidad". El Tanque podrá mantener la atención del enemigo mediante multiplicadores de amenaza y provocaciones forzadas. Los DPS deberán controlar su daño para no superar el umbral del 110%/130%. El Sanador tendrá que gestionar su maná y posición, sabiendo que curar genera amenaza dividida entre todos los enemigos activos.

- [ ] **2. Arquitectura de IA (State Machine)**
    > a IA vivirá exclusivamente en el **Servidor** (Server Authoritative). Los clientes solo reciben posición, rotación y animaciones.
    > #### 2.1 Máquina de Estados Finita (FSM)
    > - **Idle / Patrol:**
    >   - El enemigo se mueve entre `Waypoints` usando `NavMeshAgent`.
    >   - Tiene un `DetectionRadius` (ej. 15m). Si un jugador entra -> Transición a **Chase**.
    > - **Chase (Persecución):**
    >   - Se mueve hacia el jugador con mayor amenaza.
    >   - Si llega a rango de ataque (ej. 2m Melee) -> Transición a **Combat**.
    >   - Si el objetivo se aleja más allá del `LeashRange` (ej. 60m del spawn) -> Transición a **Reset**.
    > - **Combat (Combate):**
    >   - El enemigo está estático (o moviéndose lentamente) y rota para mirar al objetivo.
    >   - Ejecuta auto-ataques cada X segundos.
    >   - Chequea la Tabla de Amenaza cada 0.5s para ver si debe cambiar de objetivo.
    > - **Reset (Evade Mode - Decisión C1):**
    >   - **Flags:** `IsInvulnerable = true`, `IsActive = false`.
    >   - **Acción:** `NavMeshAgent.speed` se duplica. Regresa al punto de origen.
    >   - **Llegada:** Al tocar el spawn, `CurrentHP = MaxHP` (Instántaneo), limpia la Tabla de Amenaza y vuelve a **Idle**.
    > ---

- [ ] **3. Sistema de Amenaza (Threat System)**
    > #### 3.1 Estructura de Datos
    > Cada enemigo tiene un componente `AggroController`:
    > codeC#
    > ```
    > // Key: ClientID del Jugador, Value: Puntos de Amenaza
    > private Dictionary<ulong, int> threatTable = new Dictionary<ulong, int>();
    > private ulong currentTargetID;
    > ```
    > #### 3.2 Lógica de "Robar Aggro" (Decisión A1)
    > Para evitar que el jefe cambie de objetivo erráticamente ("Ping-Pong"), se aplica histeresis:
    > - **Caso Melee:** Un jugador a < 5m debe superar la amenaza del objetivo actual en un **110%** (Factor 1.1).
    > - **Caso Ranged:** Un jugador a > 5m debe superar la amenaza actual en un **130%** (Factor 1.3).
    > codeC#
    > ```
    > // Ejemplo de chequeo
    > if (newPlayerThreat > currentTargetThreat * 1.1f) {
    >     SwitchTarget(newPlayerID);
    > }
    > ```
    > #### 3.3 Generación de Amenaza (Decisión A2 y B1)
    > - **Daño:** 1 daño = 1 amenaza (Base).
    > - **Modificadores de Tanque:**
    >   - "Actitud Defensiva": Multiplicador x5 (1 daño = 5 amenaza).
    > - **Curación (Split Threat):**
    >   - Si el Healer cura 100 HP y hay 4 enemigos en combate:
    >   - Amenaza por enemigo = `100 / 4 = 25`.
    >   - _Nota:_ Esto evita que el Healer muera instantáneamente en pulls grandes (AOE).
    > - **Taunt (Hard Fixate):**
    >   - Aplica estado `Fixated` por 3 segundos (La IA ignora la tabla y ataca al Tanque).
    >   - Iguala la amenaza del Tanque a la del líder actual + 10%

- [ ] **4. Habilidades de Clase (Prototipo Trinidad)**
    > Para probar esto, necesitamos 3 habilidades arquetípicas:
    > #### 4.1 Guerrero (Tanque)
    > - **Habilidad:** _Golpe de Escudo_.
    > - **Efecto:** Daño bajo, pero Generación de Amenaza Alta (Bonus +500 threat plano).
    > - **Taunt:** _Grito Desafiante_ (Lógica descrita en 3.3).
    > #### 4.2 Mago (DPS)
    > - **Habilidad:** _Bola de Fuego_.
    > - **Efecto:** Daño alto. Generación de amenaza 1:1.
    > - **Riesgo:** Si critea mucho al inicio, puede superar el 130% y morir.
    > #### 4.3 Clérigo (Healer) - (Decisión D1)
    > - **Habilidad:** _Destello de Luz_.
    > - **Targeting:**
    >   - Si objetivo es Aliado -> Cura al Aliado.
    >   - Si objetivo es Enemigo -> Error / Nada (Opcional).
    >   - Si **no hay objetivo** o objetivo inválido -> **Auto-Self Cast** (Se cura a sí mismo).

- [ ] **5. Sincronización de Red (Networking)**
    > #### 5.1 ¿Qué viaja por la red?
    > - **Posición/Rotación:** `NetworkTransform` en el enemigo.
    > - **Animaciones:** `NetworkAnimator` (Attack, Run, Idle, Death).
    > - **Target Actual:** `NetworkVariable<ulong> CurrentTargetID`.
    >   - _Uso:_ Los clientes necesitan saber a quién mira el jefe para dibujar una línea roja o poner un icono de "Ojo" sobre la cabeza del jugador perseguido.
    > #### 5.2 ¿Qué NO viaja?
    > - **Tabla de Amenaza Completa:** Es demasiada información. Solo el servidor sabe los números exactos.
    > - _(Opcional Future Feature):_ Se puede enviar el % de amenaza del jugador local para mostrar un "Omen" (Medidor de Aggro) personal.

- [ ] **6. Roadmap de Implementación - Fase 4**
    > #### Semana 9: Cerebro Básico (NavMesh)
    > 1. **Setup:** Configurar `NavMeshSurface` en el terreno.
    > 2. **Script IA:** Crear `EnemyAI.cs`. Implementar detección de rango y movimiento hacia el jugador más cercano (sin aggro complejo aún).
    > 3. **Reset:** Implementar la lógica de "volver a casa" si se aleja mucho.
    > #### Semana 10: El Controlador de Aggro
    > 1. **Math:** Programar el `AggroController.cs` (Diccionario, Multiplicadores, Regla del 110%).
    > 2. **Integración:** Conectar el sistema de Daño (Fase 3) para que al recibir daño, se sume amenaza.
    > 3. **Taunt:** Programar el efecto de `Fixate` que sobreescribe temporalmente la IA.
    > #### Semana 11: Pruebas de Estrés (La Trinidad)
    > 1. **Test 1 (Ping-Pong):** Dos jugadores DPS pegando alternadamente para ver si el jefe se marea o respeta el 130%.
    > 2. **Test 2 (Tanking):** Un Tanque haciendo poco daño pero mucha amenaza vs un DPS haciendo mucho daño.
    > 3. **Test 3 (Healing):** Un Healer curando masivamente para ver si los enemigos se le tiran encima (debería ser difícil gracias al Split Threat).
    > ---
    > ### ¿Cómo procedemos?
    > Con la Fase 4 definida, tenemos el ciclo completo de un RPG.
    > **Sugerencia:**
    > La IA en multijugador es tramposa. Si no tienes cuidado, el enemigo se verá "deslizando" o lagueado.


## Documento de Especificación Técnica - Fase 5: Estructura del Mundo, Loot y Economía V1.0 Multiverso Local (Escenas Aditivas) y Gestión de Inventario

- [ ] **1. Visión de la Fase**
    > El objetivo es conectar los sistemas de combate y movimiento en un flujo de juego coherente. Implementaremos un sistema de gestión de escenas avanzado que permite al Host simular múltiples biomas simultáneamente ("Mundo Dividido"). Se establecerá la economía de objetos mediante reglas de Loot configurables (Personal vs Need/Greed) y un sistema de comercio seguro entre jugadores.

- [ ] **2. Gestión de Escenas (Arquitectura "Mundo Dividido")**
    > Dado que el Host procesa todo, pero los clientes pueden estar en lugares distintos, abandonamos la carga de escena simple (`LoadSceneMode.Single`) por la carga aditiva.
    > #### 2.1 NetworkSceneManager
    > - **Host Logic:**
    >   - El Host mantiene cargada la escena "Mundo Abierto" siempre.
    >   - Cuando un jugador entra a una Mazmorra, el Host carga esa escena de forma **Aditiva** (`LoadSceneMode.Additive`).
    >   - _Optimización:_ El Host desactiva Cámaras, Luces y MeshRenderers de la escena de la Mazmorra en su propia pantalla (si él no está ahí), pero mantiene activas las Físicas y la IA para que el Cliente remoto pueda jugar.
    > - **Client Logic:**
    >   - El Cliente recibe la instrucción de cargar _solo_ la escena donde está su personaje.
    >   - Descarga las otras escenas para ahorrar memoria.
    > #### 2.2 Portales y Transiciones
    > - **Trigger de Entrada:**
    >   - Verifica requisitos (Nivel/Quest). Si falla -> Empuje físico hacia atrás + Mensaje UI (Opción E1).
    >   - Si cumple -> Envía `RequestChangeSceneServerRPC`.
    > - **Puntos de Spawn:**
    >   - Cada escena tiene una lista de `NetworkObject` llamados `SpawnPoint` (ID 0: Inicio, ID 1: Entrada Boss, etc.).

- [ ] **3. Persistencia de Instancia (Dungeon State)**
    > ara cumplir con la Opción D1 (Persistencia Temporal), el servidor no puede olvidar el estado de la mazmorra al vaciarse.
    > #### 3.1 DungeonStateManager
    > Diccionario en el Servidor: `Dictionary<string, InstanceState>` (Key: NombreMazmorra).
    > codeC#
    > ```
    > public class InstanceState
    > {
    >     public bool[] BossesDead; // [true, false, false]
    >     public List<string> OpenedChests;
    >     public float LastPlayerExitTime; // Para el Timer de Reset
    > }
    > ```
    > #### 3.2 Ciclo de Vida
    > 1. **Entrada:** Al entrar, el Host chequea si existe un `InstanceState` activo. Si existe, sincroniza el estado (borra los bosses ya muertos).
    > 2. **Salida:** Cuando el último jugador sale, se marca `LastPlayerExitTime`.
    > 3. **Soft Reset:** Si alguien vuelve a entrar antes de 30 minutos, se retoma.
    > 4. **Hard Reset:** Si pasan > 30 minutos sin jugadores, se borra el `InstanceState` y la mazmorra se reinicia.

- [ ] **4. Sistema de Loot y Economía**
    > #### 4.1 Definición de Items (BoE)
    > En el `ItemDataSO`, añadimos:
    > - `IsSoulbound`: Bool.
    > - `BindType`: Enum (None, OnPickup, OnEquip).
    > - _Lógica:_ Al equipar un item con `BindType.OnEquip`, el sistema marca la instancia única del item (GUID) como `BoundToPlayerID`. Un item vinculado no puede entrar en la ventana de comercio.
    > #### 4.2 Distribución de Botín (Híbrido)
    > El `LootManager` gestiona el evento `OnDeath` de un Jefe.
    > - **Modo A: Personal Loot (Entrega Directa)**
    >   - El servidor calcula RNG para cada jugador por separado.
    >   - Si hay éxito, envía `GiveItemClientRPC` directo al inventario.
    >   - _Visual:_ UI Flotante ("Has recibido: Casco de Hierro").
    > - **Modo B: Need / Greed (Interacción)**
    >   - El Jefe se convierte en un contenedor "Lootable".
    >   - Jugador interactúa (F). Se abre UI de Grupo.
    >   - **Fase de Roll:** Botones (Necesidad [Dados] / Codicia [Moneda] / Pasar).
    >   - **Resolución:** El servidor espera X segundos o a que todos voten. El ganador recibe el item automáticamente.

- [ ] **5. Comercio Seguro (Trading System)**
    > Implementación de la Opción C1 (Comercio Seguro de Ventana Doble) para evitar estafas.
    > #### 5.1 Máquina de Estados de Comercio
    > 1. **Request:** Jugador A solicita comercio a Jugador B. B acepta.
    > 2. **Session Open:** Se abre UI. El movimiento se bloquea.
    > 3. **Offer:** Ambos arrastran items a su "Grid de Oferta".
    >    - _Validación:_ No se pueden poner items Vinculados (Soulbound).
    > 4. **Lock (Bloqueo):** Ambos pulsan "Bloquear Oferta".
    >    - La UI se pone gris. Ya no se pueden modificar items.
    > 5. **Confirm (Confirmación Final):** Ambos pulsan "Confirmar Intercambio".
    > 6. **Swap (Transacción Atómica):**
    >    - El Servidor ejecuta el intercambio de inventarios en el mismo frame.
    >    - Si uno falla (ej. inventario lleno), se cancela todo.

- [ ] **6. UI de Inventario y Contenedores**
    > '- **Grid System:** Inventario basado en slots.
    > - **Drag & Drop:** Mover items, equipar, tirar al suelo (con confirmación de destrucción o modelo 3D según performance).
    > - **Tooltips:** Al pasar el mouse, comparar stats con el item equipado (Shift para comparar).

- [ ] **7. Roadmap de Implementación - Fase 5**
    > #### Semana 13: El Multiverso (Escenas)
    > 1. **Scene Management:** Implementar la carga aditiva y el `SceneVisibilityManager` para el Host (ocultar lo que no ve).
    > 2. **Portales:** Crear el prefab `PortalTrigger` con lógica de requisitos (Level Check).
    > 3. **Persistencia:** Programar el `DungeonStateManager` para recordar bosses muertos.
    > #### Semana 14: Inventario y Loot
    > 1. **UI Base:** Crear el Canvas de Inventario y el sistema de Slots.
    > 2. **Loot Tables:** ScriptableObjects que definen qué tira cada mob (`DropRate`).
    > 3. **Need/Greed UI:** La ventana emergente de votación de dados.
    > #### Semana 15: Comercio y Vinculación
    > 1. **Binding:** Lógica de `OnEquip` que cambia el estado del item a "Soulbound".
    > 2. **Trade System:** Script de sesión de comercio (Handshake -> Lock -> Swap).
    > 3. **Polish:** Tooltips y sonidos de "Moneda" al lootear.
    > ---
    > ### CONCLUSIÓN DEL ROADMAP TÉCNICO
    > ¡Felicidades! Hemos completado la especificación de las 5 Fases críticas.
    > 1. **Fase 1:** Red y Movimiento (Cimientos).
    > 2. **Fase 2:** Datos y Persistencia (Seguridad).
    > 3. **Fase 3:** Combate y Habilidades (Core Loop).
    > 4. **Fase 4:** IA y Trinidad (Roles).
    > 5. **Fase 5:** Mundo y Economía (Estructura).


## Documento de Especificación Técnica - Fase 6: Progresión, Talentos y Crafting V1.0 Personalización Flexible y Escalabilidad

- [ ] **1. Visión de la Fase**
    > El objetivo es implementar los sistemas que permiten al jugador definir su identidad mecánica. Se desarrollará un sistema de Talentos dinámico basado en modificadores (Decoradores), un sistema de adquisición de habilidades simplificado (Escalado automático) y un sistema de Crafting determinista que exige gestión de inventario y ubicación física.

- [ ] **2. Sistema de Talentos (Architecture)**
    > #### 2.1 Estructura de Datos (Grafos)
    > Los árboles no son lineales. Usaremos un sistema de Nodos.
    > - `TalentNodeSO` **(ScriptableObject):**
    >   - `ID`, `Icon`, `Description`.
    >   - `MaxPoints` (ej. 3/3).
    >   - `Prerequisites`: Lista de `TalentNodeSO` padres (debe tener X puntos en el padre para desbloquear este).
    >   - `Cost`: 1 punto por nivel.
    > #### 2.2 Motor de Modificadores (Decorator Pattern)
    > Dado que elegiste la **Opción B (Decorador)**, no crearemos copias de habilidades. Implementaremos un "Pipeline de Cálculo".
    > - **Tipos de Modificadores:**
    >   1. **Stat Modifier:** `StatType.Strength`, `Value: +5`, `Mode: Percentage`.
    >   2. **Ability Modifier:** `TargetAbilityID: Fireball`, `Effect: AddBurn`.
    > - **Flujo de Cálculo (Server-Side):**
    >   Cuando el jugador lanza una habilidad, el servidor ejecuta:
    >   codeC#
    >   ```
    >   // 1. Obtener datos base
    >   float damage = ability.BaseDamage;
    >   // 2. Aplicar modificadores de Talentos activos
    >   foreach (var talent in player.ActiveTalents) {
    >       if (talent.Modifies(ability.ID)) {
    >            damage = talent.ApplyModification(damage);
    >            // O inyectar efecto secundario (Burn)
    >            if (talent.HasSideEffect) ability.AddSideEffect(talent.SideEffect);
    >       }
    >   }
    >   ```
    > #### 2.3 Respec (Libertad Total)
    > - **UI:** Botón "Reiniciar Talentos" en la ventana `N`.
    > - **Lógica:**
    >   - Verifica que el jugador no esté en combate.
    >   - Limpia la lista `ActiveTalents`.
    >   - Devuelve `PointsSpent` a `AvailablePoints`.
    >   - Recalcula todos los Stats inmediatamente.

- [ ] **3. Entrenadores y Spellbook**
    > #### 3.1 Adquisición de Habilidades
    > - **Trainer NPC:** Tiene una lista de `AbilityDefinitionSO`.
    > - **Filtro:** Muestra solo habilidades de la clase del jugador.
    > - **Estado:**
    >   - _Gris:_ Nivel insuficiente.
    >   - _Verde:_ Disponible para comprar (Cuesta Oro).
    >   - _Oculto/Check:_ Ya aprendida.
    > #### 3.2 Escalado Automático (Math)
    > No existen los "Rangos". La fórmula de daño es dinámica:
    > - `TotalDamage = BaseDamage + (PlayerLevel * LevelScaling) + (PlayerStat * StatCoefficient)`.
    > - _Ventaja:_ Esto mantiene las habilidades relevantes desde el nivel 1 al 100 sin necesidad de actualizar la base de datos.
    > #### 3.3 Spellbook & Hotbar
    > - `LearnedAbilities`**:** Lista de IDs en el `PlayerData`.
    > - **Drag & Drop:** El jugador arrastra el icono del libro a la barra.
    > - **Hotbar System:** Guarda la referencia `(Slot 1 -> AbilityID 5)` en los datos del jugador.

- [ ] **4. Sistema de Crafting (Artesanía)**
    > #### 4.1 Definición de Recetas (`RecipeSO`)
    > - `ResultItem`: ItemSO (ej. Espada de Hierro).
    > - `Amount`: int (ej. 1).
    > - `Ingredients`: List `<ItemSO, Quantity>` (ej. 2 Hierro, 1 Madera).
    > - `WorkstationType`: Enum (Yunque, Alquimia, Hoguera).
    > - `LevelRequired`: int (Nivel del Personaje, Opción C2).
    > - `CraftTime`: float (ej. 2.0s).
    > #### 4.2 Lógica de Fabricación (Server Authoritative)
    > 1. **Validación de Ubicación:** El servidor verifica `Vector3.Distance(Player, Workstation) < 3.0f`.
    > 2. **Validación de Inventario (Opción D1):**
    >    - El servidor busca en `PlayerInventory` si tiene los materiales requeridos.
    > 3. **Proceso:**
    >    - Cliente muestra barra de progreso (2s).
    >    - Si se mueve -> Cancela.
    > 4. **Resultado (Determinista - Opción C1):**
    >    - Al terminar, el servidor resta materiales y suma el item resultado.
    >    - **Sin fallo:** Siempre éxito.

- [ ] **5. Interfaz de Usuario (UI)**
    > '- **Talent Tree:** Panel con Scroll, líneas dibujadas dinámicamente entre nodos padres e hijos. Tooltips que explican el beneficio actual y el siguiente rango.
    > - **Crafting UI:** Lista de recetas a la izquierda. Detalles a la derecha (Iconos de materiales requeridos en Rojo/Verde según disponibilidad). Botón "Fabricar".
    > ---

- [ ] **6. Roadmap de Implementación - Fase 6**
    > #### Semana 17: Talentos (Backend & UI)
    > 1. **Data:** Crear `TalentNodeSO` y el gestor de puntos.
    > 2. **Logic:** Implementar el `ModifierSystem` para que los Stats se recalculen al gastar puntos.
    > 3. **UI:** Construir el visualizador del árbol (Graph View básico en Canvas).
    > #### Semana 18: Entrenadores y Escalado
    > 1. **Skills:** Modificar el cálculo de daño (Fase 3) para incluir la fórmula de escalado por nivel.
    > 2. **Shop:** Crear la lógica de "Trainer" que desbloquea IDs en la lista del jugador a cambio de Oro.
    > #### Semana 19: Crafting
    > 1. **Recipes:** Crear base de datos de recetas iniciales.
    > 2. **Stations:** Crear triggers "Area de Crafting" (Yunque).
    > 3. **Interaction:** UI de crafting que filtra recetas según la estación cercana.
    > ---
    > ### ¿Cómo procedemos?
    > Esta fase requiere mucha estructura de datos (ScriptableObjects).
    > **Sugerencia:**
    > Lo más difícil aquí es el **Sistema de Modificadores de Talentos** para que sea limpio y no un montón de `if/else` en el código de combate.


## Documento de Especificación Técnica - Fase 7: Herramientas Sociales y de Grupo V1.0 Enfoque: Coordinación Táctica y Feedback Visual

- [ ] **1. Visión de la Fase**
    > El objetivo es transformar a los jugadores individuales en una unidad cohesiva. Se implementará una gestión de grupo robusta con reglas de proximidad para evitar abusos (Power Leveling), una interfaz de sanación reactiva que filtra el "ruido visual" y un sistema de chat inmersivo con burbujas de texto.

- [ ] **2. Sistema de Grupo (Party Backend)**
    > #### 2.1 Estructura de Datos
    > Utilizaremos `NetworkList` para sincronizar la membresía del grupo.
    > - `PartyManager` **(Singleton en Host):**
    >   - `NetworkList<ulong> MemberClientIDs;`
    >   - `NetworkVariable<ulong> LeaderID;`
    >   - `int MaxMembers = 5;`
    > #### 2.2 Lógica de Gestión (Opción B2 - Sucesión)
    > - **Creación:** Al invitar al primer jugador.
    > - **Abandono/Desconexión:**
    >   - Si un miembro sale, se elimina de la lista y se actualiza la UI de todos.
    >   - Si el **Líder** sale, el sistema ejecuta `PromoteLeader()`: Selecciona al `MemberClientIDs[0]` (el más antiguo restante) como nuevo líder automáticamente.
    > #### 2.3 Reglas de Compartición (Opción B1 - Proximidad)
    > - **Evento:** `OnEnemyDeath`.
    > - **Validación:**
    >   1. Calcular XP total.
    >   2. Iterar sobre miembros del grupo.
    >   3. `if (Vector3.Distance(EnemyPos, MemberPos) < 100f)`: El miembro recibe XP y derecho a Loot.
    >   4. `else`: Mensaje de sistema "Estás demasiado lejos para recibir recompensa"

- [ ] **3. Interfaz de Unidad (Party Frames)**
    > Esta es la herramienta principal del Healer. Debe ser eficiente (Low CPU cost).
    > #### 3.1 Componentes Visuales
    > - Prefab `PartyMemberFrame`:
    >   - Barra de Vida (Verde -> Roja).
    >   - Barra de Recurso (Azul/Amarilla/Roja según clase).
    >   - Nombre.
    >   - Contenedor de Iconos (Debuffs).
    > #### 3.2 Feedback de Rango (Opción A1 - Alpha Fading)
    > - **Script:** `RangeAlphaChanger.cs` en cada Frame de la UI.
    > - **Lógica:**
    >   - En `Update()` (o corrutina cada 0.2s):
    >   - Calcular distancia entre `LocalPlayer` y `TargetMember`.
    >   - Si `Distancia > 40m`: `CanvasGroup.alpha = 0.5f;`
    >   - Si `Distancia <= 40m`: `CanvasGroup.alpha = 1.0f;`
    > #### 3.3 Sistema de Debuffs Inteligente (Opción A2)
    > - **Prioridad de Visualización:**
    >   1. **Dispellable:** Debuffs que MI clase puede limpiar (ej. Veneno si soy Druida). -> **Icono Grande + Borde Brillante.**
    >   2. **Critical:** Mecánicas de Boss (ej. "Bomba de Tiempo"). -> **Icono Grande.**
    >   3. **Standard:** Otros debuffs. -> **Icono Pequeño.**
    >   4. **Ignored:** Buffs irrelevantes o debuffs de ambiente menores. -> **Oculto.**
    > - **Implementación:** El `PartyFrame` recibe la lista de estados del miembro, la filtra según la clase del jugador local y ordena los iconos de izquierda a derecha por prioridad.

- [ ] **4. Sistema de Comunicación (Chat)**
    > #### 4.1 Estructura de Canales
    > - **Enum:** `ChatChannel { Global, Zone, Party, Whisper, System, Combat }`.
    > - **UI:** Ventana de Chat con pestañas configurables (ej. una pestaña solo para "Combate" para leer el log).
    > #### 4.2 Burbujas de Chat (Opción C2 - Inmersión)
    > - **World Space UI:** Canvas flotante sobre el `PlayerHeadBone`.
    > - **Object Pooling:** **CRÍTICO.** No instanciar y destruir burbujas. Tener un "Pool" de 20 burbujas ocultas y reciclarlas.
    > - **Comportamiento:**
    >   - Jugador escribe: "¡Ayuda!".
    >   - Aparece burbuja sobre su cabeza.
    >   - La burbuja mira siempre a la cámara (`Billboard`).
    >   - Desaparece tras 5 segundos o si se escribe otro mensaje.
    > #### 4.3 Combat Log
    > - **Formato:** Texto enriquecido (Rich Text) para colorear.
    >   - Daño Físico: Blanco/Rojo.
    >   - Daño Mágico: Color de la escuela (Fuego = Naranja).
    >   - Curación: Verde.
    > - **String:** `"[Fuente] golpea a [Objetivo] por <color=red>50</color> (<color=grey>10 Mitigado</color>)"`.

- [ ] **5. Marcadores Tácticos (Opción C1 - Híbrido**
    > Herramienta del Líder para coordinar focus.
    > #### 5.1 Sincronización
    > - Enemigos tienen `NetworkVariable<int> MarkerIndex` (-1 = Ninguno, 0 = Calavera, 1 = Cruz...).
    > - Solo el Líder puede escribir en esta variable (ServerRPC con validación de liderazgo).
    > #### 5.2 Visualización Híbrida
    > 1. **Mundo 3D:** Un Sprite Render flotando 2 metros sobre la cabeza del enemigo.
    > 2. **Target Frame (UI):** Si seleccionas al enemigo, el icono de la Calavera aparece al lado de su nombre en la interfaz 2D.

- [ ] **6. Roadmap de Implementación - Fase 7**
    > #### Semana 21: Backend de Grupo
    > 1. **Party Manager:** Script de invitación, aceptación y lista sincronizada.
    > 2. **Logica de Sucesión:** Probar qué pasa si el líder desconecta el cable de red.
    > 3. **Distance Check:** Implementar la restricción de XP por distancia.
    > #### Semana 22: UI de Grupo (Frames)
    > 1. **Prefab Design:** Diseñar la barra de vida y el grid de debuffs.
    > 2. **Smart Filter:** Programar la lógica que decide qué iconos mostrar y qué tamaño darles.
    > 3. **Click-to-Cast:** Asegurar que hacer clic en la barra selecciona al miembro en el `TargetingSystem`.
    > #### Semana 23: Chat y Marcadores
    > 1. **Chat System:** Ventana básica y canales.
    > 2. **Bubbles:** Implementar el Pool de burbujas flotantes.
    > 3. **Markers:** Sistema de iconos sincronizados sobre enemigos.
    > ---
    > ### ¿Cómo procedemos?
    > Con esto, la base social está lista.
    > **Sugerencia:**
    > El **Combat Log** suele ser subestimado, pero es lo primero que programaría porque es la herramienta de "Debugging" definitiva para ti como desarrollador. Si algo no muere o el daño no cuadra, el Combat Log te dirá la verdad matemática.


## Documento de Especificación Técnica - Fase 8: Contenido, Jefes y Narrativa V1.0 Enfoque: Atmósfera Estática, Jefes Únicos y Narrativa Sincronizada

- [ ] **1. Visión de la Fase**
    > El objetivo es vestir el juego. Reemplazaremos los prototipos grises con arte final, aprovechando la iluminación pre-calculada para lograr una atmósfera de alta calidad sin coste de CPU. Los Jefes serán entidades únicas programadas a medida (`Classes` derivadas) para ofrecer mecánicas complejas. Las cinemáticas respetarán la experiencia del veterano (Auto-Skip) y la curiosidad del novato.

- [ ] **2. Pipeline de Entornos (Environment Tech)**
    > #### 2.1 Iluminación (Baked Global Illumination)
    > Dado que los biomas tienen iluminación fija (Atardecer/Mediodía/Noche):
    > - **Workflow:**
    >   - Todos los objetos estáticos (Árboles, Ruinas, Suelo) marcados como `Static`.
    >   - **Lightmaps:** Hornear luces en texturas.
    >   - **Light Probes:** Colocar redes de sondas densas en zonas jugables para que los personajes (Dinámicos) reciban la luz y el color del entorno estático correctamente.
    > - **Gestión de Escenas:** Cada escena aditiva (Mazmorra) tiene su propio `LightingSettings`. Al cargar la escena, Unity gestiona el cambio de Skybox y Fog automáticamente.
    > #### 2.2 Navegación (NavMesh Fragmentado)
    > - **Componente:** `NavMeshSurface` (Unity AI Navigation package).
    > - **Segmentación:**
    >   - Una `NavMeshSurface` por cada "Isla" o zona de combate.
    >   - **No hay puentes de navegación** entre el Bosque y la Mazmorra. Los enemigos no pueden salir de su zona lógica.
    > - **Baking:** Se realiza en tiempo de edición, guardando la data en la escena.

- [ ] **3. Arquitectura de Encuentros (Boss Scripting)**
    > Optamos por la flexibilidad del código puro (`Hardcoded C#`).
    > #### 3.1 Clase Base: `BossBaseController`
    > Clase abstracta que maneja lo común:
    > - **Stats:** Vida, Mana, Tabla de Amenaza (Fase 4).
    > - **Eventos:** `OnAggro`, `OnDeath`, `OnReset`.
    > - **Reset Lógico (Hard Reset):**
    >   - Si no hay jugadores en la `BossRoomTrigger`:
    >   - `Transform.position = SpawnPoint`.
    >   - `CurrentHP = MaxHP`.
    >   - `Minions.Clear()` (Destruir invocaciones).
    >   - `ThreatTable.Clear()`.
    > #### 3.2 Implementación Específica (Ejemplo: `SkeletonKing.cs`)
    > Hereda de `BossBaseController`. Usa una Máquina de Estados interna simple (`switch/case`).
    > - **Fase 1 (100% - 50% HP):**
    >   - Ataque básico y "Golpe de Escudo" (CD 10s).
    > - **Transición (50% HP):**
    >   - Vuelve inmune (`IsInvulnerable = true`).
    >   - Camina al centro de la sala.
    >   - Invoca 4 `SkeletonMinion`.
    > - **Fase 2 (Esbirros muertos):**
    >   - Pierde inmunidad.
    >   - Gana buff "Enrage" (+50% Daño).

- [ ] **4. Sistema de Cinemáticas (Timeline + Netcode)**
    > #### 4.1 Intro del Jefe (Sincronizada)
    > - **Trigger:** Al entrar el primer jugador a la sala del jefe.
    > - **Check de "Visto" (Auto-Skip):**
    >   - El Servidor consulta: `bool allHaveSeen = true;`
    >   - Revisa el `PlayerData.HasSeenBossIntro[BossID]` de todos los conectados.
    >   - **Si TODOS true:** Salta la cinemática e inicia combate directo.
    >   - **Si ALGUIEN false:** Inicia modo Cinemática.
    > - **Modo Cinemática (Votación):**
    >   - **Input:** Bloqueado (Nadie se mueve).
    >   - **UI:** Botón "Saltar (F1)".
    >   - **Lógica:** Si `Votes >= Players / 2 + 1` -> El servidor corta el Timeline y activa al Jefe.
    > #### 4.2 Outro del Jefe (Muerte)
    > - **Trigger:** `OnDeath` del Jefe.
    > - **Lógica (Individual):**
    >   - El servidor dispara `ClientRPC: PlayOutro()`.
    >   - Cada cliente reproduce su Timeline localmente.
    >   - **Input:** Jugador se queda en estado `Idle` (congelado).
    >   - **Skip:** Si el jugador presiona Esc/Saltar, su cámara vuelve al juego y recupera control. Puede ver a los demás personajes quietos (aún viendo el video).

- [ ] **5. Narrativa y Diálogos**
    > '- **Sistema de Burbujas:** Reutilizamos el sistema de Chat de la Fase 7.
    > - **NPCs de Quest:**
    >   - Trigger de Proximidad.
    >   - Al interactuar (F), muestra ventana de texto con botones "Aceptar / Rechazar".
    >   - El texto se lee de un `DialogueSO` (ScriptableObject) para fácil localización/traducción.

- [ ] **6. Roadmap de Implementación - Fase 8**
    > #### Semana 25: Arte y Atmósfera (El "Look")
    > 1. **Baking:** Configurar la iluminación de la escena "Bosque" y "Mazmorra 1". Ajustar las Light Probes para que los personajes no se vean oscuros.
    > 2. **NavMesh:** Hornear las superficies de navegación separadas.
    > 3. **Spawners:** Colocar los puntos de aparición de enemigos y configurar sus rutas de patrulla visuales.
    > #### Semana 26: El Primer Jefe Real (`SkeletonKing`)
    > 1. **Scripting:** Programar `BossBaseController` y la lógica específica del Rey Esqueleto.
    > 2. **Fases:** Implementar la lógica de inmunidad y chequeo de esbirros muertos.
    > 3. **Reset:** Probar exhaustivamente que el jefe vuelva a su sitio y se cure si los jugadores intentan hacer trampas.
    > #### Semana 27: Cinemáticas y UX
    > 1. **Timeline:** Crear la secuencia de entrada (Cámara moviéndose, Jefe gritando).
    > 2. **Cutscene Manager:** Implementar la lógica de red para pausar a los jugadores y gestionar la votación de salto.
    > 3. **Persistencia:** Guardar en el `SaveFile` (Fase 2) el array de `HasSeenBossIntro`.
    > ---
    > ### CONCLUSIÓN DEL PLAN MAESTRO
    > Hemos recorrido el camino completo desde "Mover un cubo" hasta "Matar al Rey Esqueleto con cinemáticas y votación".
    > Has definido:
    > 1. **Arquitectura:** P2P robusta con Host-Play.
    > 2. **Datos:** Seguros, encriptados y persistentes.
    > 3. **Combate:** Tab-Targeting clásico y responsivo.
    > 4. **Rol:** Trinidad sagrada (Tanque/Healer/DPS) forzada por mecánicas de Aggro.
    > 5. **Mundo:** Instanciado, con economía de loot y crafting.
    > 6. **Contenido:** Jefes complejos y narrativa respetuosa.


## Documento de Especificación Técnica - Fase 9: Pulido, UX y End-Game V1.0

- [ ] **1. Visión de la Fase**
    > El objetivo es elevar el estándar del producto final. Implementaremos un sistema de configuración robusto y portátil para PC, una arquitectura de audio inteligente que prioriza la claridad sobre el caos, y un modo "Heroico" que reutiliza los assets existentes para duplicar las horas de juego mediante retos mecánicos y recompensas visuales exclusivas (Recolors).

- [ ] **2. Sistema de Configuración (Settings & Input)**
    > #### 2.1 Persistencia (JSON Local)
    > - **Archivo:** `Application.persistentDataPath + "/user_config.json"`.
    > - **Estructura:**
    >   codeJSON
    >   ```
    >   {
    >     "Graphics": { "Resolution": "1920x1080", "Fullscreen": true, "ShadowQuality": 2 },
    >     "Audio": { "MasterVolume": 0.8, "MusicVolume": 0.6, "SFXVolume": 1.0 },
    >     "Keybindings": [
    >       { "Action": "Attack", "Key": "Z" },
    >       { "Action": "Interact", "Key": "F" }
    >     ]
    >   }
    >   ```
    > - **Carga:** Se lee en el `Awake()` del `GameManager`. Si no existe, crea uno por defecto.
    > #### 2.2 Reasignación de Teclas (Input Remapping)
    > - **Tecnología:** Unity Input System (`InputActionAsset`).
    > - **Lógica de Conflicto (Swap Inteligente):**
    >   - Evento: Jugador intenta asignar `Q` a "Atacar".
    >   - Check: ¿`Q` está en uso? -> Sí, en "Caminar Izquierda".
    >   - Acción:
    >     1. Desasignar `Q` de "Caminar Izquierda".
    >     2. Asignar `Q` a "Atacar".
    >     3. (Opcional) Asignar la tecla vieja de "Atacar" (ej. `Z`) a "Caminar Izquierda".
    >     4. Actualizar UI inmediatamente para reflejar el cambio.

- [ ] **3. Arquitectura de Audio (Audio Manager)**
    > #### 3.1 Música (Ambiental vs Jefe)
    > - **Mundo Abierto / Mazmorra:**
    >   - Lista de reproducción (`Playlist`) que hace loop con `Crossfade` suave. No reacciona al combate básico ("Trash mobs") para evitar cortes molestos.
    > - **Jefes (Boss Music):**
    >   - `AudioManager` escucha el evento `OnBossAggro`.
    >   - Hace fade-out de la música ambiental -> Fade-in "Tema del Jefe".
    >   - **Cooldown de Tensión (Opción B1):** Al morir el jefe, la música de tensión sigue 5 segundos, luego fade-out lento hacia la música ambiental.
    > #### 3.2 Prioridad de SFX (Voice Stealing)
    > - **Problema:** 10 Bolas de fuego suenan a la vez = Ruido blanco.
    > - **Solución:** Sistema de prioridad dinámica en `SoundEmitter`.
    >   - `MaxVoices = 3` (para hechizos repetitivos).
    >   - Antes de `Play()`, calcular distancia a la `Camera.main`.
    >   - Si hay más de 3 sonidos iguales sonando, detener el más lejano y reproducir el nuevo (o simplemente no reproducir el nuevo si está lejos).

- [ ] **4. End-Game: Selector de Dificultad y Seguridad de Instancia**
    > En lugar de una configuración global, la dificultad es una propiedad mutable de cada Mazmorra específica, controlada por el Líder del Grupo.
    > #### 4.1 Selector de Dificultad (Por Portal)
    > - **UI del Portal:** Al interactuar con el portal de entrada de una Mazmorra Grande:
    >   - Si eres Miembro: Solo ves el estado actual ("Dificultad: Normal").
    >   - Si eres **Líder**: Ves un Dropdown/Selector ("Normal" / "Heroico").
    > - **Validación de Cambio (**`CanChangeDifficulty`**):**
    >   - El Líder intenta cambiar a Heroico.
    >   - El Servidor chequea: `ActivePlayersInDungeonCount`.
    >   - **Si Count == 0:** El cambio se aplica. El `DungeonStateManager` actualiza la flag `IsHeroic = true`.
    >   - **Si Count > 0:** Error en pantalla: _"No se puede cambiar la dificultad mientras hay jugadores (o desconectados recientes) dentro."_
    > #### 4.2 Lógica de Desconexión en Instancia (The "Ghost Timer")
    > El sistema debe diferenciar entre "Caída de internet breve" y "Abandono", evitando que un jugador desconectado bloquee el cambio de dificultad para siempre.
    > - **Estado:** `DisconnectedInside`
    >   - Cuando un jugador pierde la conexión dentro de una escena de Mazmorra, el Host **NO** lo elimina inmediatamente de la lista `ActivePlayersInDungeon`.
    >   - El personaje desaparece visualmente, pero su "Slot" lógico sigue ocupando la instancia.
    > - **El Temporizador (2 Minutos):**
    >   - Al desconectar, el Host inicia una Corrutina: `StartDisconnectTimer(PlayerID, 120f)`.
    >   - **Caso A: Reconexión Rápida (< 120s):**
    >     - El jugador vuelve. El Host detiene el timer.
    >     - El jugador respawnea en su última posición guardada dentro de la mazmorra. (Sigue jugando normal).
    >   - **Caso B: Tiempo Agotado (> 120s):**
    >     - El Host ejecuta `ForceKickFromDungeon(PlayerID)`.
    >     - **Acción de Datos:** Modifica el archivo de guardado del jugador (en memoria del servidor o disco): `Position = DungeonEntrancePoint` (Afuera en el mundo abierto).
    >     - **Acción de Estado:** Resta 1 al `ActivePlayersInDungeonCount`.
    >     - _Resultado:_ Ahora el contador es 0 y el Líder puede cambiar la dificultad.
    > #### 4.3 Implementación en `DungeonManager`
    > codeC#
    > ```
    > public class DungeonInstance
    > {
    >     public Difficulty CurrentDifficulty;
    >     public List<ulong> PlayersInside;
    >     public Dictionary<ulong, Coroutine> DisconnectTimers; // ID -> Timer
    >     public void OnPlayerDisconnect(ulong clientId) {
    >         // No lo sacamos de PlayersInside todavía
    >         // Iniciamos cuenta regresiva
    >         DisconnectTimers[clientId] = StartCoroutine(KickRoutine(clientId, 120f));
    >     }
    >     private IEnumerator KickRoutine(ulong clientId, float duration) {
    >         yield return new WaitForSeconds(duration);
    >         // Se acabó el tiempo
    >         PlayersInside.Remove(clientId);
    >         SaveSystem.ForceMovePlayerToEntrance(clientId);
    >         DisconnectTimers.Remove(clientId);
    >         // Notificar al Líder que la mazmorra está libre
    >         CheckIdeallyEmpty();
    >     }
    > }
    > ```
    > 4\.4 Lógica del Jefe Final (Scripting Extendido)
    > Extendemos el script del Jefe (Fase 8) para soportar el flujo del 20%.
    > codeC#
    > ```
    > public override void TakeDamage(float amount) {
    >     base.TakeDamage(amount);
    >     // Check de Fase Final
    >     if (currentHP <= maxHP * 0.20f) {
    >         if (GameDifficulty == Difficulty.Normal) {
    >             // VICTORIA NARRATIVA
    >             StartCoroutine(PlayVictoryCinematic()); // El jefe huye o se rinde
    >             EndEncounter(win: true);
    >         }
    >         else if (GameDifficulty == Difficulty.Heroic && !isInTrueForm) {
    >             // ACTIVAR FASE OCULTA
    >             EnterTrueForm(); // Sana un poco, cambia modelo, nuevas skills
    >         }
    >     }
    >     // Muerte Real (Solo Heroico)
    >     if (isInTrueForm && currentHP <= maxHP * 0.001f) {
    >          DieForReal();
    >     }
    > }
    > ```
    > #### 4.5 Loot Heroico (Variantes Visuales)
    > - **Data:** `ItemSO` tiene un campo `HeroicVariant` (referencia a otro `ItemSO`).
    > - **Arte:**
    >   - **Normal:** "Espada del Rey" (Filo plateado).
    >   - **Heroico:** "Espada del Rey Caído" (Filo rojo brillante, mismo modelo 3D, diferente Material/Textura).
    > - **Stats:** Escalado numérico simple (+20% Stats).
    > ---

- [ ] **5. Integración Final y Red**
    > '- **Steamworks:** Se inicializa solo para Overlay y Logros.
    > - **Conexión:** Se mantiene el sistema de **Unity Relay (Código)** definido en la Fase 1. Esto asegura que la conexión funcione siempre, independientemente de si la API de amigos de Steam falla o si decides portar a otras tiendas (Epic/Itch) en el futuro.

- [ ] **6. Roadmap de Implementación - Fase 9**
    > ### 6. Roadmap de Implementación - Fase 9
    > #### Semana 29: Settings y JSON
    > 1. **Backend:** Crear `ConfigManager.cs` que serializa/deserializa el JSON.
    > 2. **UI:** Construir el menú de opciones.
    > 3. **Input:** Implementar la lógica de "Rebind" con intercambio de teclas.
    > #### Semana 30: Audio System
    > 1. **Mixer:** Configurar canales (Master, Music, SFX) en Unity Audio Mixer.
    > 2. **Manager:** Script que gestiona las listas de reproducción y el cambio de estado en Bosses.
    > 3. **Optimization:** Implementar el chequeo de distancia para sonidos repetitivos.
    > #### Semana 31: Heroic Mode
    > 1. **Loot:** Duplicar los ScriptableObjects de items Tier Máximo, cambiarles el nombre y asignarles un Material tintado.
    > 2. **Boss Logic:** Modificar el script del Jefe Final para incluir el `if (Heroic)` y la fase "True Form".
    > 3. **Portal UI:** Crear el menú emergente del portal que solo el Líder puede editar.
    > 4. **Disconnect Logic:** Implementar el `DisconnectTimer` en el Host. Es vital probar esto desconectando el cable de red simulado (Unity Editor) y esperando 2 minutos para ver si el servidor libera el bloqueo de dificultad.
    > 5. **Heroic Toggle:** Conectar la variable `IsHeroic` para que cambie las tablas de loot y la vida de los enemigos al spawnear.
    > #### Semana 32: Pulido Final (Gold Master)
    > 1. **Créditos:** Pantalla de créditos.
    > 2. **Bug Fixing:** Semana dedicada exclusivamente a cerrar tickets de Trello.
    > 3. **Build:** Generar la versión 1.0 Release.
    > ---
    > ### RESUMEN GLOBAL DEL PROYECTO "THE ETHER DOMES"
    > Has completado la especificación de un MMORPG Cooperativo completo.
    > - **Core:** P2P, Movimiento WoW-like, Persistencia segura.
    > - **Loop:** Tab-Targeting, Trinidad (Tank/Healer/DPS), Crafting.
    > - **Contenido:** 5 Biomas, Jefes con Fases, Modo Heroico.
    > - **Tech:** Unity 6 + Netcode for GameObjects + JSON Configs.


## Documento de Especificación Técnica y de Diseño v1.2

- [ ] **1. Vision General**
    > Un Micro-MMORPG cooperativo para 1 a 10 jugadores. Combina la "Santa Trinidad" de WoW con la portabilidad de servidor de Valheim.

- [ ] **2. Arquitectura Tecnica**
    > Modelo de Red: Híbrido Host-Play (P2P con Host) / Servidor Dedicado (Headless).
    > Datos de Personaje: Persistencia Local (Cliente) con encriptación para evitar trampas. Permite viajar entre mundos con el mismo personaje ("Cross-World").
    > Seguridad: Validación server-side al conectar (Checkeo de integridad de stats del equipo).

- [ ] **3. Clases y Combate**
    > 3\.1 Sistema
    > Inputs: 20 habilidades activas (Barra principal 1-0, secundaria Ctrl + 1-0). Asignación libre de teclas.
    > Sin Colisión: Los jugadores no colisionan entre sí.
    > 3\.2 Roles y Balance (La Trinidad Definida)
    > Cada personaje elige UNA clase única (ej. Guerrero O Mago).
    > Jerarquía de Daño (DPS): DPS Dedicado > Tanque/Sanador.
    > Jerarquía de Aguante (Mitigación): Tanque Dedicado > DPS/Sanador.
    > Jerarquía de Curación (Sustain): Sanador Dedicado > Tanque/DPS.
    > Nota: Las clases híbridas tienen capacidades de autosuficiencia para contenido en solitario, pero sus números nunca igualarán al especialista en un entorno grupal.
    > 3\.3 Progresión
    > Árbol de Talentos: Puntos por nivel para especialización.
    > Tope de Nivel: 100.

- [ ] **4. Estructura del Mundo**
    > 5 Regiones (Biomas) instanciadas conectadas linealmente.
    > Bosque Exuberante.
    > Arrecife de Coral.
    > Llanura Dorada.
    > Cordillera Montañosa.
    > Ciudadela Dorada.

- [ ] **5. Mazmorras y Encuentro**
    > El contenido PvE principal está fuertemente estructurado.
    > 5\.1 Distribución del Contenido
    > Cada una de las 5 Regiones contiene:
    > 3 Mazmorras Pequeñas (3 Jefes cada una).
    > 1 Mazmorra Grande (5 Jefes, contiene el jefe de historia de la zona).
    > Total del juego: 15 Mazmorras Pequeñas + 5 Grandes.
    > 5\.2 Mecánicas
    > Escalado Flex:
    > Numérico (Vida/Daño) según número de jugadores.
    > Mecánico (Habilidades complejas requieren más jugadores o se simplifican para solos).
    > Wipes y Respawn:
    > Mazmorra Pequeña: Respawn al inicio.
    > Mazmorra Grande: Puntos de control (Checkpoints) tras ciertos jefes.
    > Muerte en combate: Se debe esperar el final del encuentro.

- [ ] **6. Economia y Sistemas**
    > 6\.1 Loot y Comercio
    > Reparto: Configurable (Personal Loot / Need or Greed).
    > Regla de Vinculación (Binding): "Ligado al Equipar" (BoE).
    > Si el ítem cae y lo guardas en la mochila: Es Comerciable.
    > Si te equipas el ítem (lo usas): Queda Ligado al Personaje (Intransferible).
    > Esto permite comercio libre pero evita el "préstamo infinito" de equipo de alto nivel.
    > 6\.2 Crafting
    > Universal (sin clases de oficio).
    > Aprendizaje por Instructores (NPC) + Práctica.
    > Recetas desbloqueadas por nivel y región.
    > 6\.3 Muerte y Desgaste
    > Mundo: Corpse run (fantasma).
    > Desgaste: Equipo pierde durabilidad al morir. Si llega a 0, pierde stats hasta reparar con Oro.

- [ ] **7. Questing**
    > Sincronización: Misión debe estar activa en el diario personal para contar.
    > Objetivos: Completar un objetivo (ej. matar jefe) cuenta para todo el grupo simultáneamente si tienen la misión.

- [ ] **8. End-game**
    > Al llegar al Nivel 100 y completar la Ciudadela:
    > Dificultad Aumentada: Rejugar mazmorras en modo difícil para loot Tier Máximo.
    > Mazmorra Final "True Form": Nueva dificultad para la última mazmorra grande con una fase oculta del Jefe Final y cinemática verdadera (True Ending).
    > Contenido Abierto: Finalización de misiones secundarias y coleccionismo en el mundo.


## Pendiente

- [ ] **Fase 1.3 Sistema de Lobby (Valheim Style)**
    > '- Implementar **Unity Relay** y **Lobby** (servicios gratuitos de Unity Gaming Services para pruebas) para evitar abrir puertos en el router.
    > - Crear una UI simple: "Crear Sala", "Unirse con Código".

- [ ] **Fase 2: Persistencia y Datos del Jugador (Semana 3-4)**
    > _Objetivo: Crear un personaje, guardar sus stats en un archivo encriptado y cargarlo en otra sesión._
    > Este es el pilar de tu diseño "Cross-World".

- [ ] **Fase 2.2 Sistema de Guardado/Carga (JSON + AES)**
    > '- Crear el `SaveManager`.
    > - Serializar la clase `CharacterData` a JSON.
    > - **Crucial:** Implementar la encriptación AES antes de escribir en disco para cumplir con tu requisito de seguridad anti-trampas.

- [ ] **Fase 2.3 Validación Server-Side (Handshake):**
    > '- Cuando un jugador se une, el servidor debe leer sus datos y validar que no tenga "99999 de fuerza" (Integrity Check).

- [ ] **Fse 3: El Núcleo de Combate y "Tab Targeting" (Mes 2)**
- [ ] **Fase 3.1 Sistema de Selección (Targeting System):**
    > '- Lógica de `Tab`: Raycast o SphereCast frente al jugador para seleccionar al enemigo más cercano.
    > - UI de Target: Mostrar el marco de unidad del objetivo seleccionado (Vida, Nombre).

- [ ] **Fase 3.2 Sistema de Habilidades (Abilities Architecture):**
    > '- Crear un sistema modular (ScriptableObjects) para habilidades.
    > - Propiedades: `CastTime`, `Cooldown`, `Range`, `Damage`, `Cost`.
    > - Implementar la lógica de "Casteo" (Barra de progreso).

- [ ] **Fase 3.3 Networking de Combate:**
    > '- Uso de `ServerRPC` para pedir permiso de atacar.
    > - Uso de `ClientRPC` para mostrar efectos visuales/partículas a todos.
    > - Cálculo de daño **siempre en el servidor**.

- [ ] **Fase 4: La "Santa Trinidad" y la IA (Mes 3)**
    > _Objetivo: Un tanque aguanta, un DPS pega y un enemigo reacciona._

- [ ] **Fase 4.1 Tablas de Amenaza (Aggro Tables):**
    > '- Cada enemigo debe tener una lista: `Dictionary<PlayerID, int> ThreatAmount`.
    > - El Tanque genera x2 de amenaza. El Healer genera amenaza al curar.
    > - La IA ataca siempre al que tenga más amenaza.

- [ ] **Fase 4.2 Estados de la IA (State Machine):**
    > '- Idle (Patrulla).
    > - Chase (Perseguir objetivo).
    > - Combat (Usar habilidades de jefe).
    > - Reset (Si el jugador se aleja demasiado o muere, el enemigo vuelve y se cura).

- [ ] **Fase 4.3 Interacción de Clases:**
    > '- Implementar una habilidad de cura, una de taunt (provocación) y una de daño directo para probar las sinergias.

- [ ] **Fase 5: Estructura del Mundo y Mazmorras (Mes 4)**
    > _Objetivo: Viajar del "Bosque" a una "Mazmorra" y que funcione el instanciado._

- [ ] **Fase 5.1 Gestión de Escenas:**
    > '- Carga aditiva de escenas o cambio de escena para mazmorras.
    > - Puntos de Spawn definidos.

- [ ] **Fase 5.2 Portales e Instancias:**
    > '- Trigger que al entrar, mueve al grupo (Network) a la escena de la mazmorra.
    > - Sistema de "Muros invisibles" basado en Nivel/Quest (según tu diseño).

- [ ] **Fase 5.3 Loot y Economía Básica:**
    > '- Al morir un enemigo, generar un objeto.
    > - Lógica de `BoE` (Bind on Equip).
    > - UI de Inventario básica.

- [ ] **Fase 6 Progresión y Especialización (Mes 5)**
    > _Objetivo: Que subir de nivel tenga sentido. El jugador deja de ser genérico y empieza a definir su "Build"._

- [ ] **Fase 6.1 Árbol de Talentos (Talent System)**
    > '- **UI:** Ventana de Talentos (tecla `N`). Árboles visuales con conectores.
    > - **Backend:** Sistema de puntos (1 punto por nivel).
    > - **Efectos:**
    >   - _Pasivos:_ "Aumenta Fuerza en 5%" (Modifica stats base).
    >   - _Activos:_ Desbloquea nuevas habilidades en el `Spellbook`.
    >   - _Modificadores:_ "Tu Bola de Fuego ahora quema" (Cambia la lógica del hechizo).

- [ ] **Fase 6.2 Sistema de Crafting (Artesanía)**
    > '- **Recetas:** ScriptableObjects (`Materiales requeridos` -> `Item resultante`).
    > - **Estaciones de Trabajo:** El jugador debe estar cerca de un objeto "Yunque" o "Mesa de Alquimia" para fabricar.
    > - **Lógica:** Chequeo de inventario -> Consumo de items -> Generación de item nuevo + XP de Crafting.

- [ ] **Fase 6.3 Entrenadores y Libros de Hechizos**
    > '- **Progresión de Skills:** Las habilidades no aparecen solas. Hay que comprarlas con Oro a NPCs instructores según el nivel.
    > - **Spellbook UI:** Ventana para ver skills aprendidas y arrastrarlas a la barra de acción.

- [ ] **Fase 7: Herramientas Sociales y de Grupo (Mes 6)**
    > _Objetivo: Darle al Healer y al Tanque las herramientas para coordinarse. Sin esto, la "Trinidad" es injugable._

- [ ] **Fase 7.1 Marcos de Unidad de Grupo (Party/Raid Frames)**
    > '- **Vital:** El Healer necesita ver las barras de vida de sus 4 compañeros en la pantalla estáticas (no solo sobre sus cabezas en 3D).
    > - **Funcionalidad:** Clic en la barra de vida = Seleccionar a ese compañero (para curarlo).
    > - **Estados:** Mostrar debuffs importantes sobre la barra (ej. "Envenenado" para que el Healer lo limpie).

- [ ] **Fase 7.2 Sistema de Chat y Comunicación**
    > '- **Canales:** General (Zona), Grupo (Solo party), Susurro (Privado).
    > - **Comandos:** `/invitar Nombre`, `/salir`.
    > - **Combat Log:** Ventana de texto que muestra: _"Orco te golpea por 50 daño (Mitigado 10)"_. Esencial para que el Tanque entienda por qué murió.

- [ ] **Fase 7.3 Sistema de Grupo (Party Logic)**
    > '- **Líder de Grupo:** Capacidad de invitar, expulsar y marcar objetivos (poner un icono de "Calavera" sobre un enemigo).
    > - **Compartir Misiones:** Si el líder acepta una misión, se ofrece a los miembros (si cumplen requisitos).

- [ ] **Fase 8 Creación de Contenido y "Level Design" (Mes 7-8)**
    > _Objetivo: Dejar de usar cubos grises y construir el juego real. Esta fase es menos código y más arte/diseño._

- [ ] **Fase 8.1 Pipeline de Biomas**
    > '- Construcción de las 5 regiones (Terreno, Iluminación, Decoración).
    > - Configuración de Spawners de enemigos (Zonas de respawn).

- [ ] **Fase 8.2 Scripting de Encuentros (Jefes)**
    > '- Programar la lógica específica de los 5 Jefes de Mazmorra Grande.
    > - _Ejemplo Boss:_ "Al 50% de vida, se vuelve inmune y convoca 4 esbirros que el Tanque debe agarrar".
    > - Esto requiere extender la IA de la Fase 4 para soportar fases personalizadas.

- [ ] **Fase 8.3 Cinemáticas y Narrativa**
    > '- Diálogos con NPCs (Sistema de Quest Text).
    > - Cinemática de entrada a Bosses (Timeline de Unity).

- [ ] **Fase 9 Pulido, UX y End-Game (Mes 9)**
    > _Objetivo: Convertir el proyecto en un producto profesional listo para Steam._

- [ ] **Fase 9.1 Menús de Configuración (Settings)**
    > '- **Gráficos:** Resolución, Calidad de Sombras, Pantalla completa/Ventana.
    > - **Audio:** Sliders para Música, FX, Voces.
    > - **Keybindings (Reasignación de Teclas):** **Crucial para PC.** Permitir que el jugador cambie `W` por `Z` o sus habilidades

- [ ] **Fase 9.2 Audio Implementation**
    > '- Integrar música adaptativa (Cambia al entrar en combate).
    > - Sonidos de impacto, pasos, UI, ambiente.

- [ ] **Fase 9.3 El "True Form" (End-Game)**
    > '- Implementar el modo "New Game+" o Dificultad Heroica para las mazmorras existentes (Escalado numérico de stats).

- [ ] **Fase 10: Lanzamiento y Operaciones (Mes 10+)**
    > '- Integración con Steamworks (Logros, Amigos de Steam).
    > - Bug fixing masivo (Beta cerrada).
    > - Optimización de red (Reducir el ancho de banda).


## Terminado

- [x] **Fase 1: El Esqueleto de Red y Movimiento (Semana 1-2)**
    > _Objetivo: Un cubo se mueve en la pantalla de dos jugadores distintos y se ven mutuamente._
    > Antes de hacer hechizos o inventarios, necesitamos que el multijugador funcione.

- [x] **Fase 1.1 Configuración de Netcode for GameObjects (NGO):**
    > '- Instalar el paquete `com.unity.netcode.gameobjects`.
    > - Crear el `NetworkManager`.
    > - Configurar el transporte (recomiendo **Unity Transport**).
    > - _Meta:_ Lograr conectar un "Host" y un "Cliente" en local.

- [x] **Fase 1.2 Controlador de Personaje Básico (Greybox)**
    > '- Usar un modelo cápsula.
    > - Implementar movimiento en tercera persona (WASD + Salto).
    > - Cámara (Cinemachine es ideal aquí).
    > - Sincronización de red: `NetworkTransform` para que los jugadores vean moverse a los otros.

- [x] **Fase 2.1 Estructura de Datos (ScriptableObjects & Classes):**
    > '- Definir la clase `CharacterData` (Nivel, XP, Clase, InventarioIDs).
    > - Definir las `BaseStats` (Fuerza, Intelecto, Aguante) usando ScriptableObjects para configurar las clases (Guerrero/Mago).


