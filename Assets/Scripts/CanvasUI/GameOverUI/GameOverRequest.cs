using System.Collections;

namespace PolarityBreach.UI
{
    public class GameOverRequest : IUIRequest
    {
        private readonly GameOverScreen screen;

        public GameOverRequest(GameOverScreen screen)
        {
            this.screen = screen;
        }

        public IEnumerator Show()
        {
            yield return screen.PlaySequence();
        }
    }
}