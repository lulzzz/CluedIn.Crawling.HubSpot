﻿using System;
using System.Collections.Generic;
using System.Linq;
using CluedIn.Core.Logging;
using CluedIn.Crawling.HubSpot.Core;
using CluedIn.Crawling.HubSpot.Infrastructure;
using CluedIn.Crawling.HubSpot.Infrastructure.Exceptions;

namespace CluedIn.Crawling.HubSpot.Iterators
{
    public class TaskCalendarEventsIterator : HubSpotIteratorBase
    {
        public TaskCalendarEventsIterator(IHubSpotClient client, HubSpotCrawlJobData jobData, ILogger logger)
            : base(client, jobData, logger)
        {
        }

        public override IEnumerable<object> Iterate(int? limit = null)
        {
            var offset = 0;
            var retries = 0;
            limit = limit ?? 20;
            var canContinue = true;

            while (canContinue)
            {
                var result = new List<object>();
                try
                {
                    var response = Client.GetTaskCalendarEventsAsync(JobData.LastCrawlFinishTime, DateTimeOffset.UtcNow, limit.Value, offset).Result;

                    if (response == null || !response.Any())
                        canContinue = false;
                    else
                    {
                        result.AddRange(response);

                        if (response.Count < limit)
                            canContinue = false;
                        else
                        {
                            offset += 100;
                            retries = 0;
                        }
                    }
                }
                catch (ThrottlingException e)
                {
                    if (!ShouldRetryThrottledCall(e, retries))
                    {
                        canContinue = false;
                    }

                    retries++;
                }
                catch
                {
                    Logger.Warn(() => $"Failed to retrieve data in {GetType().FullName}");
                    canContinue = false;
                }

                foreach (var item in result)
                {
                    yield return item;
                }
            }
        }
    }
}
