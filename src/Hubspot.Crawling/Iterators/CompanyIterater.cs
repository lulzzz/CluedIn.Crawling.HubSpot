﻿using System;
using System.Collections.Generic;
using System.Linq;
using CluedIn.Crawling.HubSpot.Core;
using CluedIn.Crawling.HubSpot.Core.Models;
using CluedIn.Crawling.HubSpot.Infrastructure;

namespace CluedIn.Crawling.HubSpot.Iterators
{
    public class CompanyIterator : HubSpotIteratorBase
    {
        private readonly IList<string> _properties;
        private readonly Settings _settings;

        public CompanyIterator(IHubSpotClient client, HubSpotCrawlJobData jobData, IList<string> properties, Settings settings) : base(client, jobData)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public override IEnumerable<object> Iterate(int? limit = null)
        {
            int offset = 0;
            long portalId = 0;
            limit = limit ?? 100;
            while (true)
            {
                var response = Client.GetCompaniesAsync(_properties, limit.Value, offset).Result;

                if (response.results == null || !response.results.Any())
                    break;

                foreach (var company in response.results)
                {
                    if (_settings?.currency != null)
                        company.Currency = _settings.currency;

                    if (company.portalId.HasValue)
                        portalId = company.portalId.Value;

                    yield return company;

                    if (company.companyId.HasValue)
                    {
                        var contacts = Client.GetContactsByCompanyAsync(company.companyId.Value).Result;
                        foreach (var contact in contacts.contacts)
                        {
                            yield return contact;
                        }

                        var engagements = Client.GetEngagementByIdAndTypeAsync(company.companyId.Value, "COMPANY").Result;
                        foreach (var engagement in engagements)
                        {
                            yield return engagement;
                        }
                    }
                }

                if (response.hasMore == false || response.offset == null || response.results.Count < limit)
                    break;

                offset = response.offset.Value;
            }

            // TODO Is this correct? Just get deal pipelines for last company portal id?
            var dealPipelines = Client.GetDealPipelinesAsync().Result;
            foreach (var dealPipeline in dealPipelines)
            {
                dealPipeline.portalId = portalId;
                yield return dealPipeline;
            }

            //var tables = Client.GetTablesAsync().Result;

            //foreach (var table in tables)
            //{
            //    table.PortalId = portalId;

            //    yield return table;
            //    foreach (var row in new TableRowsIterator(Client, JobData, table, portalId).Iterate())
            //    {
            //        yield return row;
            //    }
            //}
        }

    }
}
