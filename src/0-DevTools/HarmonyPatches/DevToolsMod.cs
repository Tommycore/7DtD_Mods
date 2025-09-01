using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace HarmonyPatches
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
