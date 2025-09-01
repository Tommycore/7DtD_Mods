using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevStartMenu;
using HarmonyLib;

namespace HarmonyPatches
{
    public class GameStarter
    {
        private static bool HasDevStartLaunchArgument =>
            Environment.GetCommandLineArgs()
                .Any(argument => argument.EqualsCaseInsensitive("-devstart"));

        [HarmonyPatch(typeof(XUiC_MainMenu), nameof(XUiC_MainMenu.Open))]
        public class XUiC_MainMenu_Open
        {
            private static bool Prefix(XUiC_MainMenu __instance, XUi _xuiInstance)
            {
                Log.Out("[TC-DT] XUiC_MainMenu_Open");
                return true;

                if (HasDevStartLaunchArgument)
                {
                    // Skip straight to opening the main menu
                    //_xuiInstance.playerUI.windowManager.Open(XUiC_DevStartMainMenu.ID, true, false, true);
                    _xuiInstance.playerUI.windowManager.Open(XUiC_MainMenu.ID, true, false, true);
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(XUiC_MainMenu), nameof(XUiC_MainMenu.OnOpen))]
        public class XUiC_MainMenu_OnOpen
        {
            private static void Postfix(XUiC_MainMenu __instance)
            {

                GamePrefs.Set(EnumGamePrefs.CreativeMenuEnabled, 1);
            }
        }
    }
}
