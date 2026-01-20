using UnityEngine;
using System.Collections;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Sistema de efectos visuales para ataques en el sistema de testing offline.
    /// Proporciona feedback visual inmediato para ataques básicos, pesados y ranged.
    /// </summary>
    public class AttackEffects : MonoBehaviour
    {
        [Header("Attack Visual Effects")]
        [SerializeField] private GameObject _basicAttackEffect;
        [SerializeField] private GameObject _heavyAttackEffect;
        [SerializeField] private GameObject _rangedAttackEffect;
        
        [Header("Effect Settings")]
        [SerializeField] private float _effectDuration = 1f;
        [SerializeField] private Color _basicAttackColor = Color.yellow;
        [SerializeField] private Color _heavyAttackColor = Color.red;
        [SerializeField] private Color _rangedAttackColor = Color.blue;
        
        private static AttackEffects _instance;
        public static AttackEffects Instance => _instance;
        
        private void Awake()
        {
            _instance = this;
        }
        
        /// <summary>
        /// Reproduce efecto visual de ataque básico
        /// </summary>
        public void PlayBasicAttackEffect(Vector3 attackerPos, Vector3 targetPos)
        {
            StartCoroutine(CreateAttackLine(attackerPos, targetPos, _basicAttackColor, 0.1f));
            CreateImpactEffect(targetPos, _basicAttackColor, 0.5f);
        }
        
        /// <summary>
        /// Reproduce efecto visual de ataque pesado
        /// </summary>
        public void PlayHeavyAttackEffect(Vector3 attackerPos, Vector3 targetPos)
        {
            StartCoroutine(CreateAttackLine(attackerPos, targetPos, _heavyAttackColor, 0.15f));
            CreateImpactEffect(targetPos, _heavyAttackColor, 0.8f);
            
            // Efecto adicional para ataque pesado
            StartCoroutine(CreateShockwave(targetPos, _heavyAttackColor));
        }
        
        /// <summary>
        /// Reproduce efecto visual de ataque ranged
        /// </summary>
        public void PlayRangedAttackEffect(Vector3 attackerPos, Vector3 targetPos)
        {
            StartCoroutine(CreateProjectile(attackerPos, targetPos, _rangedAttackColor));
        }
        
        /// <summary>
        /// Crea una línea visual entre atacante y objetivo
        /// </summary>
        private IEnumerator CreateAttackLine(Vector3 start, Vector3 end, Color color, float width)
        {
            GameObject line = new GameObject("AttackLine");
            LineRenderer lr = line.AddComponent<LineRenderer>();
            
            // Configurar LineRenderer
            lr.material = CreateLineMaterial(color);
            lr.startWidth = width;
            lr.endWidth = width * 0.5f;
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.sortingOrder = 10;
            
            // Ajustar posiciones (desde pecho del atacante hacia centro del objetivo)
            Vector3 adjustedStart = start + Vector3.up * 1.5f;
            Vector3 adjustedEnd = end + Vector3.up * 1f;
            
            lr.SetPosition(0, adjustedStart);
            lr.SetPosition(1, adjustedEnd);
            
            // Fade out
            float elapsed = 0f;
            Color originalColor = color;
            
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / 0.3f);
                lr.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
            
            Destroy(line);
        }
        
        /// <summary>
        /// Crea efecto de impacto en el objetivo
        /// </summary>
        private void CreateImpactEffect(Vector3 position, Color color, float scale)
        {
            GameObject impact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            impact.name = "ImpactEffect";
            impact.transform.position = position + Vector3.up * 1f;
            impact.transform.localScale = Vector3.one * 0.1f;
            
            // Remover collider
            Destroy(impact.GetComponent<Collider>());
            
            // Configurar material
            Renderer renderer = impact.GetComponent<Renderer>();
            renderer.material = CreateEffectMaterial(color);
            renderer.sortingOrder = 15;
            
            StartCoroutine(AnimateImpact(impact, scale));
        }
        
        /// <summary>
        /// Anima el efecto de impacto
        /// </summary>
        private IEnumerator AnimateImpact(GameObject impact, float maxScale)
        {
            float elapsed = 0f;
            float duration = 0.4f;
            Vector3 startScale = Vector3.one * 0.1f;
            Vector3 endScale = Vector3.one * maxScale;
            
            Renderer renderer = impact.GetComponent<Renderer>();
            Color originalColor = renderer.material.color;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                
                // Escalar
                impact.transform.localScale = Vector3.Lerp(startScale, endScale, progress);
                
                // Fade out
                float alpha = Mathf.Lerp(0.8f, 0f, progress);
                renderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                
                yield return null;
            }
            
            Destroy(impact);
        }
        
        /// <summary>
        /// Crea efecto de onda expansiva para ataques pesados
        /// </summary>
        private IEnumerator CreateShockwave(Vector3 position, Color color)
        {
            GameObject shockwave = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shockwave.name = "Shockwave";
            shockwave.transform.position = position;
            shockwave.transform.localScale = new Vector3(0.1f, 0.05f, 0.1f);
            
            // Remover collider
            Destroy(shockwave.GetComponent<Collider>());
            
            // Configurar material
            Renderer renderer = shockwave.GetComponent<Renderer>();
            renderer.material = CreateEffectMaterial(color);
            
            float elapsed = 0f;
            float duration = 0.6f;
            Color originalColor = color;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                
                // Expandir
                float scale = Mathf.Lerp(0.1f, 3f, progress);
                shockwave.transform.localScale = new Vector3(scale, 0.05f, scale);
                
                // Fade out
                float alpha = Mathf.Lerp(0.6f, 0f, progress);
                renderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                
                yield return null;
            }
            
            Destroy(shockwave);
        }
        
        /// <summary>
        /// Crea proyectil visual para ataques ranged
        /// </summary>
        private IEnumerator CreateProjectile(Vector3 start, Vector3 end, Color color)
        {
            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "Projectile";
            projectile.transform.localScale = Vector3.one * 0.2f;
            
            // Remover collider
            Destroy(projectile.GetComponent<Collider>());
            
            // Configurar material
            Renderer renderer = projectile.GetComponent<Renderer>();
            renderer.material = CreateEffectMaterial(color);
            
            // Ajustar posiciones
            Vector3 adjustedStart = start + Vector3.up * 1.5f;
            Vector3 adjustedEnd = end + Vector3.up * 1f;
            
            float elapsed = 0f;
            float duration = 0.3f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                
                projectile.transform.position = Vector3.Lerp(adjustedStart, adjustedEnd, progress);
                yield return null;
            }
            
            // Efecto de impacto al llegar
            CreateImpactEffect(adjustedEnd, color, 0.6f);
            Destroy(projectile);
        }
        
        /// <summary>
        /// Crea material para líneas de ataque
        /// </summary>
        private Material CreateLineMaterial(Color color)
        {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = color;
            return mat;
        }
        
        /// <summary>
        /// Crea material para efectos con transparencia
        /// </summary>
        private Material CreateEffectMaterial(Color color)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 3); // Transparent mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            mat.color = color;
            return mat;
        }
    }
}