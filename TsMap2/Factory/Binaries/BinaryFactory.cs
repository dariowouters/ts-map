using System.IO;
using TsMap2.Helper;

namespace TsMap2.Factory.Binaries {
    public abstract class BinaryFactory< T > : IBinariesFactory< T > {
        private readonly BinaryWriter _writer;
        protected        StoreHelper  Store => StoreHelper.Instance;


        protected BinaryFactory() {
            FileStream stream = new(GetSavingPath(), FileMode.Create);
            _writer = new BinaryWriter( stream );
        }

        public abstract string       GetSavingPath();
        public abstract void         Save();
        public          BinaryWriter Writer() => _writer;

        public void Close() {
            _writer.Close();
        }
    }
}