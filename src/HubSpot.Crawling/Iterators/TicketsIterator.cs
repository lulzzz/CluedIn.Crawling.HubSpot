﻿using System;
using System.Collections.Generic;
using System.Linq;
using CluedIn.Core.Logging;
using CluedIn.Crawling.HubSpot.Core;
using CluedIn.Crawling.HubSpot.Core.Models;
using CluedIn.Crawling.HubSpot.Infrastructure;
using CluedIn.Crawling.HubSpot.Infrastructure.Exceptions;

namespace CluedIn.Crawling.HubSpot.Iterators
{
    public class TicketsIterator : HubSpotIteratorBase
    {

        private readonly Settings _settings;

        public TicketsIterator(IHubSpotClient client, HubSpotCrawlJobData jobData, Settings properties, ILogger logger)
            : base(client, jobData, logger)
        {
            _settings = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public override IEnumerable<object> Iterate(int? limit = null)
        {
            var offset = 0;
            var retries = 0;
            limit = limit ?? 100;
            var canContinue = true;

            var properties = Client.GetTicketPropertiesAsync(_settings).Result;
            while (canContinue)
            {
                var result = new List<object>();

                try
                {
                    var response = Client.GetTicketsAsync(properties, limit.Value, offset).Result;

                    if (response?.Objects == null || !response.Objects.Any())
                        canContinue = false;

                    else
                    {
                        foreach (var ticket in response.Objects)
                        {
                            if (ticket.ObjectId.HasValue)
                            {
                                try
                                {
                                    var associations = new AssociationsIterator(Client, JobData, ticket.ObjectId.Value, AssociationType.TicketToContact, Logger).Iterate(100).Cast<long>();
                                    ticket.Associations.Contacts.AddRange(associations);
                                }
                                catch (Exception exception)
                                {
                                    Logger.Warn(() => $"Failed to get Associations for Ticket Contacts {ticket.ObjectId.Value}", exception);
                                }

                                try
                                {
                                    var associations = new AssociationsIterator(Client, JobData, ticket.ObjectId.Value, AssociationType.TicketToEngagement, Logger).Iterate(100).Cast<long>();
                                    ticket.Associations.Engagements.AddRange(associations);
                                }
                                catch (Exception exception)
                                {
                                    Logger.Warn(() => $"Failed to get Associations for Ticket Engagements {ticket.ObjectId.Value}", exception);
                                }

                                try
                                {
                                    var associations = new AssociationsIterator(Client, JobData, ticket.ObjectId.Value, AssociationType.TicketToCompany, Logger).Iterate(100).Cast<long>();
                                    ticket.Associations.Companies.AddRange(associations);
                                }
                                catch (Exception exception)
                                {
                                    Logger.Warn(() => $"Failed to get Associations for Ticket Companies {ticket.ObjectId.Value}", exception);
                                }
                            }

                            result.Add(ticket);
                        }

                        if (response.HasMore == false || response.Objects.Count < limit || response.Offset == null)
                            canContinue = false;
                        else
                        {
                            offset = response.Offset.Value;
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
