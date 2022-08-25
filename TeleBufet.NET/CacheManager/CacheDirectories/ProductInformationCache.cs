﻿using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.CacheManager.Attributes;

namespace TeleBufet.NET.CacheManager.CacheDirectories
{
    [CacheTable(typeof(ProductInformationTable))]
    internal sealed class ProductInformationCache : CacheFile
    {
        public override string FileName => "ProductInformationCacheTwentysssssss";
        public static TimeSpan LastChanges { get; set; }

        public ProductInformationCache() : base() { }
    }
}
