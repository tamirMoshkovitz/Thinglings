using System.Runtime.InteropServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "UIConfig", menuName = "Scriptable Objects/UIConfig")]
public class UIConfiguration : TabbedScriptableObject
{
    
    [System.Serializable]
    public struct PopUpSettings
    {
        public bool text;
        [ShowIf("text")] 
        public TextMeshProUGUI textPrefab;
        
        public bool moveUp;

        [ShowIf("moveUp")] 
        [Range(0.1F, 2F)] 
        [SerializeField] private float moveUpDistance; 
        
        public bool fadeOut; 

        [ShowIf("fadeOut")]
        [Range(0.1F, 2F)] 
        [SerializeField] private float fadeTime;

        
        [Range(0.1F, 5F)] 
        [SerializeField] public float TimeUntilDispawn;
        
        
        public float GetMoveDistance() => moveUp ? moveUpDistance : 0f;
        public float GetFadeTime() => fadeOut ? fadeTime : 0f;
    }

    [Tab("PopUps")] 
    public PopUpSettings bossHealthPopUp;
    
}
