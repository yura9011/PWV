# Reglas para Kiro - The Ether Domes

## REGLAS CRÍTICAS - NUNCA OLVIDAR

### 1. CONSULTAR SIEMPRE
- **NUNCA asumir nada**
- **PREGUNTAR** antes de cualquier acción
- **CONFIRMAR** antes de crear, modificar o eliminar elementos
- **VERIFICAR** qué existe antes de proponer cambios

### 2. PRESERVAR CONFIGURACIONES EXISTENTES
- **NUNCA borrar** elementos sin autorización explícita del usuario
- **PRESERVAR** toda configuración específica de cada escena
- **RESPETAR** elementos únicos por escena:
  - Portales (diferentes destinos)
  - Enemigos (diferentes tipos y posiciones)
  - NPCs específicos
  - Entornos únicos
  - Modelos específicos de cada área

### 3. PROTOCOLO DE TRABAJO
- Antes de modificar cualquier escena → **VERIFICAR** qué ya existe
- Solo **AGREGAR** elementos faltantes sin tocar lo existente
- **NUNCA recrear** desde cero elementos que ya funcionan
- **GUARDAR** inmediatamente después de cada cambio
- **DOCUMENTAR** todos los cambios realizados

### 4. GESTIÓN DE ESCENAS
- **NO cambiar** entre escenas sin consultar
- **NO asumir** que una configuración de una escena aplica a otra
- **TRABAJAR** en la escena que el usuario indique
- **MANTENER** consistencia solo en elementos que deben ser iguales

### 5. ELEMENTOS CONSISTENTES VS ÚNICOS

#### Elementos que DEBEN ser consistentes:
- TestPlayer (mismo comportamiento)
- Cámara (mismos controles)
- AttackEffects (mismo sistema visual)
- Sistemas de combate básicos

#### Elementos que son ÚNICOS por escena:
- Portales y sus destinos
- Enemigos específicos
- NPCs y diálogos
- Decoración y entorno
- Configuraciones específicas del área

### 6. COMUNICACIÓN
- **EXPLICAR** qué voy a hacer antes de hacerlo
- **CONFIRMAR** que el usuario está de acuerdo
- **REPORTAR** qué se hizo después de completar
- **ADMITIR** cuando no estoy seguro de algo

## RECORDATORIO PARA NUEVAS SESIONES

**Al inicio de cada nueva sesión, el usuario debe recordarme estas reglas ya que no las recordaré automáticamente.**

**Frase clave:** "Recuerda las reglas de docs/REGLAS_KIRO.md"

---

**Fecha de creación:** 2025-01-17  
**Autor:** Kiro (por instrucción del usuario)  
**Propósito:** Evitar pérdida de trabajo y configuraciones en el proyecto