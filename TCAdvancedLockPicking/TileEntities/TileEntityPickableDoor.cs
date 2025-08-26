using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileEntities
{
    public class TileEntityPickableDoor : TileEntitySecureDoor
    {
        private int requiredLockPickTier = 0;

        public int RequiredLockPickTier
        {
            get => requiredLockPickTier;
            set => requiredLockPickTier = value;
        }

        public TileEntityPickableDoor(Chunk _chunk) : base(_chunk)
        {
        }
    }
}
