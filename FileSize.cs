namespace Bps.Common
{
    using System.Collections.Generic;

    public class FileSize
    {
        #region Types

        public enum Unit
        {
            Byte,
            KiloByte,
            MegaByte,
            GigaByte,
            TeraByte,
            PetaByte,
            ExaByte
        }

        #endregion

        #region Fields

        public static Dictionary<Unit, ulong> UnitSize { get; protected set; }

        private readonly ulong bytes;

        #endregion

        #region Constructors

        public FileSize(ulong bytes)
        {
            this.bytes = bytes;
        }

        #endregion

        #region Methods

        public static FileSize Create(int Size, Unit Unit)
        {
            return new FileSize((ulong)Size * GetUnitSize(Unit));
        }

        private static ulong GetUnitSize(Unit Unit)
        {
            if (UnitSize == null)
            {
                ulong size = 1;
                UnitSize = new Dictionary<Unit, ulong>();
                for (Unit u = Unit.Byte; u < Unit.ExaByte; u++)
                {
                    UnitSize.Add(u, size);
                    size *= 1024;
                }
            }

            return UnitSize[Unit];
        }

        public ulong Size(Unit Unit)
        {
            return bytes / GetUnitSize(Unit);
        }

        #endregion
    }
}
