using System;
using _SLIME.BaseScripts;
using UnityEngine;

namespace _SLIME.Utils
{
    public class CursorHider: ProjectMonoBehavior
    {
        private void Awake()
        {
        #if !UNITY_EDITOR
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked; // optional, but common
        #endif
        }

        private void OnDestroy()
        {
            Cursor.visible = true;
        }
    }
}