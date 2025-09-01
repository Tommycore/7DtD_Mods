using System;
using Platform;

namespace TEFeatures
{
    public class TEFeatureLockPickableTC : TEFeatureLockPickable
    {
        public override void UpdateBlockActivationCommands(ref BlockActivationCommand _command, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
        {
            base.UpdateBlockActivationCommands(ref _command, _commandName, _world, _blockPos, _blockValue, _entityFocusing);
            if (base.CommandIs(_commandName, "pick"))
            {
                Log.Out("[TC-ALP] TEFeatureLockPickableTC.UpdateBlockActivationCommands");
                Log.Out($"[TC-ALP] Locked: {lockFeature.IsLocked()}");
                Log.Out($"[TC-ALP] IsUserAllowed: {lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier)}");
                _command.enabled = (lockFeature.IsLocked() && !lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier));
                Log.Out($"[TC-ALP] Pick command enabled: {_command.enabled}");

                return;
            }
        }
    }
}
