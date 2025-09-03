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

        private bool isInitialised = false;

        public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
        {
            DynamicProperties props = _featureData.Props;
            props.ParseInt(PropLockDifficulty, ref lockDifficulty);

            base.Init(_parent, _featureData);
        }

        public override void SetBlockEntityData(BlockEntityData _blockEntityData)
        {
            base.SetBlockEntityData(_blockEntityData);
            Initialize();
        }

        public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode, int _readVersion)
        {
            base.Read(_br, _eStreamMode, _readVersion);
            isInitialised = _br.ReadBoolean();
        }

        public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
        {
            base.Write(_bw, _eStreamMode);
            _bw.Write(isInitialised);
        }

        public new bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
        {
            return IsOwner(_userIdentifier);
        }

        private void Initialize()
        {
            if (isInitialised || LocalPlayerIsOwner())
            {
                return;
            }

            isInitialised = true;
            SetLocked(true);
        }
    }
}
