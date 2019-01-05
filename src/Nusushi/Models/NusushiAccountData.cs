using System;

namespace Nusushi.Models
{
    public struct NusushiAccountData
    {
        public string NusushiUserId { get; set; }
        public string TokyoUserId { get; set; }
        public string TokyoDescription { get; set; }
        public string CaliforniaUserId { get; set; }
        public string CaliforniaDescription { get; set; }
        public int TimeStampCount { get; set; }
    }
}