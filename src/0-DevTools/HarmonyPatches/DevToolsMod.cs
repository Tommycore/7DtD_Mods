using System.Reflection;
using HarmonyLib;

namespace DevTools.HarmonyPatches
{
    public class DevToolsMod : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            Log.Out("[TC-DT] Loading Patch: " + GetType());

            Harmony harmony = new Harmony(nameof(DevToolsMod));
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
