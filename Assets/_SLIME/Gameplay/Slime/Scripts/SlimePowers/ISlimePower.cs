using UnityEngine;

namespace _SLIME.Slime
{
    public interface ISlimePower
    {
        public void Activate(Vector2 hitPoint, Collider2D collider2D);
        public void Update();
        public void Deactivate();
    }
}