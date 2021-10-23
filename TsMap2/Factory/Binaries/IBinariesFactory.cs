using System.IO;

namespace TsMap2.Factory.Binaries {
    public interface IBinariesFactory< out T > {
        string GetSavingPath();

        BinaryWriter Writer();

        void Save();

        void Close();
    }
}