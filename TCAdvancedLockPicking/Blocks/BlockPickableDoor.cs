using Audio;
using UnityEngine;

namespace Blocks
{
    public class BlockPickableDoor : BlockDoorSecure
    {
        public override bool AllowBlockTriggers => true;

        private static string PropRequiredLockPickTier => "RequiredLockPickTier";

        private int requiredLockPickTier = 0;

        public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
        {
            new BlockActivationCommand("pick", "unlock", false, false),
            new BlockActivationCommand("close", "door", false, false),
            new BlockActivationCommand("open", "door", false, false)
        };

        public override void Init()
        {
            base.Init();

            Log.Out("[TC-ALP] Init - loading additional properties");
            base.Properties.ParseInt(BlockPickableDoor.PropRequiredLockPickTier, ref requiredLockPickTier);
        }

        public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
        {
            base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
        }

        public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
        {
            Debug.Log("[TC-ALP] GetBlockActivationCommands");

            cmds[0].enabled = !BlockDoor.IsDoorOpen(_blockValue.meta) && IsDoorLockedMeta(_blockValue.meta);
            cmds[1].enabled = BlockDoor.IsDoorOpen(_blockValue.meta);
            cmds[2].enabled = !BlockDoor.IsDoorOpen(_blockValue.meta);

            return cmds;
        }

        public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
        {
            Debug.Log("[TC-ALP] OnBlockActivated");

            if (_blockValue.ischild)
            {
                Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
                BlockValue block = _world.GetBlock(parentPos);
                return OnBlockActivated(_commandName, _world, _cIdx, parentPos, block, _player);
            }

            TileEntitySecureDoor tileEntitySecureDoor = (TileEntitySecureDoor)_world.GetTileEntity(_cIdx, _blockPos);
            if (tileEntitySecureDoor != null)
            {
                Debug.Log("[TC-ALP] No TileEntitySecureDoorFound");
                return false;
            }

            switch (_commandName)
            {
                case "pick": return ExecutePickCommand(_player, tileEntitySecureDoor);
                case "close": return ExecuteOpenCloseCommand(_world, _cIdx, _blockPos, _blockValue, _player);
                case "open": return ExecuteOpenCloseCommand(_world, _cIdx, _blockPos, _blockValue, _player);

                default: return false;
            }
        }

        private bool ExecutePickCommand(EntityPlayerLocal _player, TileEntitySecureDoor tileEntitySecureDoor)
        {
            Debug.Log("[TC-ALP] ExecutePickCommand");

            LocalPlayerUI playerUI = _player.PlayerUI;
            ItemValue item = ItemClass.GetItem(lockPickItem, false);

            if (playerUI.xui.PlayerInventory.GetItemCount(item) == 0)
            {
                playerUI.xui.CollectedItemList.AddItemStack(new ItemStack(item, 0), true);
                GameManager.ShowTooltip(_player, Localization.Get("ttLockpickMissing", false), false, false, 0f);
                return true;
            }

            return true;
        }

        private bool ExecuteOpenCloseCommand(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
        {
            Debug.Log("[TC-ALP] ExecuteOpenCloseCommand");

            bool flag = !BlockDoor.IsDoorOpen(_blockValue.meta);
            updateOpenCloseState(flag, _world, _blockPos, _cIdx, _blockValue, false);
            if (_player != null)
            {
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, flag ? openSound : closeSound);
            }
            return true;
        }
    }
}
