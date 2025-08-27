using System;
using UnityEngine;

namespace TEFeatures
{
    public class TEFeatureOpenable : TEFeatureAbs
    {
        private ILockable lockFeature;

        public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
        {
            base.Init(_parent, _featureData);

            lockFeature = base.Parent.GetFeature<ILockable>();
        }

        public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
        {
            PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
            string arg = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
            return string.Format(Localization.Get("useBlock", false), arg, _blockValue.Block.GetLocalizedBlockName());
        }

        public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
        {
            base.InitBlockActivationCommands(_addCallback);

            _addCallback(new BlockActivationCommand("close", "door", false, false), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
            _addCallback(new BlockActivationCommand("open", "door", false, false), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
        }

        public override void UpdateBlockActivationCommands(ref BlockActivationCommand _command, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
        {
            base.UpdateBlockActivationCommands(ref _command, _commandName, _world, _blockPos, _blockValue, _entityFocusing);

            bool isOpen = BlockDoor.IsDoorOpen(_blockValue.meta);

            _command.enabled =
                base.CommandIs(_commandName, "open") && !isOpen
                || base.CommandIs(_commandName, "close") && isOpen;
        }

        public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
        {
            string command = base.CommandIs(_commandName, "open")
                ? "open"
                : base.CommandIs(_commandName, "close")
                    ? "close"
                    : string.Empty; // Invalid

            bool isOpen = BlockDoor.IsDoorOpen(_blockValue.meta);

            if (string.IsNullOrEmpty(command))
            {
                return false;
            }

            if (command == "open")
            {
                _blockValue.meta = (byte)((int)_blockValue.meta & -2);
            }
            else
            {
                _blockValue.meta |= 1;
            }

            isOpen = !isOpen;

            UpdateOpenCloseState(isOpen, _world, _blockPos, _blockValue, false);
            UpdateAnimState(_world, _blockPos, isOpen);

            if (_player != null)
            {
                //Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, isOpen ? this.openSound : this.closeSound);
            }

            return base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
        }

        private void UpdateAnimState(WorldBase _world, Vector3i _blockPos, bool _isOpen)
        {
            BlockEntityData blockEntity = _world.ChunkClusters[0].GetBlockEntity(_blockPos);
            if (blockEntity != null && blockEntity.bHasTransform)
            {
                Animator[] componentsInChildren = blockEntity.transform.GetComponentsInChildren<Animator>();
                if (componentsInChildren != null)
                {
                    for (int i = componentsInChildren.Length - 1; i >= 0; i--)
                    {
                        Animator animator = componentsInChildren[i];
                        animator.enabled = true;
                        animator.SetBool(AnimatorDoorState.IsOpenHash, _isOpen);
                        animator.SetTrigger(AnimatorDoorState.OpenTriggerHash);
                    }
                }
            }
        }

        private void UpdateOpenCloseState(bool _isOpen, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _isOnlyLocal)
            {
            _blockValue.meta = (byte)((_isOpen ? 1 : 0) | ((int)_blockValue.meta & -2));
            if (!_isOnlyLocal)
            {
                _world.SetBlockRPC(0, _blockPos, _blockValue);
                return;
            }

            _world.ChunkClusters[0]?.SetBlockRaw(_blockPos, _blockValue);
        }
    }
}
