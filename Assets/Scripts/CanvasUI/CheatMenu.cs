using PolarityBreach.Enemy;
using PolarityBreach.Player;
using PolarityBreach.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PolarityBreach
{
    public class CheatMenu : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerStatsData playerStats;
        [SerializeField] private EnemyWaveSpawner waveSpawner;
        [SerializeField] private EnemyWaveSpawner bossSpawner;

        [Header("Boss Debug")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform bossRoomPoint;
        [SerializeField] private GameObject bossObject;

        [Header("Window")]
        [SerializeField] private float windowWidth = 300f;
        [SerializeField] private float windowHeight = 600f;

        [Header("Gamepad Navigation")]
        [Tooltip("Slider range is divided into this many steps. One D-Pad press = one step.")]
        [SerializeField] private int sliderSteps = 20;

        private bool showMenu;
        private Vector2 scrollPosition;

        private GUIStyle boldStyle;
        private GUIStyle selectedStyle;

        private int selectedIndex;
        private int itemCount;
        private int currentIndex;

        private Rect selectedRect;
        private bool selectedRectValid;
        private float scrollAreaHeight;

        private int pendingSliderDir;

        private bool navigatedWithGamepad;

        private void Awake()
        {
            if (playerStats == null)
                Debug.LogWarning("CheatMenu: playerStats is not assigned.");

            if (bossObject != null)
                bossObject.SetActive(false);
        }

        private void Update()
        {
            bool keyboardToggle = Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame;
            bool gamepadToggle = Gamepad.current != null && Gamepad.current.selectButton.wasPressedThisFrame;

            if (keyboardToggle || gamepadToggle)
                ToggleMenu();

            if (showMenu)
                HandleNavigation();
        }

        private void HandleNavigation()
        {
            Gamepad pad = Gamepad.current;
            if (pad == null) return;

            if (pad.dpad.up.wasPressedThisFrame)
            {
                selectedIndex = Mathf.Max(0, selectedIndex - 1);
                navigatedWithGamepad = true;
            }

            if (pad.dpad.down.wasPressedThisFrame)
            {
                selectedIndex = Mathf.Min(Mathf.Max(0, itemCount - 1), selectedIndex + 1);
                navigatedWithGamepad = true;
            }

            pendingSliderDir = 0;
            if (pad.dpad.right.wasPressedThisFrame) pendingSliderDir = 1;
            if (pad.dpad.left.wasPressedThisFrame) pendingSliderDir = -1;

            if (pad.buttonSouth.wasPressedThisFrame)
                Confirm(selectedIndex);
        }

        private void Confirm(int index)
        {
            switch (index)
            {
                case 2: playerStats.godMode = !playerStats.godMode; break;
                case 4: playerStats.dashUnlocked = !playerStats.dashUnlocked; break;
                case 12: playerStats.chargeShotUnlocked = !playerStats.chargeShotUnlocked; break;
                case 17: KillAllEnemies(); break;
                case 18: GoToBossFight(); break;
            }
        }

        private void GoToBossFight()
        {
            EnemyWaveSpawner[] spawners = FindObjectsByType<EnemyWaveSpawner>(FindObjectsSortMode.None);

            for (int i = 0; i < spawners.Length; i++)
            {
                if (bossObject != null && spawners[i].transform.IsChildOf(bossObject.transform))
                    continue;

                spawners[i].DebugStopAndClearEnemies();
            }

            if (WaveWarningUI.Instance != null)
                WaveWarningUI.Instance.HideNow();

            if (player != null && bossRoomPoint != null)
                StartCoroutine(TeleportRoutine());
            else if (bossObject != null)
                bossObject.SetActive(true);
        }

        private System.Collections.IEnumerator TeleportRoutine()
        {
            GameObject playerObj = player.gameObject;

            playerObj.SetActive(false);

            yield return new WaitForSecondsRealtime(0.1f);

            player.position = bossRoomPoint.position;

            Rigidbody rb = playerObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.position = bossRoomPoint.position;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            playerObj.SetActive(true);

            if (bossObject != null)
                bossObject.SetActive(true);
        }

        private void OnGUI()
        {
            if (!showMenu || playerStats == null) return;

            BuildStyles();
            currentIndex = 0;

            GUILayout.BeginArea(new Rect(20, 20, windowWidth, windowHeight), "Debug Menu", GUI.skin.window);
            GUILayout.Space(20);

            scrollAreaHeight = windowHeight - 40f;
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Player Stats", boldStyle);
            playerStats.movementSpeed = NavSlider("Movement Speed", playerStats.movementSpeed, 0f, 20f);        // 0
            playerStats.maxHealth = NavSlider("Max Health", playerStats.maxHealth, 1f, 300f);                   // 1
            playerStats.godMode = NavToggle(playerStats.godMode, "God Mode");                                   // 2
            playerStats.polaritySwitchCooldown = NavSlider("Polarity Cooldown", playerStats.polaritySwitchCooldown, 0f, 5f); // 3

            GUILayout.Space(20);

            GUILayout.Label("Dash", boldStyle);
            playerStats.dashUnlocked = NavToggle(playerStats.dashUnlocked, "Dash Unlocked");                    // 4
            playerStats.dashSpeed = NavSlider("Dash Speed", playerStats.dashSpeed, 0f, 60f);                    // 5
            playerStats.dashDuration = NavSlider("Dash Duration", playerStats.dashDuration, 0f, 2f);            // 6
            playerStats.dashCooldown = NavSlider("Dash Cooldown", playerStats.dashCooldown, 0f, 5f);            // 7

            GUILayout.Space(20);

            GUILayout.Label("Normal Shot", boldStyle);
            playerStats.attackSpeedDelay = NavSlider("Attack Speed Delay", playerStats.attackSpeedDelay, 0.01f, 3f); // 8
            playerStats.attackDamage = NavSlider("Attack Damage", playerStats.attackDamage, 0f, 100f);          // 9
            playerStats.attackSpeed = NavSlider("Projectile Speed", playerStats.attackSpeed, 0f, 200f);         // 10
            playerStats.knockBackPower = NavSlider("Knockback", playerStats.knockBackPower, 0f, 100f);          // 11

            GUILayout.Space(20);

            GUILayout.Label("Charge Shot", boldStyle);
            playerStats.chargeShotUnlocked = NavToggle(playerStats.chargeShotUnlocked, "Charge Shot Unlocked"); // 12
            playerStats.chargeShotDamage = NavSlider("Charge Damage", playerStats.chargeShotDamage, 0f, 300f);  // 13
            playerStats.chargeShotSpeed = NavSlider("Charge Speed", playerStats.chargeShotSpeed, 0f, 50f);      // 14
            playerStats.chargeShotKnockBackPower = NavSlider("Charge Knockback", playerStats.chargeShotKnockBackPower, 0f, 200f); // 15
            playerStats.chargeTime = NavSlider("Charge Time", playerStats.chargeTime, 0f, 5f);                  // 16

            GUILayout.Space(20);

            GUILayout.Label("Wave Debug", boldStyle);
            if (NavButton("Kill all enemies"))                                           // 17
                KillAllEnemies();

            GUILayout.Space(10);

            GUILayout.Label("Boss Debug", boldStyle);
            if (NavButton("Go to Boss Fight"))                                                                  // 18
                GoToBossFight();

            GUILayout.Space(10);

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            itemCount = currentIndex;
            FollowSelectionWithScroll();

            if (Event.current.type == EventType.Repaint)
                pendingSliderDir = 0;
        }

        private void BuildStyles()
        {
            if (boldStyle != null) return;

            boldStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };

            Texture2D highlight = new Texture2D(1, 1);
            highlight.SetPixel(0, 0, new Color(1f, 1f, 1f, 0.22f));
            highlight.Apply();

            selectedStyle = new GUIStyle(GUI.skin.box);
            selectedStyle.normal.background = highlight;
        }

        private void FollowSelectionWithScroll()
        {
            if (!navigatedWithGamepad) return;
            if (!selectedRectValid || Event.current.type != EventType.Repaint) return;

            float itemTop = selectedRect.y;
            float itemBottom = selectedRect.y + selectedRect.height;

            if (itemTop < scrollPosition.y)
                scrollPosition.y = itemTop - 10f;
            else if (itemBottom > scrollPosition.y + scrollAreaHeight)
                scrollPosition.y = itemBottom - scrollAreaHeight + 10f;

            scrollPosition.y = Mathf.Max(0f, scrollPosition.y);
            selectedRectValid = false;
            navigatedWithGamepad = false;
        }

        private bool IsSelected() => currentIndex == selectedIndex;

        private void CaptureSelectedRect(bool selected)
        {
            if (!selected || Event.current.type != EventType.Repaint) return;
            selectedRect = GUILayoutUtility.GetLastRect();
            selectedRectValid = true;
        }
        private void CheckMouseHover(int myIndex)
        {
            if (Event.current.type != EventType.Repaint) return;

            Rect r = GUILayoutUtility.GetLastRect();
            if (r.Contains(Event.current.mousePosition))
                selectedIndex = myIndex;
        }


        private float NavSlider(string label, float value, float min, float max)
        {
            int myIndex = currentIndex;
            bool selected = IsSelected();

            if (selected) GUILayout.BeginVertical(selectedStyle);

            GUILayout.Label((selected ? "> " : "   ") + label + ": " + value.ToString("0.00"));
            value = GUILayout.HorizontalSlider(value, min, max);

            if (selected && pendingSliderDir != 0)
            {
                float step = (max - min) / Mathf.Max(1, sliderSteps);
                value = Mathf.Clamp(value + step * pendingSliderDir, min, max);
            }

            if (selected) GUILayout.EndVertical();

            CheckMouseHover(myIndex);
            CaptureSelectedRect(selected);
            currentIndex++;
            return value;
        }

        private bool NavToggle(bool value, string label)
        {
            int myIndex = currentIndex;
            bool selected = IsSelected();

            if (selected) GUILayout.BeginVertical(selectedStyle);

            value = GUILayout.Toggle(value, (selected ? "> " : "   ") + label);

            if (selected) GUILayout.EndVertical();

            CheckMouseHover(myIndex);
            CaptureSelectedRect(selected);
            currentIndex++;
            return value;
        }

        private bool NavButton(string label)
        {
            int myIndex = currentIndex;
            bool selected = IsSelected();
            bool pressed = false;

            if (selected) GUILayout.BeginVertical(selectedStyle);

            if (GUILayout.Button((selected ? "> " : "   ") + label))
                pressed = true;

            if (selected) GUILayout.EndVertical();

            CheckMouseHover(myIndex);
            CaptureSelectedRect(selected);
            currentIndex++;
            return pressed;
        }

        public void ToggleMenu() => showMenu = !showMenu;
        public void OpenMenu() => showMenu = true;
        public void CloseMenu() => showMenu = false;

        private void KillAllEnemies()
        {
            EnemyWaveSpawner[] spawners = FindObjectsByType<EnemyWaveSpawner>(FindObjectsSortMode.None);

            foreach (EnemyWaveSpawner spawner in spawners)
                spawner.DebugCompleteCurrentWave();
        }
    }
}
