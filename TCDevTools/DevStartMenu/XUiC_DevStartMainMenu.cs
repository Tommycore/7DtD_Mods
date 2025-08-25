using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevStartMenu
{
    public class XUiC_DevStartMainMenu : XUiController
    {
        public static string ID = "";

        public override void Init()
        {
            base.Init();
            XUiC_DevStartMainMenu.ID = base.WindowGroup.ID;
        }
    }
}
