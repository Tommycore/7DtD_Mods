using System;
using System.Runtime.CompilerServices;

namespace DevTools.TEFeatures
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

        private readonly string guid;

        public TEFeatureLoggable()
        {
            guid = Guid.NewGuid().ToString();
            LogCallMessage();
        }

        public override void CopyFrom(TileEntityComposite _other)
        {
            LogCallMessage();
            base.CopyFrom(_other);
        }

        public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
        {
            LogCallMessage();
            return base.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
        }

        public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
        {
            LogCallMessage();
            base.Init(_parent, _featureData);
        }

        public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
        {
            LogCallMessage();
            base.InitBlockActivationCommands(_addCallback);
        }

        public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
        {
            LogCallMessage();
            return base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
        }

        public override void OnDestroy()
        {
            LogCallMessage();
            base.OnDestroy();
        }

        public override void OnUnload(World _world)
        {
            LogCallMessage();
            base.OnUnload(_world);
        }

        public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _placingEntity)
        {
            LogCallMessage();
            base.PlaceBlock(_world, _result, _placingEntity);
        }

        public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode, int _readVersion)
        {
            LogCallMessage();
            base.Read(_br, _eStreamMode, _readVersion);
        }

        public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
        {
            LogCallMessage();
            base.ReplacedBy(_bvOld, _bvNew, _teNew);
        }

        public override void Reset(FastTags<TagGroup.Global> _questTags)
        {
            LogCallMessage();
            base.Reset(_questTags);
        }

        public override void SetBlockEntityData(BlockEntityData _blockEntityData)
        {
            LogCallMessage();
            base.SetBlockEntityData(_blockEntityData);
        }

        public override void UpdateBlockActivationCommands(ref BlockActivationCommand _command, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
        {
            LogCallMessage();
            base.UpdateBlockActivationCommands(ref _command, _commandName, _world, _blockPos, _blockValue, _entityFocusing);
        }

        public override void UpdateTick(World _world)
        {
            //LogMessage();
            base.UpdateTick(_world);
        }

        public override void UpgradeDowngradeFrom(TileEntityComposite _other)
        {
            LogCallMessage();
            base.UpgradeDowngradeFrom(_other);
        }

        public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
        {
            LogCallMessage();
            base.Write(_bw, _eStreamMode);
        }

        private void LogCallMessage(string message = "", [CallerMemberName] string caller = "")
        {
            string identifier = $"guid: {guid}";
            int entityId = EntityId;
            if (entityId > -1)
            {
                identifier = $"eid: {entityId} - identifier";
            }

            if (string.IsNullOrEmpty(message))
            {
                Log.Out($"[TC-DT] TEFLog - {caller} ({identifier})");
            }
            else
            {
                Log.Out($"[TC-DT] TEFLog - {caller}: {message} ({identifier})");
            }
        }
    }
}
