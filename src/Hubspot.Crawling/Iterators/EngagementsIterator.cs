﻿using System.Collections.Generic;
using System.Linq;
using CluedIn.Crawling.HubSpot.Core;
using CluedIn.Crawling.HubSpot.Infrastructure;

namespace CluedIn.Crawling.HubSpot.Iterators
{
    public class EngagementsIterator : HubSpotIteratorBase
    {
        public EngagementsIterator(IHubSpotClient client, HubSpotCrawlJobData jobData) : base(client, jobData)
        {
        }

        public override IEnumerable<object> Iterate(int? limit = null)
        {
            long offset = 0;
            limit = limit ?? 20;

            var result = new List<object>();
            try
            {
                while (true)
                {
                    var response = Client.GetEngagementsAsync(limit.Value, offset).Result;

                    if (response?.results == null || !response.results.Any())
                        break;

                    result.AddRange(response.results);

                    if (response.results.Count < limit || response.offset == null)
                        break;

                    offset = response.offset.Value;
                }
            }
            catch
            {
                return Enumerable.Empty<object>();
            }

            return result;
        }
    }
}
