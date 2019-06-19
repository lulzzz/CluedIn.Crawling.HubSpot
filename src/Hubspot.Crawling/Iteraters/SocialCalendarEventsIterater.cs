﻿using System;
using System.Collections.Generic;
using System.Linq;
using CluedIn.Crawling.HubSpot.Core;
using CluedIn.Crawling.HubSpot.Infrastructure;

namespace CluedIn.Crawling.HubSpot.Iteraters
{
    public class SocialCalendarEventsIterater : HubSpotIteraterBase
    {
        public SocialCalendarEventsIterater(IHubSpotClient client, HubSpotCrawlJobData jobData) : base(client, jobData)
        {
        }

        public override IEnumerable<object> Iterate(int? limit)
        {
            int offset = 0;
            limit = limit ?? 20;

            while (true)
            {
                var response = Client.GetSocialCalendarEventsAsync(JobData.LastCrawlFinishTime, DateTimeOffset.UtcNow, limit.Value, offset).Result;

                if (response == null || !response.Any())
                    break;

                foreach (var obj in response)
                {
                    yield return obj;
                }

                if (response.Count < limit)
                    break;

                offset += limit.Value;
            }
        }
    }
}
