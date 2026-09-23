using UnityEngine;

namespace PolarityBreach.PolaritySystem
{
    [RequireComponent(typeof(PolarityComponent))]
    public class TrailPolarityColor : MonoBehaviour
    {
        [Header("Trail")]
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private string colorProperty = "_Color";
        [SerializeField] private float trailIntensity = 1f;

        [Header("Particles")]
        [SerializeField] private ParticleSystem[] particles;
        [SerializeField] private float particleIntensity = 1f;

        [Header("Colors")]
        [SerializeField] private Color whiteColor = Color.white;
        [SerializeField] private Color blackColor = Color.black;

        private PolarityComponent polarity;
        private bool cached;

        private void Awake()
        {
            CacheReferences();
        }

        private void OnEnable()
        {
            CacheReferences();

            if (polarity == null) return;

            polarity.OnPolarityChanged += Apply;
            Apply(polarity.CurrentPolarity);
        }

        private void OnDisable()
        {
            if (polarity != null)
                polarity.OnPolarityChanged -= Apply;
        }

        public void ApplyCurrent()
        {
            CacheReferences();

            if (polarity == null) return;

            Apply(polarity.CurrentPolarity);
        }

        private void CacheReferences()
        {
            if (cached) return;

            polarity = GetComponent<PolarityComponent>();

            if (trail == null)
                trail = GetComponentInChildren<TrailRenderer>();

            if (particles == null || particles.Length == 0)
                particles = GetComponentsInChildren<ParticleSystem>();

            cached = true;
        }

        private void Apply(Polarity value)
        {
            Color color = value == Polarity.Black ? blackColor : whiteColor;

            ApplyToTrail(color);
            ApplyToParticles(color);
        }

        private void ApplyToTrail(Color color)
        {
            if (trail == null) return;

            Material mat = trail.material;

            if (mat.HasProperty(colorProperty))
                mat.SetColor(colorProperty, color * trailIntensity);
        }

        private void ApplyToParticles(Color color)
        {
            if (particles == null) return;

            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i] == null) continue;

                ParticleSystem.MainModule main = particles[i].main;
                main.startColor = color * particleIntensity;
            }
        }
    }
}