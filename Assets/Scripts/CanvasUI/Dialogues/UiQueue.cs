using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolarityBreach.UI
{
    public class UIQueue : MonoBehaviour
    {
        public static UIQueue Instance { get; private set; }
        public static bool IsBlocking { get; private set; }
        public static event Action<bool> OnBlockingChanged;

        private readonly Queue<IUIRequest> requests = new Queue<IUIRequest>();
        private bool isProcessing;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                IsBlocking = false;
            }
        }

        public void Enqueue(IUIRequest request)
        {
            if (request == null) return;

            requests.Enqueue(request);

            if (!isProcessing)
                StartCoroutine(ProcessQueue());
        }

        private IEnumerator ProcessQueue()
        {
            isProcessing = true;
            SetBlocking(true);

            while (requests.Count > 0)
            {
                IUIRequest request = requests.Dequeue();
                yield return request.Show();
            }

            SetBlocking(false);
            isProcessing = false;
        }

        private void SetBlocking(bool blocking)
        {
            IsBlocking = blocking;
            Time.timeScale = blocking ? 0f : 1f;
            OnBlockingChanged?.Invoke(blocking);
        }
    }
}