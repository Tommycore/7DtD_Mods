using System;
using Audio;
using Platform;
using TEFeatures.Interfaces;
using UnityEngine;

namespace TEFeatures
{
    public class TEFeatureOpenable : TEFeatureAbs
    {
        public static string PropChanceToStartOpen => "ChanceToStartOpen";
        public static string PropChanceToStartLocked => "ChanceToStartLocked";
        public static string PropOpenSound => "OpenSound";
        public static string PropCloseSound => "CloseSound";

        public bool IsOpen
        {
            get => isOpen;
            set
            {
                isOpen = value;
                base.SetModified();
            }
        }

        private ILockableTC lockFeature;
        private bool isOpen = false;
        private float chanceToStartOpen;
        private float chanceToStartLocked;
        private string openSound;
        private string closeSound;
        private bool isInitialised = false;

        public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
        {
            base.Init(_parent, _featureData);

            lockFeature = base.Parent.GetFeature<ILockableTC>();

            DynamicProperties props = _featureData.Props;
            props.ParseFloat(PropChanceToStartOpen, ref chanceToStartOpen);
            props.ParseFloat(PropChanceToStartLocked, ref chanceToStartLocked);

            DynamicProperties parentProps = _parent.blockValue.Block.Properties;

            parentProps.ParseString(PropOpenSound, ref openSound);
            parentProps.ParseString(PropCloseSound, ref closeSound);
        }

        public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
        {
            bool isDoorLocked = lockFeature != null && lockFeature.IsLocked();
            PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
            string actionButton = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
            string door = Localization.Get("door", false);

            return isDoorLocked
                ? string.Format(Localization.Get("tooltipLocked", false), actionButton, door)
                : string.Format(Localization.Get("tooltipUnlocked", false), actionButton, door);
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
                (base.CommandIs(_commandName, "open") && CanOpen())
                || (base.CommandIs(_commandName, "close") && CanClose());
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

            UpdateAnimState();

            if (_player != null)
            {
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, isOpen ? openSound : closeSound);
            }

            return base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
        }

        public override void SetBlockEntityData(BlockEntityData _blockEntityData)
        {
            base.SetBlockEntityData(_blockEntityData);
            Initialize();
            UpdateAnimState();
        }

        private bool CanOpen()
        {
            return !IsOpen && (lockFeature == null || !lockFeature.IsLocked());
        }

        private bool CanClose()
        {
            return IsOpen;
        }

        /// <summary>
        /// Handles opening or closing the door visually
        /// </summary>
        /// <param name="_isForced">If true, skips animation and sets it to the final position</param>
        private void UpdateAnimState(bool _isForced = false)
        {
            BlockEntityData blockEntity = Parent.GetChunk().GetBlockEntity(Parent.ToWorldPos());
            if (blockEntity == null || !blockEntity.bHasTransform)
            {
                Log.Warning($"[TC-ALP] BlockEntityData not found or no transform - {Parent?.TeData?.Block?.GetBlockName()}");
                return;
            }

            Animator[] componentsInChildren = blockEntity.transform.GetComponentsInChildren<Animator>();
            if (componentsInChildren == null)
            {
                Log.Warning($"[TC-ALP] no Animator components in Children - {Parent?.TeData?.Block?.GetBlockName()}");
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

        private void Initialize()
        {
            if (isInitialised || IsOwnedByLocalPlayer())
            {
                return;
            }

            isInitialised = true;
            IsOpen = UnityEngine.Random.value < chanceToStartOpen;
            lockFeature.InitializeLockStatus(IsOpen ? -1f : chanceToStartLocked);
        }

        private bool IsOwnedByLocalPlayer()
        {
            return PlatformManager.InternalLocalUserIdentifier?.Equals(base.Parent.Owner) ?? false;
        }

        public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode, int _readVersion)
        {
            base.Read(_br, _eStreamMode, _readVersion);
            isOpen = _br.ReadBoolean();
            isInitialised = _br.ReadBoolean();
        }

        public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
        {
            base.Write(_bw, _eStreamMode);
            _bw.Write(isOpen);
            _bw.Write(isInitialised);
        }
    }
}
