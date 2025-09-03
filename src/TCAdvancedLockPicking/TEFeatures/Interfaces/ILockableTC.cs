namespace TEFeatures.Interfaces
{
    public interface ILockableTC : ILockable
    {
        int LockDifficulty { get; }
    }
}
