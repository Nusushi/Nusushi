using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;

namespace Nusushi.Models.IndexViewModels
{
    public class IndexViewModel
    {
        public IHostingEnvironment HostingEnvironment { get; set; }
        public IEnumerable<NusushiAccountData> NusushiAccounts { get; set; } = new List<NusushiAccountData>();
    }
}