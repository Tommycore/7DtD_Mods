using Audio;
using TileEntities;
using UnityEngine;

namespace Blocks
{
    public class BlockPickableDoor : BlockDoorSecure
    {
        public override bool AllowBlockTriggers => true;

        private static string PropRequiredLockPickTier => "RequiredLockPickTier";
        private static string PropStartLockedChance => "StartLockedChance";

        private int requiredLockPickTier = 0;
        private float startLockedChance = 0.5f;

        public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
        {
            new BlockActivationCommand("pick", "unlock", false, false),
            new BlockActivationCommand("close", "door", false, false),
            new BlockActivationCommand("open", "door", false, false)
        };

        public override void Init()
        {
            Log.Out("[TC-ALP] Init");
            base.Init();

            Log.Out("[TC-ALP] Init - loading additional properties");
            base.Properties.ParseInt(BlockPickableDoor.PropRequiredLockPickTier, ref requiredLockPickTier);
            base.Properties.ParseFloat(BlockPickableDoor.PropRequiredLockPickTier, ref startLockedChance);
        }

        public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
        {
            Log.Out("[TC-ALP] OnBlockAdded");
            base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);

            if (_world.IsEditor()
                || _blockValue.ischild
                || _world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityPickableDoor tileEntityPickableDoor)
            {
                return;
            }

            tileEntityPickableDoor = new TileEntityPickableDoor(_chunk);
            tileEntityPickableDoor.SetDisableModifiedCheck(true);
            tileEntityPickableDoor.localChunkPos = World.toBlock(_blockPos);
            tileEntityPickableDoor.SetLocked(Random.value < startLockedChance);
            tileEntityPickableDoor.SetDisableModifiedCheck(false);
            tileEntityPickableDoor.RequiredLockPickTier = requiredLockPickTier;
            _chunk.AddTileEntity(tileEntityPickableDoor);
        }

        public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
        {
            if (!(_world.GetTileEntity(_clrIdx, _blockPos) is TileEntityPickableDoor tileEntityPickableDoor))
            {
                Log.Warning("[TC-ALP] No TileEntityPickableDoor found");
                var te = _world.GetTileEntity(_clrIdx, _blockPos);
                Log.Out($"[TC-ALP] Type: {te.GetType()}");
                Log.Out($"[TC-ALP] TileEntityType: {te.GetTileEntityType()}");

                return BlockActivationCommand.Empty;
            }

            Log.Out("[TC-ALP] GetBlockActivationCommands");
            Log.Out($"[TC-ALP] IsDoorOpen: {BlockDoor.IsDoorOpen(_blockValue.meta)}");
            Log.Out($"[TC-ALP] IsDoorLocked: {tileEntityPickableDoor.IsLocked()}");

            cmds[0].enabled = !BlockDoor.IsDoorOpen(_blockValue.meta) && tileEntityPickableDoor.IsLocked();
            cmds[1].enabled = BlockDoor.IsDoorOpen(_blockValue.meta);
            cmds[2].enabled = !BlockDoor.IsDoorOpen(_blockValue.meta);

            return cmds;
        }

        public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
        {
            Log.Out("[TC-ALP] OnBlockActivated");

            if (_blockValue.ischild)
            {
                Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
                BlockValue block = _world.GetBlock(parentPos);
                return OnBlockActivated(_commandName, _world, _cIdx, parentPos, block, _player);
            }

            if (!(_world.GetTileEntity(_cIdx, _blockPos) is TileEntityPickableDoor tileEntityPickableDoor))
            {
                Log.Warning("[TC-ALP] No TileEntityPickableDoor found");
                return false;
            }

            switch (_commandName)
            {
                case "pick": return ExecutePickCommand(_player, tileEntityPickableDoor);
                case "close": return ExecuteOpenCloseCommand(_world, _cIdx, _blockPos, _blockValue, _player);
                case "open": return ExecuteOpenCloseCommand(_world, _cIdx, _blockPos, _blockValue, _player);

                default: return false;
            }
        }

        private bool ExecutePickCommand(EntityPlayerLocal _player, TileEntityPickableDoor _tileEntityPickableDoor)
        {
            Log.Out("[TC-ALP] ExecutePickCommand");

            LocalPlayerUI playerUI = _player.PlayerUI;
            int tierAvailable = GetHighestTierLockPickAvailable(playerUI);

            Log.Out($"[TC-ALP] Highest tier lock pick found: {tierAvailable}");
            Log.Out($"[TC-ALP] Lock pick tier {requiredLockPickTier} required");

            if (requiredLockPickTier > tierAvailable)
            {
                ItemValue item = ItemClass.GetItem($"resourceLockPickT{requiredLockPickTier}", false);
                playerUI.xui.CollectedItemList.AddItemStack(new ItemStack(item, 0), true);
                GameManager.ShowTooltip(_player, Localization.Get("ttLockpickMissing", false), false, false, 0f);
                return true;
            }

            return true;
        }

        private bool ExecuteOpenCloseCommand(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
        {
            Log.Out("[TC-ALP] ExecuteOpenCloseCommand");

            bool flag = !BlockDoor.IsDoorOpen(_blockValue.meta);
            updateOpenCloseState(flag, _world, _blockPos, _cIdx, _blockValue, false);
            if (_player != null)
            {
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, flag ? openSound : closeSound);
            }

            return true;
        }

        private int GetHighestTierLockPickAvailable(LocalPlayerUI _playerUI)
        {
            for (int i = 4; i > -1; --i)
            {
                ItemValue item = ItemClass.GetItem($"resourceLockPickT{i}", false);

                if (_playerUI.xui.PlayerInventory.GetItemCount(item) > 0)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
