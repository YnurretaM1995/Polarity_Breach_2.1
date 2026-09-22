using UnityEngine;

namespace PolarityBreach
{
    public class CheatMenuGUI : MonoBehaviour
    {
        private CheatMenu owner;

        public void Initialize(CheatMenu cheatMenu)
        {
            owner = cheatMenu;
        }

        private void OnGUI()
        {
            if (owner != null) owner.DrawGUI();
        }
    }
}