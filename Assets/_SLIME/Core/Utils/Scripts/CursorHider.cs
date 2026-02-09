using System;
using _SLIME.BaseScripts;
using UnityEngine;

namespace _SLIME.Utils
{
    public class CursorHider: ProjectMonoBehavior
    {
        private void Awake()
        {
            Cursor.visible = false;
        }

        private void OnDestroy()
        {
            Cursor.visible = true;
        }
    }
}