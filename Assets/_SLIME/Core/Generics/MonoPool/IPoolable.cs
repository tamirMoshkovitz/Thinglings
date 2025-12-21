namespace _SLIME.Generics
{
    /// <summary>
    /// Interface for objects that can be pooled and reset to an initial state.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Resets the object to its initial state for reuse in a pool.
        /// </summary>
        void Reset();
    }
}