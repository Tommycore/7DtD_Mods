namespace TEFeatures.Interfaces
{
    public interface ILockableTC : ILockable
    {
        int LockDifficulty { get; }
        void InitializeLockStatus(float lockChance);
    }
}
