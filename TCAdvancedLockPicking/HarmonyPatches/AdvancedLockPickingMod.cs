using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace HarmonyPatches
{
    public class AdvancedLockPickingMod : IModApi
    {
        public static Dictionary<Vector3i, float> pickTimeDict = new Dictionary<Vector3i, float>();

        public void InitMod(Mod _modInstance)
        {
            Log.Out("[TC-ALP] Loading Patch: " + GetType());

            Harmony harmony = new Harmony(nameof(AdvancedLockPickingMod));
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
