using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleBufet.NET.CacheManager.CacheDirectories
{
    internal sealed class ProductCache : CacheFile
    {
        public override string FileName => "Products";

        public ProductCache() : base() { }
    }
}
