﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Core.Logging;
using CluedIn.Core.Utilities;
using CluedIn.Crawling.Factories;
using CluedIn.Crawling.HubSpot.Core.Models;
using CluedIn.Crawling.HubSpot.Vocabularies;

namespace CluedIn.Crawling.HubSpot.ClueProducers
{
    public class NoteClueProducer : BaseClueProducer<Note>
    {
        private readonly IClueFactory _factory;
        private readonly ILogger _log;

        public NoteClueProducer(IClueFactory factory, ILogger log)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        protected override Clue MakeClueImpl(Note input, Guid accountId)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var clue = _factory.Create(EntityType.Note, input.engagement.id.ToString(), accountId);
            
            clue.ValidationRuleSuppressions.Add(Constants.Validation.Rules.EDGES_001_Outgoing_Edge_MustExist);
            clue.ValidationRuleSuppressions.Add(Constants.Validation.Rules.EDGES_002_Incoming_Edge_ShouldNotExist);
            clue.ValidationRuleSuppressions.Add(Constants.Validation.Rules.PROPERTIES_002_Unknown_VocabularyKey_Used);
            clue.ValidationRuleSuppressions.Add(Constants.Validation.Rules.METADATA_002_Uri_MustBeSet);


            var data = clue.Data.EntityData;

            if (input.attachments != null)
            {
                foreach (var attachment in input.attachments)
                {
                    var r = JsonUtility.Deserialize<KeyValuePair<string, object>>(JsonUtility.Serialize(attachment));

                    if (r.Key == "id")
                        _factory.CreateIncomingEntityReference(clue, EntityType.Files.File, EntityEdgeType.Parent, input, selector => r.Value.ToString());
                }
            }

            if (input.associations != null)
            {
                if (input.associations.companyIds != null)
                {
                    foreach (var companyId in input.associations.companyIds)
                        _factory.CreateIncomingEntityReference(clue, EntityType.Organization, EntityEdgeType.Parent, input, selector => companyId.ToString());
                }
                if (input.associations.contactIds != null)
                {
                    foreach (var contactId in input.associations.contactIds)
                        _factory.CreateIncomingEntityReference(clue, EntityType.Infrastructure.Contact, EntityEdgeType.Parent, input, selector => contactId.ToString());
                }
                if (input.associations.dealIds != null)
                {
                    foreach (var dealId in input.associations.dealIds)
                        _factory.CreateIncomingEntityReference(clue, EntityType.Sales.Deal, EntityEdgeType.Parent, input, selector => dealId.ToString());
                }
                if (input.associations.ownerIds != null)
                {
                    foreach (var ownerId in input.associations.ownerIds)
                        _factory.CreateIncomingEntityReference(clue, EntityType.Person, EntityEdgeType.OwnedBy, input, selector => ownerId.ToString());
                }
                if (input.associations.workflowIds != null)
                {
                    foreach (var workflowId in input.associations.workflowIds)
                        _factory.CreateIncomingEntityReference(clue, EntityType.Process, EntityEdgeType.Parent, input, selector => workflowId.ToString());
                }
            }

            if (input.engagement != null)
            {
                if (input.engagement.createdAt != null)
                {
                    if (long.TryParse(input.engagement.createdAt.ToString(), out long date))
                        data.CreatedDate = DateUtilities.EpochRef.AddMilliseconds(date);
                }
                if (input.engagement.lastUpdated != null)
                {
                    if (long.TryParse(input.engagement.lastUpdated.ToString(), out long date))
                        data.CreatedDate = DateUtilities.EpochRef.AddMilliseconds(date);
                }
                if (input.engagement.timestamp != null)
                {
                    if (data.CreatedDate == null)
                    {
                        if (long.TryParse(input.engagement.createdAt.ToString(), out long date))
                            data.CreatedDate = DateUtilities.EpochRef.AddMilliseconds(date);
                    }
                }
                if (input.engagement.active != null)
                {
                    data.Properties[HubSpotVocabulary.Email.Active] = input.engagement.active.Value.ToString();
                }
                if (input.engagement.createdBy != null)
                {
                    _factory.CreateIncomingEntityReference(clue, EntityType.Person, EntityEdgeType.CreatedBy, input, selector => input.engagement.createdBy.ToString());
                }
                if (input.engagement.modifiedBy != null)
                {
                    _factory.CreateIncomingEntityReference(clue, EntityType.Person, EntityEdgeType.ModifiedBy, input, selector => input.engagement.modifiedBy.ToString());
                }
                if (input.engagement.ownerId != null)
                {
                    _factory.CreateIncomingEntityReference(clue, EntityType.Person, EntityEdgeType.OwnedBy, input, selector => input.engagement.ownerId.ToString());
                }
                if (input.engagement.portalId != null)
                {
                    _factory.CreateIncomingEntityReference(clue, EntityType.Infrastructure.Site, EntityEdgeType.Parent, input, selector => input.engagement.portalId.ToString());
                }
            }

            try
            {
                var metadata = JsonUtility.Deserialize<Dictionary<string, object>>(JsonUtility.Serialize(input.metadata));

                if (metadata != null)
                {
                    foreach (var property in metadata)
                    {
                        if (property.Key == "body")
                        {
                            if (property.Value != null && !string.IsNullOrEmpty(property.Value.ToString()))
                            {
                                data.Name = Regex.Replace(property.Value.ToString(), "<.*?>", String.Empty);
                                data.Description = Regex.Replace(property.Value.ToString(), "<.*?>", String.Empty);
                                data.Properties[HubSpotVocabulary.Note.Body] = property.Value.ToString();
                            }
                        }
                        else
                        {
                            if (property.Value != null)
                                data.Properties[string.Format("hubspot.note.custom-{0}", property.Key)] = property.Value.ToString();
                        }
                    }
                }

            }

            catch (Exception exception)
            {
               _log.Error(() => "Failed to parse metadata for Hubspot Note", exception);
            }
            if (data.Name == null)
                data.Name = input.engagement.type + " at " + data.CreatedDate.Value.ToString("MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture);


            return clue;
        }
    }
}
