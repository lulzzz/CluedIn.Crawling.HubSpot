﻿using System;
using CluedIn.Core.Data;
using CluedIn.Crawling.Factories;
using CluedIn.Crawling.Helpers;
using CluedIn.Crawling.HubSpot.Core.Models;
using CluedIn.Crawling.HubSpot.Vocabularies;

namespace CluedIn.Crawling.HubSpot.ClueProducers
{
    public class DealPipelineClueProducer : BaseClueProducer<DealPipeline>
    {
        private readonly IClueFactory _factory;

        public DealPipelineClueProducer(IClueFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        protected override Clue MakeClueImpl(DealPipeline input, Guid accountId)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            // TODO: Create clue specifying the type of entity it is and ID
            var clue = _factory.Create(EntityType.News, input.pipelineId, accountId);

            // TODO: Populate clue data
            var data = clue.Data.EntityData;

            data.Name = input.label;
            data.Uri = new Uri($"https://app.hubspot.com/sales-products-settings/{input.portalId}/deals/{input.pipelineId}");

            data.Properties[HubSpotVocabulary.DealPipeline.Active] = input.active.PrintIfAvailable();
            data.Properties[HubSpotVocabulary.DealPipeline.DisplayOrder] = input.displayOrder.PrintIfAvailable();
            data.Properties[HubSpotVocabulary.DealPipeline.PipelineId] = input.pipelineId;

            if (input.stages != null)
            {
                foreach (var stage in input.stages)
                {
                    // TODO: Do not create multiple clues in subjects
                    var stageClue = CreateStageClue(stage, accountId);
                    //this.state.Status.Statistics.Tasks.IncrementTaskCount();
                    //this.state.Status.Statistics.Tasks.IncrementQueuedCount();

                    // TODO Verify how we handle multiple clues and statistics
                    _factory.CreateIncomingEntityReference(clue, EntityType.ProcessStage, EntityEdgeType.PartOf, stage, s => s.stageId);
                }
            }

            _factory.CreateIncomingEntityReference(clue, EntityType.Provider.Root, EntityEdgeType.Parent, input, s => "HubSpot");


            return clue;
        }

        public Clue CreateStageClue(Stage value, Guid accountId)
        {
            var clue = _factory.Create(EntityType.ProcessStage, value.stageId, accountId);
            var data = clue.Data.EntityData;

            data.Name = value.label;

            data.Properties[HubSpotVocabulary.Stage.Active] = value.active.PrintIfAvailable();
            data.Properties[HubSpotVocabulary.Stage.DisplayOrder] = value.displayOrder.PrintIfAvailable();
            data.Properties[HubSpotVocabulary.Stage.Label] = value.label;
            data.Properties[HubSpotVocabulary.Stage.Probability] = value.probability.PrintIfAvailable();
            data.Properties[HubSpotVocabulary.Stage.ClosedWon] = value.closedWon.PrintIfAvailable();

            return clue;
        }

    }
}
