using DatagramsNet;
using TeleBufet.NET.CacheManager.CacheDirectories;

namespace TeleBufet.NET.CacheManager.CustomCacheHelper.ShoppingCartCache
{
    internal class CartCacheHelper : TableCacheHelper<ProductHolder, CartCache>
    {
        public int CacheLength => (int)this.directory.CacheFileStream.Length;

        public override void Serialize()
        {
            int baseIndex = base.GetProperIndex();
            if (CacheValue.Amount < 1)
            {
                if (baseIndex == NotFoundInt)
                    return;
                byte[] byteHolder = new byte[1];
                var tableSize = BinaryHelper.GetSizeOf(CacheValue, CacheValue.GetType(), ref byteHolder);
                RemoveBytes(baseIndex, tableSize);
                return;
            }


            int properIndex = GetProperIndex();
            if (properIndex != NotFoundInt)
            {
                directory.CacheFileStream.Seek(properIndex, SeekOrigin.Begin);
                var binaryWriter = new BinaryWriter(directory.CacheFileStream);
                byte[] amountBytes = BinaryHelper.Write(CacheValue.Amount);
                binaryWriter.Write(amountBytes);
            }
            else
                base.Serialize();
        }

        public int GetCurrentAmount()
        {
            var properIndex = GetProperIndex();
            if (properIndex == NotFoundInt)
                return 0;

            directory.CacheFileStream.Seek(properIndex, SeekOrigin.Begin);
            var binaryReader = new BinaryReader(directory.CacheFileStream);

            //For now We will leave it on this field size, but in the future it's
            //going to be a proper generic solution
            Span<byte> byteAmount = binaryReader.ReadBytes(sizeof(int));
            return BinaryHelper.Read<int>(byteAmount.ToArray());
        }


        protected override int GetProperIndex()
        {
            byte[] byteHolder = new byte[1];

            int tableIndex = base.GetProperIndex();
            if (tableIndex == NotFoundInt)
                return tableIndex;
            int indetificatorSize = BinaryHelper.GetSizeOf(CacheValue.Id, CacheValue.Id.GetType(), ref byteHolder);

            return tableIndex + indetificatorSize;
        }

        private void RemoveBytes(int startIndex, int length) 
        {
            var shiftBytes = GetShiftBytes(startIndex, length).Span;

            directory.CacheFileStream.Seek(startIndex, SeekOrigin.Begin);
            var binaryWriter = new BinaryWriter(directory.CacheFileStream);
            int fileLength = (int)directory.CacheFileStream.Length;
            binaryWriter.Write(shiftBytes);

            directory.CacheFileStream.SetLength(fileLength - length);
        }

        private Memory<byte> GetShiftBytes(int startIndex, int length) 
        {
            directory.CacheFileStream.Seek(startIndex, SeekOrigin.Begin);
            var binaryReader = new BinaryReader(directory.CacheFileStream);
            var spanBytes = binaryReader.ReadBytes((int)directory.CacheFileStream.Length);
            return spanBytes[(length)..];
        }
    }
}
