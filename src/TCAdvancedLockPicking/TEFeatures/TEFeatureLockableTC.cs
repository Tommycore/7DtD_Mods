using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEFeatures
{
    public class TEFeatureLockableTC : TEFeatureLockable
    {
        private bool isInitialised = false;

        public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
        {
            base.Init(_parent, _featureData);
        }

        public override void SetBlockEntityData(BlockEntityData _blockEntityData)
        {
            Log.Out("[TC-ALP] SetBlockEntityData");
            base.SetBlockEntityData(_blockEntityData);
            Initialize();
        }

        public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode, int _readVersion)
        {
            Log.Out("[TC-ALP] Read");
            base.Read(_br, _eStreamMode, _readVersion);
            isInitialised = _br.ReadBoolean();
        }

        public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
        {
            Log.Out("[TC-ALP] Write");
            base.Write(_bw, _eStreamMode);
            _bw.Write(isInitialised);
        }

        private void Initialize()
        {
            if (isInitialised || LocalPlayerIsOwner())
            {
                return;
            }

            Log.Out("[TC-ALP] Locking");
            isInitialised = true;
            SetLocked(true);
        }
    }
}
