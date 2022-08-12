using DatagramsNet;
using TeleBufet.NET.CacheManager.CacheDirectories;

namespace TeleBufet.NET.CacheManager.CustomCacheHelper.ShoppingCartCache
{
    internal class CartCacheHelper : TableCacheHelper<ProductHolder, CartCache>
    {
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

            directory.CacheFileStream.Seek(0, SeekOrigin.Begin);
            var binaryReader = new BinaryReader(directory.CacheFileStream);
            var spanBytes = binaryReader.ReadBytes((int)directory.CacheFileStream.Length);
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
