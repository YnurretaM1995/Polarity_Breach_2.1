using System.Collections;

namespace PolarityBreach.UI
{
    public class DialogueRequest : IUIRequest
    {
        private readonly DialogueUI ui;
        private readonly DialogueSequence sequence;

        public DialogueRequest(DialogueUI ui, DialogueSequence sequence)
        {
            this.ui = ui;
            this.sequence = sequence;
        }

        public IEnumerator Show()
        {
            yield return ui.PlaySequence(sequence);
        }
    }
}