using UnityEngine;

namespace PolarityBreach.PolaritySystem
{
    [RequireComponent(typeof(PolarityComponent))]
    public class BulletPolarityVisual : MonoBehaviour
    {
        [Header("Mesh")]
        [SerializeField] private Renderer[] meshRenderers;
        [SerializeField] private string meshColorProperty = "_BaseColor";

        [Header("Trail")]
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private string trailColorProperty = "Color";
        [SerializeField] private string trailColor2Property = "Color2";

        [Header("Particles")]
        [SerializeField] private ParticleSystem[] particles;

        [Header("Colors")]
        [SerializeField] private Color whiteColor = Color.white;
        [SerializeField] private Color blackColor = Color.black;
        [SerializeField] private float hdrIntensity = 2f;

        private PolarityComponent polarity;

        private void Awake()
        {
            polarity = GetComponent<PolarityComponent>();
        }

        private void OnEnable()
        {
            polarity.OnPolarityChanged += Apply;
            Apply(polarity.CurrentPolarity);
        }

        private void OnDisable()
        {
            polarity.OnPolarityChanged -= Apply;
        }

        private void Apply(Polarity value)
        {
            Color color = value == Polarity.Black ? blackColor : whiteColor;
            Color hdr = color * hdrIntensity;

            ApplyToMesh(color);
            ApplyToTrail(hdr);
            ApplyToParticles(color);
        }

        private void ApplyToMesh(Color color)
        {
            if (meshRenderers == null) return;

            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if (meshRenderers[i] == null) continue;

                Material[] mats = meshRenderers[i].materials;
                for (int m = 0; m < mats.Length; m++)
                {
                    if (mats[m].HasProperty(meshColorProperty))
                        mats[m].SetColor(meshColorProperty, color);
                }
            }
        }

        private void ApplyToTrail(Color color)
        {
            if (trail == null) return;

            Material mat = trail.material;

            if (mat.HasProperty(trailColorProperty))
                mat.SetColor(trailColorProperty, color);

            if (mat.HasProperty(trailColor2Property))
                mat.SetColor(trailColor2Property, color);
        }

        private void ApplyToParticles(Color color)
        {
            if (particles == null) return;

            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i] == null) continue;

                ParticleSystem.MainModule main = particles[i].main;
                main.startColor = color;
            }
        }
    }
}