﻿using DatagramsNet;
using TeleBufet.NET.CacheManager.Attributes;

namespace TeleBufet.NET.CacheManager.CacheDirectories
{

    [CacheTable(typeof(ProductHolder))]
    internal sealed class CartCache : CacheFile
    {
        public override string FileName => "CartCachessssssssssssssssssssssssssss";
        public static TimeSpan LastChanges { get; set; }

        public CartCache() : base() { }
    }
}
