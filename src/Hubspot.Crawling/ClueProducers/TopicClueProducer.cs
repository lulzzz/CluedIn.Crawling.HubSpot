﻿using System;
using CluedIn.Core.Data;
using CluedIn.Core.Utilities;
using CluedIn.Crawling.Factories;
using CluedIn.Crawling.HubSpot.Core.Models;
using CluedIn.Crawling.HubSpot.Vocabularies;

namespace CluedIn.Crawling.HubSpot.ClueProducers
{
    public class TopicClueProducer : BaseClueProducer<Topic>
    {
        private readonly IClueFactory _factory;

        public TopicClueProducer(IClueFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        protected override Clue MakeClueImpl(Topic input, Guid accountId)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            // TODO: Create clue specifying the type of entity it is and ID
            var clue = _factory.Create(EntityType.News, input.id.ToString(), accountId);

            // TODO: Populate clue data
            var data = clue.Data.EntityData;

            if (input.created != null)
            {
                data.CreatedDate = DateUtilities.EpochRef.AddMilliseconds(input.created.Value);
                data.Properties[HubSpotVocabulary.Topic.Created] = DateUtilities.EpochRef.AddMilliseconds(input.created.Value).ToString("o");
            }

            if (input.deletedAt != null)
                data.Properties[HubSpotVocabulary.Topic.DeletedAt] = input.deletedAt.ToString();
            if (input.description != null)
            {
                data.Description = input.description;

            }
            if (input.name != null)
            {
                data.Name = input.name;

            }
            if (input.portalId != null)
            {
                _factory.CreateIncomingEntityReference(clue, EntityType.Infrastructure.Site, EntityEdgeType.PartOf, input, s => s.portalId.ToString(), s => "Hubspot");
                data.Properties[HubSpotVocabulary.Topic.PortalId] = input.portalId.ToString();
            }

            if (input.slug != null)
                data.Properties[HubSpotVocabulary.Topic.Slug] = input.slug.ToString();
            if (input.updated != null)
            {
                data.ModifiedDate = DateUtilities.EpochRef.AddMilliseconds(input.updated.Value);
                data.Properties[HubSpotVocabulary.Topic.Updated] = input.updated.ToString();
            }


            return clue;
        }
    }
}
