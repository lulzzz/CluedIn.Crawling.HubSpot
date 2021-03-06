﻿namespace CluedIn.Crawling.HubSpot.Core.Models
{
    public class Keyword
    {
        public string keyword { get; set; }
        public string keyword_guid { get; set; }
        public int country { get; set; }
        public int visits { get; set; }
        public int contacts { get; set; }
        public int leads { get; set; }
        public long created_at { get; set; }
    }
}
