﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetCatalogExtensionViewBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Ajsuth.Foundation.Catalogs.ComponentExtension.Engine.Components;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System.Collections.Generic;

namespace Ajsuth.Foundation.Catalogs.ComponentExtension.Engine.Pipelines.Blocks
{
    /// <summary>Defines the synchronous executing GetCatalogExtensionView pipeline block</summary>
    /// <seealso cref="SyncPipelineBlock{TInput, TOutput, TContext}" />
    [PipelineDisplayName(ComponentExtensionConstants.Pipelines.Blocks.GetCatalogExtensionView)]
    public class GetCatalogExtensionViewBlock : SyncPipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        /// <summary>Executes the pipeline block's code logic.</summary>
        /// <param name="entityView">The entity view.</param>
        /// <param name="context">The context.</param>
        /// <returns>The <see cref="EntityView"/>.</returns>
        public override EntityView Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            // Ensure parameters are provided
            Condition.Requires(entityView, nameof(entityView)).IsNotNull();
            Condition.Requires(context, nameof(context)).IsNotNull();

            var viewsPolicy = context.GetPolicy<KnownCatalogViewsPolicy>();
            var request = context.CommerceContext.GetObject<EntityViewArgument>();

            // Determine the context of rendering the entity view, e.g. create/edit or view (page or via commerce context)
            var isViewAction = (request.ViewName.EqualsOrdinalIgnoreCase(viewsPolicy.Master) ||
                request.ViewName.EqualsOrdinalIgnoreCase(viewsPolicy.ConnectCatalog)) &&
                string.IsNullOrWhiteSpace(request.ForAction);
            var isEditAction = string.IsNullOrWhiteSpace(request.ItemId) &&
                request.ViewName.EqualsOrdinalIgnoreCase("ExtensionProperties") &&
                request.ForAction.EqualsOrdinalIgnoreCase("EditExtensionProperties");

            // Validate the context of the request, i.e. entity, view name, and action name
            if (!(request?.Entity is Catalog) ||
                (!isViewAction &&
                !isEditAction))
            {
                return entityView;
            }

            var catalog = request.Entity as Catalog;
            var extensionComponent = catalog.GetComponent<CatalogExtensionComponent>();

            var propertiesEntityView = entityView;
            if (isViewAction)
            {
                // Create an entity view to host the component properties
                propertiesEntityView = new EntityView
                {
                    DisplayName = "Extended Information",
                    Name = "Extended Information",
                    EntityId = entityView.EntityId,
                    EntityVersion = entityView.EntityVersion,
                    ItemId = entityView.ItemId
                };

                // Add the entity view as a child of the current entity view
                entityView.ChildViews.Add(propertiesEntityView);
            }

            // Add the entity view as a child of the page entity view
            propertiesEntityView.Properties.AddRange(new List<ViewProperty>
            {
                CreateViewProperty(nameof(extensionComponent.Description), extensionComponent.Description)
            });

            return entityView;
        }

        /// <summary>
        /// Helper method to simplify ViewProperty creation
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="uiType">The UI type that modifies how the value of the property is rendered in BizFx.</param>
        /// <returns></returns>
        private ViewProperty CreateViewProperty(string name, object value, string uiType = "")
        {
            return new ViewProperty
            {
                Name = name,
                RawValue = value,
                UiType = uiType
            };
        }
    }
}