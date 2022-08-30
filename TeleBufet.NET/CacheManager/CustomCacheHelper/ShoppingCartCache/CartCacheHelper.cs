using DatagramsNet;
using TeleBufet.NET.CacheManager.CacheDirectories;

namespace TeleBufet.NET.CacheManager.CustomCacheHelper.ShoppingCartCache
{
    internal sealed class CartCacheHelper : TableCacheHelper<ProductHolder>
    {
        private static int productSize = GetProductSize();

        public int CacheLength => (int)this.directory.CacheFileStream.Length;
        public static int ProductsCount { get; private set; }


        public CartCacheHelper() 
        {
            directory = new CartCache();
            ProductsCount = CacheLength / productSize;
        }

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
                if (!directory.CacheFileStream.CanRead)
                    directory = GetCurrentCacheDirectory();

                directory.CacheFileStream.Seek(properIndex, SeekOrigin.Begin);
                using var binaryWriter = new BinaryWriter(directory.CacheFileStream);
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

            if (!directory.CacheFileStream.CanRead)
                directory = GetCurrentCacheDirectory();

            directory.CacheFileStream.Seek(properIndex, SeekOrigin.Begin);
            using var binaryReader = new BinaryReader(directory.CacheFileStream);

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

        private static int GetProductSize() 
        {
            var byteHolder = new byte[1];
            var productHolderSize = BinaryHelper.GetSizeOf(new ProductHolder(), typeof(ProductHolder), ref byteHolder);

            return productHolderSize;
        }

        public static void Clear() 
        {
            using var cartCacheHelper = new CartCacheHelper();
            cartCacheHelper.RemoveBytes(0, cartCacheHelper.CacheLength);
        }
    }
}
