using Antlr.Runtime;
using TEFeatures.Interfaces;

namespace TEFeatures
{
    public class TEFeatureLockableTC : TEFeatureLockable, ILockableTC
    {
        public static string PropLockDifficulty => "LockDifficulty";

        public int LockDifficulty
        {
            get { return lockDifficulty; }
            private set { lockDifficulty = value; }
        }

        private int lockDifficulty = 3;

        public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
        {
            DynamicProperties props = _featureData.Props;
            props.ParseInt(PropLockDifficulty, ref lockDifficulty);

            base.Init(_parent, _featureData);
        }

        public override void SetBlockEntityData(BlockEntityData _blockEntityData)
        {
            base.SetBlockEntityData(_blockEntityData);
        }

        /// <summary>
        /// Dumbed down version of the hidden method.
        /// </summary>
        public new bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
        {
            return IsOwner(_userIdentifier);
        }

        public void InitializeLockStatus(float lockChance)
        {
            SetLocked(UnityEngine.Random.value < lockChance);
        }
    }
}
