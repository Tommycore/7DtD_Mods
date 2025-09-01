using System;
using System.Runtime.CompilerServices;

namespace TEFeatures
{
    public class TEFeatureLoggable : TEFeatureAbs
    {
        public override int EntityId
        {
            get
            {
                return Parent?.EntityId ?? -1;
            }

            set
            {
            }
        }

        private string guid;

        public TEFeatureLoggable()
        {
            guid = Guid.NewGuid().ToString();
            LogMessage();
        }

        public override void CopyFrom(TileEntityComposite _other)
        {
            LogMessage();
            base.CopyFrom(_other);
        }

        public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
        {
            LogMessage();
            return base.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
        }

        public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
        {
            LogMessage();
            base.Init(_parent, _featureData);
        }

        public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
        {
            LogMessage();
            base.InitBlockActivationCommands(_addCallback);
        }

        public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
        {
            LogMessage();
            return base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
        }

        public override void OnDestroy()
        {
            LogMessage();
            base.OnDestroy();
        }

        public override void OnUnload(World _world)
        {
            LogMessage();
            base.OnUnload(_world);
        }

        public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _placingEntity)
        {
            LogMessage();
            base.PlaceBlock(_world, _result, _placingEntity);
        }

        public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode, int _readVersion)
        {
            LogMessage();
            base.Read(_br, _eStreamMode, _readVersion);
        }

        public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
        {
            LogMessage();
            base.ReplacedBy(_bvOld, _bvNew, _teNew);
        }

        public override void Reset(FastTags<TagGroup.Global> _questTags)
        {
            LogMessage();
            base.Reset(_questTags);
        }

        public override void SetBlockEntityData(BlockEntityData _blockEntityData)
        {
            LogMessage();
            base.SetBlockEntityData(_blockEntityData);
        }

        public override void UpdateBlockActivationCommands(ref BlockActivationCommand _command, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
        {
            LogMessage();
            base.UpdateBlockActivationCommands(ref _command, _commandName, _world, _blockPos, _blockValue, _entityFocusing);
        }

        public override void UpdateTick(World _world)
        {
            //LogMessage();
            base.UpdateTick(_world);
        }

        public override void UpgradeDowngradeFrom(TileEntityComposite _other)
        {
            LogMessage();
            base.UpgradeDowngradeFrom(_other);
        }

        public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
        {
            LogMessage();
            base.Write(_bw, _eStreamMode);
        }

        private void LogMessage(string message = "", [CallerMemberName] string from = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                Log.Out($"[TC-DT] - TEFLog - {from} (EID: {EntityId} - guid: {guid})");
            }
            else
            {
                Log.Out($"[TC-DT] - TEFLog - {from}: {message} (EID: {EntityId} - guid: {guid})");
            }
        }
    }
}
