using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PolarityBreach.Menus
{
    public class LoadPrefs : MonoBehaviour
    {
        [Header("General Settings")] 
        [SerializeField] private bool canUse = false;
        [SerializeField] private MenuController menuController;
        
    }
}
