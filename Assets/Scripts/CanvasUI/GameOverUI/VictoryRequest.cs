using System.Collections;

namespace PolarityBreach.UI
{
    public class VictoryRequest : IUIRequest
    {
        private readonly VictoryScreen screen;
        private readonly float runSeconds;

        public VictoryRequest(VictoryScreen screen, float runSeconds)
        {
            this.screen = screen;
            this.runSeconds = runSeconds;
        }

        public IEnumerator Show()
        {
            yield return screen.PlaySequence(runSeconds);
        }
    }
}