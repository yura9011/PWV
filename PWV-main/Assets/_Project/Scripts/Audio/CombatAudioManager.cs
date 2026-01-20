using UnityEngine;

namespace EtherDomes.Audio
{
    /// <summary>
    /// Gestor de audio para efectos de combate.
    /// Reproduce sonidos de ataques, impactos y otros eventos de combate.
    /// </summary>
    public class CombatAudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _attackAudioSource;
        [SerializeField] private AudioSource _impactAudioSource;
        
        [Header("Attack Sounds")]
        [SerializeField] private AudioClip _basicAttackSound;
        [SerializeField] private AudioClip _heavyAttackSound;
        [SerializeField] private AudioClip _rangedAttackSound;
        
        [Header("Impact Sounds")]
        [SerializeField] private AudioClip _hitSound;
        [SerializeField] private AudioClip _criticalHitSound;
        [SerializeField] private AudioClip _deathSound;
        
        [Header("Volume Settings")]
        [SerializeField] private float _attackVolume = 0.7f;
        [SerializeField] private float _impactVolume = 0.8f;
        
        private static CombatAudioManager _instance;
        public static CombatAudioManager Instance => _instance;
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                SetupAudioSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void SetupAudioSources()
        {
            // Crear AudioSource para ataques si no existe
            if (_attackAudioSource == null)
            {
                _attackAudioSource = gameObject.AddComponent<AudioSource>();
                _attackAudioSource.volume = _attackVolume;
                _attackAudioSource.spatialBlend = 0f; // 2D sound
            }
            
            // Crear AudioSource para impactos si no existe
            if (_impactAudioSource == null)
            {
                _impactAudioSource = gameObject.AddComponent<AudioSource>();
                _impactAudioSource.volume = _impactVolume;
                _impactAudioSource.spatialBlend = 0f; // 2D sound
            }
        }
        
        /// <summary>
        /// Reproduce sonido de ataque básico
        /// </summary>
        public void PlayBasicAttack()
        {
            if (_basicAttackSound != null && _attackAudioSource != null)
            {
                _attackAudioSource.PlayOneShot(_basicAttackSound);
            }
            else
            {
                // Sonido sintético si no hay clip
                PlaySyntheticSound(_attackAudioSource, 440f, 0.1f); // A4 note
            }
        }
        
        /// <summary>
        /// Reproduce sonido de ataque pesado
        /// </summary>
        public void PlayHeavyAttack()
        {
            if (_heavyAttackSound != null && _attackAudioSource != null)
            {
                _attackAudioSource.PlayOneShot(_heavyAttackSound);
            }
            else
            {
                // Sonido sintético más grave para ataque pesado
                PlaySyntheticSound(_attackAudioSource, 220f, 0.2f); // A3 note, longer
            }
        }
        
        /// <summary>
        /// Reproduce sonido de ataque ranged
        /// </summary>
        public void PlayRangedAttack()
        {
            if (_rangedAttackSound != null && _attackAudioSource != null)
            {
                _attackAudioSource.PlayOneShot(_rangedAttackSound);
            }
            else
            {
                // Sonido sintético agudo para ranged
                PlaySyntheticSound(_attackAudioSource, 880f, 0.15f); // A5 note
            }
        }
        
        /// <summary>
        /// Reproduce sonido de impacto normal
        /// </summary>
        public void PlayHit()
        {
            if (_hitSound != null && _impactAudioSource != null)
            {
                _impactAudioSource.PlayOneShot(_hitSound);
            }
            else
            {
                // Sonido sintético de impacto
                PlaySyntheticSound(_impactAudioSource, 150f, 0.05f); // Low thud
            }
        }
        
        /// <summary>
        /// Reproduce sonido de impacto crítico
        /// </summary>
        public void PlayCriticalHit()
        {
            if (_criticalHitSound != null && _impactAudioSource != null)
            {
                _impactAudioSource.PlayOneShot(_criticalHitSound);
            }
            else
            {
                // Sonido sintético más intenso para crítico
                PlaySyntheticSound(_impactAudioSource, 200f, 0.1f);
            }
        }
        
        /// <summary>
        /// Reproduce sonido de muerte
        /// </summary>
        public void PlayDeath()
        {
            if (_deathSound != null && _impactAudioSource != null)
            {
                _impactAudioSource.PlayOneShot(_deathSound);
            }
            else
            {
                // Sonido sintético de muerte (descending tone)
                StartCoroutine(PlayDeathSequence());
            }
        }
        
        /// <summary>
        /// Genera un sonido sintético simple
        /// </summary>
        private void PlaySyntheticSound(AudioSource source, float frequency, float duration)
        {
            if (source == null) return;
            
            // Crear un AudioClip sintético simple
            int sampleRate = 44100;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] audioData = new float[samples];
            
            for (int i = 0; i < samples; i++)
            {
                float time = (float)i / sampleRate;
                float envelope = Mathf.Exp(-time * 5f); // Decay envelope
                audioData[i] = Mathf.Sin(2 * Mathf.PI * frequency * time) * envelope * 0.3f;
            }
            
            AudioClip clip = AudioClip.Create("SyntheticSound", samples, 1, sampleRate, false);
            clip.SetData(audioData, 0);
            
            source.PlayOneShot(clip);
            
            // Destruir el clip después de reproducirlo
            Destroy(clip, duration + 0.1f);
        }
        
        /// <summary>
        /// Secuencia de sonido de muerte
        /// </summary>
        private System.Collections.IEnumerator PlayDeathSequence()
        {
            // Tono descendente para simular muerte
            float startFreq = 300f;
            float endFreq = 100f;
            float duration = 0.5f;
            int steps = 10;
            
            for (int i = 0; i < steps; i++)
            {
                float progress = (float)i / (steps - 1);
                float freq = Mathf.Lerp(startFreq, endFreq, progress);
                PlaySyntheticSound(_impactAudioSource, freq, duration / steps);
                yield return new WaitForSeconds(duration / steps);
            }
        }
    }
}