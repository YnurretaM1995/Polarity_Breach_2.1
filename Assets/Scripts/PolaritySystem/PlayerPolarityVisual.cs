using System.Collections;
using UnityEngine;

namespace PolarityBreach.PolaritySystem
{
    [RequireComponent(typeof(PolarityComponent))]
    public class PlayerPolarityVisual : MonoBehaviour
    {
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private string invertProperty = "_InvertAmount";
        [SerializeField] private Polarity invertedPolarity = Polarity.Black;
        [SerializeField] private float transitionDuration = 0.15f;

        private PolarityComponent polarity;
        private Coroutine transition;
        private float currentValue;

        private void Awake()
        {
            polarity = GetComponent<PolarityComponent>();

            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>();
        }

        private void OnEnable()
        {
            polarity.OnPolarityChanged += Apply;

            currentValue = GetTarget(polarity.CurrentPolarity);
            SetInvert(currentValue);
        }

        private void OnDisable()
        {
            polarity.OnPolarityChanged -= Apply;
        }

        private void Apply(Polarity newPolarity)
        {
            float target = GetTarget(newPolarity);

            if (transition != null) StopCoroutine(transition);

            if (transitionDuration <= 0f)
            {
                currentValue = target;
                SetInvert(target);
                return;
            }

            transition = StartCoroutine(TransitionRoutine(target));
        }

        private float GetTarget(Polarity value)
        {
            return value == invertedPolarity ? 1f : 0f;
        }

        private IEnumerator TransitionRoutine(float target)
        {
            float start = currentValue;
            float t = 0f;

            while (t < transitionDuration)
            {
                t += Time.deltaTime;
                currentValue = Mathf.Lerp(start, target, t / transitionDuration);
                SetInvert(currentValue);
                yield return null;
            }

            currentValue = target;
            SetInvert(target);
            transition = null;
        }

        private void SetInvert(float value)
        {
            if (renderers == null) return;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;

                Material[] mats = renderers[i].materials;
                for (int m = 0; m < mats.Length; m++)
                {
                    if (mats[m].HasProperty(invertProperty))
                        mats[m].SetFloat(invertProperty, value);
                }
            }
        }
    }
}