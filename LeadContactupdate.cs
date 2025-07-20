using System;
using Microsoft.Xrm.Sdk;

namespace LeadUpdatePlugin
{
    public class UpdateContactOnLeadChange : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity targetEntity)
                {
                    if (targetEntity.LogicalName != "lead")
                        return;

                    // Check if parentcontactid is being updated
                    if (!targetEntity.Attributes.Contains("parentcontactid"))
                        return;

                    // Get the new contact reference
                    EntityReference newContactRef = (EntityReference)targetEntity["parentcontactid"];
                    Guid leadId = context.PrimaryEntityId;

                    // Update the new contact with the lead reference
                    Entity contactToUpdate = new Entity("contact", newContactRef.Id);
                    contactToUpdate["new_lastleadid"] = new EntityReference("lead", leadId);

                    service.Update(contactToUpdate);
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace("UpdateContactOnLeadChange Plugin: {0}", ex.ToString());
                throw new InvalidPluginExecutionException("An error occurred in the UpdateContactOnLeadChange plugin.", ex);
            }
        }
    }
}
