using UnityEngine;
using UnityEngine.VFX;

namespace PolarityBreach.PolaritySystem
{
    [RequireComponent(typeof(PolarityComponent))]
    public class VfxPolarityColor : MonoBehaviour
    {
        [SerializeField] private VisualEffect[] effects;
        [SerializeField] private string colorProperty = "PolarityColor";

        [Header("Colors")]
        [SerializeField] private Color whiteColor = Color.white;
        [SerializeField] private Color blackColor = Color.black;
        [SerializeField] private float intensity = 1f;

        private PolarityComponent polarity;

        private void Awake()
        {
            polarity = GetComponent<PolarityComponent>();

            if (effects == null || effects.Length == 0)
                effects = GetComponentsInChildren<VisualEffect>();
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
            if (effects == null) return;

            Color color = (value == Polarity.Black ? blackColor : whiteColor) * intensity;

            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i] == null) continue;

                if (effects[i].HasVector4(colorProperty))
                    effects[i].SetVector4(colorProperty, color);
            }
        }
    }
}