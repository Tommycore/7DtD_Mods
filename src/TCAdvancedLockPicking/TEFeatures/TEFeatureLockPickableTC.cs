using System;
using Audio;
using LockPicking;
using Platform;
using TEFeatures.Interfaces;
using UnityEngine;

namespace TEFeatures
{
    public class TEFeatureLockPickableTC : TEFeatureAbs, ILockPickable
    {
        public static string LockPickItemBaseName => "resourceLockPickT";

        private ILockableTC lockFeature;
        private string lockPickSuccessEvent;
        private string lockPickFailedEvent;
        private BlockValue lockpickDowngradeBlock;
        private float pickPercentageLeft = 1f;

        public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
        {
            base.Init(_parent, _featureData);
            lockFeature = base.Parent.GetFeature<ILockableTC>();
            if (lockFeature == null)
            {
                Log.Error("Block with name " + base.Parent.TeData.Block.GetBlockName() + " does have a TEFeatureLockPickableTC but no ILockableTC feature");
            }

            DynamicProperties props = _featureData.Props;

            props.ParseString(BlockSecureLoot.PropOnLockPickSuccessEvent, ref lockPickSuccessEvent);
            props.ParseString(BlockSecureLoot.PropOnLockPickFailedEvent, ref lockPickFailedEvent);
            if (props.Values.ContainsKey(Block.PropLockpickDowngradeBlock))
            {
                string text = props.Values[Block.PropLockpickDowngradeBlock];
                if (!string.IsNullOrEmpty(text))
                {
                    lockpickDowngradeBlock = Block.GetBlockValue(text, false);
                    if (lockpickDowngradeBlock.isair)
                    {
                        throw new Exception("Block with name '" + text + "' not found in block " + base.Parent.TeData.Block.GetBlockName());
                    }
                }
            }
            else
            {
                lockpickDowngradeBlock = base.Parent.TeData.Block.DowngradeBlock;
            }
        }

        public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
        {
            return !lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier)
                ? string.Format(Localization.Get("tooltipLocked", false), _activateHotkeyMarkup, _focusedTileEntityName)
                : null;
        }

        public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
        {
            base.InitBlockActivationCommands(_addCallback);
            _addCallback(new BlockActivationCommand("pick", "unlock", false, false), TileEntityComposite.EBlockCommandOrder.First, base.FeatureData);
        }

        public override void UpdateBlockActivationCommands(ref BlockActivationCommand _command, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
        {
            base.UpdateBlockActivationCommands(ref _command, _commandName, _world, _blockPos, _blockValue, _entityFocusing);
            if (base.CommandIs(_commandName, "pick"))
            {
                _command.enabled = (lockFeature.IsLocked() && !lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier));
                return;
            }
        }

        public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
        {
            base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
            if (!base.CommandIs(_commandName, "pick"))
            {
                return false;
            }

            bool hasLockPicks = GetHighestLockpickTier(_player) > -1;

            if (!hasLockPicks)
            {
                int requiredLockpickLevel = lockFeature.LockDifficulty;
                ItemValue item = ItemClass.GetItem($"{LockPickItemBaseName}{requiredLockpickLevel}", false);

                LocalPlayerUI playerUI = _player.PlayerUI;
                playerUI.xui.CollectedItemList.AddItemStack(new ItemStack(item, 0), true);
                GameManager.ShowTooltip(_player, Localization.Get("ttLockpickMissing", false), false, false, 0f);

                return true;
            }

            _player.AimingGun = false;
            Vector3i blockPos = base.Parent.ToWorldPos();
            _world.GetGameManager().TELockServer(0, blockPos, base.Parent.EntityId, _player.entityId, "lockpick");
            return true;
        }

        public void ShowLockpickUi(EntityPlayerLocal _player)
        {
            if (_player == null)
            {
                return;
            }

            int lockLevel = lockFeature.LockDifficulty;
            int lockPickTier = GetHighestLockpickTier(_player);

            float alternateTime = -1f;
            float effectivePickTimeMax = GetEffectivePickTimeMax(_player, lockLevel, lockPickTier);
            float pickTimeLeft = effectivePickTimeMax * pickPercentageLeft;

            if (IsLockPickBreakingOnAttempt(_player, lockLevel, lockPickTier))
            {
                alternateTime = _player.rand.RandomRange(0.3f, 0.95f) * pickTimeLeft;
            }

            TimerEventData timerEventData = new TimerEventData();
            timerEventData.CloseEvent += EventData_PlayerCanceled;
            timerEventData.Data = new LockPickingTimerData() { Player = _player, TimeMax = effectivePickTimeMax };
            timerEventData.Event += EventData_Success;
            timerEventData.alternateTime = alternateTime;
            timerEventData.AlternateEvent += EventData_Failure;

            LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_player);
            uiforPlayer.windowManager.Open("timer", true, false, true);
            XUiC_Timer childByType = uiforPlayer.xui.GetChildByType<XUiC_Timer>();
            childByType.SetTimer(effectivePickTimeMax, timerEventData, pickTimeLeft, "");
            Manager.BroadcastPlayByLocalPlayer(base.Parent.ToWorldPos().ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
        }

        private float GetEffectivePickTimeMax(EntityPlayerLocal _player, int _lockLevel, int _lockPickTier)
        {
            float basePickTime = 30.0f + 10.0f * lockFeature.LockDifficulty;
            float pickTimeFactorLockPick = GetLockPickTimeFactor(_lockLevel, _lockPickTier);
            float effectivePickTime = Math.Max(3.0f, EffectManager.GetValue(PassiveEffects.LockPickTime, _player.inventory.holdingItemItemValue, basePickTime * pickTimeFactorLockPick, _player, null, default, true, true, true, true, true, 1, true, false));

            return effectivePickTime;
        }

        private bool IsLockPickBreakingOnAttempt(EntityPlayerLocal _player, int _lockLevel, int _lockPickTier)
        {
            float lockPickBreakChance = GetLockPickBreakChance(_lockLevel, _lockPickTier);
            float effectiveLockpickBreakChance = EffectManager.GetValue(PassiveEffects.LockPickBreakChance, _player.inventory.holdingItemItemValue, lockPickBreakChance, _player, null, default, true, true, true, true, true, 1, true, false);
            float normalisedRandomValue = _player.rand.RandomRange(1f);

            return normalisedRandomValue < effectiveLockpickBreakChance;
        }

        private void EventData_PlayerCanceled(TimerEventData _timerData)
        {
            LockPickingTimerData lockPickingTimerData = (LockPickingTimerData)_timerData.Data;

            Vector3i vector3i = base.Parent.ToWorldPos();
            Manager.BroadcastPlayByLocalPlayer(vector3i.ToVector3() + Vector3.one * 0.5f, "Misc/locked");

            pickPercentageLeft = Mathf.Max(_timerData.timeLeft, 3.0f) / lockPickingTimerData.TimeMax;

            if (lockPickFailedEvent != null)
            {
                GameEventManager.Current.HandleAction(lockPickFailedEvent, null, lockPickingTimerData.Player, false, vector3i, "", "", false, true, "", null);
            }

            ResetEventData(_timerData);
            GameManager.Instance.TEUnlockServer(0, vector3i, base.Parent.EntityId, false);
        }

        private void EventData_Failure(TimerEventData _timerData)
        {
            LockPickingTimerData lockPickingTimerData = (LockPickingTimerData)_timerData.Data;

            BreakLockPick(lockPickingTimerData.Player);

            Vector3i vector3i = base.Parent.ToWorldPos();
            Manager.BroadcastPlayByLocalPlayer(vector3i.ToVector3() + Vector3.one * 0.5f, "Misc/locked");

            pickPercentageLeft = Mathf.Max(_timerData.timeLeft, 3.0f) / lockPickingTimerData.TimeMax;

            if (lockPickFailedEvent != null)
            {
                GameEventManager.Current.HandleAction(lockPickFailedEvent, null, lockPickingTimerData.Player, false, vector3i, "", "", false, true, "", null);
            }

            ResetEventData(_timerData);
            GameManager.Instance.TEUnlockServer(0, vector3i, base.Parent.EntityId, false);
        }

        private void BreakLockPick(EntityPlayerLocal _player)
        {
            int highestLockpick = GetHighestLockpickTier(_player);
            ItemValue item = ItemClass.GetItem($"{LockPickItemBaseName}{highestLockpick}", false);
            LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_player);

            ItemStack itemStack = new ItemStack(item, 1);
            uiforPlayer.xui.PlayerInventory.RemoveItem(itemStack);
            uiforPlayer.xui.CollectedItemList.RemoveItemStack(itemStack);
            GameManager.ShowTooltip(_player, Localization.Get("ttLockpickBroken", false), false, false, 0f);
        }

        private void EventData_Success(TimerEventData _timerData)
        {
            World world = GameManager.Instance.World;
            LockPickingTimerData lockPickingTimerData = (LockPickingTimerData)_timerData.Data;
            Vector3i vector3i = base.Parent.ToWorldPos();
            BlockValue block = world.GetBlock(vector3i);
            lockFeature.SetLocked(false);

            if (!lockpickDowngradeBlock.isair)
            {
                BlockValue blockValue = base.Parent.TeData.Block.LockpickDowngradeBlock;
                blockValue = BlockPlaceholderMap.Instance.Replace(blockValue, world.GetGameRandom(), vector3i.x, vector3i.z, false);
                blockValue.rotation = block.rotation;
                blockValue.meta = block.meta;
                world.SetBlockRPC(0, vector3i, blockValue, blockValue.Block.Density);
            }

            Manager.BroadcastPlayByLocalPlayer(vector3i.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
            if (lockPickSuccessEvent != null)
            {
                GameEventManager.Current.HandleAction(lockPickSuccessEvent, null, lockPickingTimerData.Player, false, vector3i, "", "", false, true, "", null);
            }

            ResetEventData(_timerData);
            GameManager.Instance.TEUnlockServer(0, vector3i, base.Parent.EntityId, false);
        }

        private void ResetEventData(TimerEventData _timerData)
        {
            _timerData.AlternateEvent -= EventData_PlayerCanceled;
            _timerData.CloseEvent -= EventData_PlayerCanceled;
            _timerData.Event -= EventData_Success;
        }

        public float GetLockPickBreakChance(int baseLockDifficulty, int lockPickTier)
        {
            return Math.Clamp(0.6f + 0.1f * GetChallengeRating(baseLockDifficulty, lockPickTier), 0.2f, 0.95f);
        }

        public float GetLockPickTimeFactor(int baseLockDifficulty, int lockPickTier)
        {
            return Math.Clamp(1.0f + 0.1f * GetChallengeRating(baseLockDifficulty, lockPickTier), 0.8f, 1.2f);
        }

        public int GetHighestLockpickTier(EntityPlayerLocal _player)
        {
            LocalPlayerUI playerUI = _player.PlayerUI;
            for (int i = 4; i > -1; --i)
            {
                ItemValue item = ItemClass.GetItem($"resourceLockPickT{i}", false);
                if (playerUI.xui.PlayerInventory.GetItemCount(item) > 0)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Compares lock and lockpick tiers.
        /// </summary>
        /// <returns>Negative values for easier challenges, positive values for harder challenges</returns>
        private static float GetChallengeRating(int baseLockDifficulty, int lockPickTier)
        {
            return baseLockDifficulty - lockPickTier;
        }
    }
}
