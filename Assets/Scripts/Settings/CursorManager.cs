using UnityEngine;

namespace PolarityBreach.Settings
{
    public class CursorManager : MonoBehaviour
    {
        public enum CursorType
        {
            Menu,
            Gameplay
        }

        private static CursorManager instance;
        private static bool gameplayCursorVisible = true;

        [SerializeField] private CursorType cursorOnStart = CursorType.Gameplay;
        private CursorType currentCursorType;

        [Header("Menu Cursor")]
        [SerializeField] private Texture2D menuCursor;
        [SerializeField] private Vector2 menuCursorHotspot = Vector2.zero;
        [SerializeField] private CursorMode menuCursorMode = CursorMode.Auto;

        [Header("Gameplay Cursor")]
        [SerializeField] private Texture2D gameplayCursor;
        [SerializeField] private Vector2 gameplayCursorHotspot = new Vector2(16f, 16f);
        [SerializeField] private CursorMode gameplayCursorMode = CursorMode.Auto;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            ApplyCursor(cursorOnStart);
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static void ShowGameplayCursor()
        {
            ShowCursor(CursorType.Gameplay);
        }

        public static void ShowMenuCursor()
        {
            ShowCursor(CursorType.Menu);
        }

        public static void SetGameplayCursorVisible(bool visible)
        {
            gameplayCursorVisible = visible;

            if (instance != null && instance.currentCursorType == CursorType.Gameplay)
                instance.ApplyCursor(CursorType.Gameplay);
        }

        private static void ShowCursor(CursorType cursorType)
        {
            if (instance != null)
            {
                instance.ApplyCursor(cursorType);
                return;
            }

            Cursor.visible = cursorType != CursorType.Gameplay || gameplayCursorVisible;
            Cursor.lockState = cursorType == CursorType.Gameplay ? CursorLockMode.Confined : CursorLockMode.None;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private void ApplyCursor(CursorType cursorType)
        {
            currentCursorType = cursorType;
            Cursor.visible = cursorType != CursorType.Gameplay || gameplayCursorVisible;
            Cursor.lockState = cursorType == CursorType.Gameplay ? CursorLockMode.Confined : CursorLockMode.None;

            if (cursorType == CursorType.Gameplay)
            {
                Cursor.SetCursor(gameplayCursor, gameplayCursorHotspot, gameplayCursorMode);
            }
            else
            {
                Cursor.SetCursor(menuCursor, menuCursorHotspot, menuCursorMode);
            }
        }
    }
}
