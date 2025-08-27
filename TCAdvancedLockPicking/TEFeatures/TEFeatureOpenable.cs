using System;
using UnityEngine;

namespace TEFeatures
{
    public class TEFeatureOpenable : TEFeatureAbs
    {
        private ILockable lockFeature;

        private bool isOpen = false;
        public bool IsOpen
        {
            get => isOpen;
            set
            {
                isOpen = value;
                base.SetModified();
            }
        }

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

            _command.enabled =
                (base.CommandIs(_commandName, "open") && !isOpen)
                || (base.CommandIs(_commandName, "close") && isOpen);
        }

        public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
        {
            string command = base.CommandIs(_commandName, "open")
                ? "open"
                : base.CommandIs(_commandName, "close")
                    ? "close"
                    : string.Empty; // Invalid

            if (string.IsNullOrEmpty(command))
            {
                return false;
            }

            if (command == "open")
            {
                IsOpen = true;
            }
            else // command must be "close"
            {
                IsOpen = false;
            }

            UpdateAnimState(_world, _blockPos);

            if (_player != null)
            {
                //Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, isOpen ? this.openSound : this.closeSound);
            }

            return base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
        }

        /// <summary>
        /// Handles opening or closing the door visually
        /// </summary>
        /// <param name="_isForced">If true, skips animation and sets it to the final position</param>
        private void UpdateAnimState(WorldBase _world, Vector3i _blockPos, bool _isForced = false)
        {
            BlockEntityData blockEntity = _world.ChunkClusters[0].GetBlockEntity(_blockPos);
            if (blockEntity == null || !blockEntity.bHasTransform)
            {
                return;
            }

            Animator[] componentsInChildren = blockEntity.transform.GetComponentsInChildren<Animator>();
            if (componentsInChildren == null)
            {
                return;
            }

            for (int i = componentsInChildren.Length - 1; i >= 0; i--)
            {
                Animator animator = componentsInChildren[i];
                animator.enabled = true;
                animator.SetBool(AnimatorDoorState.IsOpenHash, IsOpen);
                if (_isForced)
                {
                    animator.Play(IsOpen ? AnimatorDoorState.OpenHash : AnimatorDoorState.CloseHash, 0, 1f);
                }
                else
                {
                    animator.SetTrigger(AnimatorDoorState.OpenTriggerHash);
                }
            }
        }

        public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode, int _readVersion)
        {
            base.Read(_br, _eStreamMode, _readVersion);
            isOpen = _br.ReadBoolean();
        }

        public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
        {
            base.Write(_bw, _eStreamMode);
            _bw.Write(isOpen);
        }
    }
}
