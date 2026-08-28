using System;
using System.Collections;

namespace PolarityBreach.UI
{
    public class DialogueRequest : IUIRequest
    {
        private readonly DialogueUI ui;
        private readonly DialogueSequence sequence;
        private readonly Action onFinished;

        public DialogueRequest(DialogueUI ui, DialogueSequence sequence, Action onFinished = null)
        {
            this.ui = ui;
            this.sequence = sequence;
            this.onFinished = onFinished;
        }

        public IEnumerator Show()
        {
            yield return ui.PlaySequence(sequence);
            onFinished?.Invoke();
        }
    }
}
