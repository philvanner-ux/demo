using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace LeadToContactPlugin
{
    public class UpdateContactWithLeadReference : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity lead = (Entity)context.InputParameters["Target"];

                    if (lead.LogicalName != "lead")
                        return;

                    // Get the ID of the created lead
                    Guid leadId = lead.Id;

                    // Check if the lead has a related contact
                    if (lead.Attributes.Contains("parentcontactid") && lead["parentcontactid"] is EntityReference contactRef)
                    {
                        // Create update for the contact
                        Entity contactToUpdate = new Entity("contact", contactRef.Id);
                        contactToUpdate["new_lastleadid"] = new EntityReference("lead", leadId); // Assuming this is a custom lookup field

                        service.Update(contactToUpdate);
                    }
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace("UpdateContactWithLeadReference Plugin: {0}", ex.ToString());
                throw new InvalidPluginExecutionException("An error occurred in the UpdateContactWithLeadReference plugin.", ex);
            }
        }
    }
}
