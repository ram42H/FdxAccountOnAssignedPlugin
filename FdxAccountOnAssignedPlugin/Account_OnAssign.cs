using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FdxAccountOnAssignedPlugin
{
    public class Account_OnAssign : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //Extract the tracing service for use in debugging sandboxed plug-ins.....
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            //Obtain execution contest from the service provider....
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            int step = 0;

            if (context.InputParameters.Contains("Target"))
            {
                step = 2;
                Entity accountPreImageEntity = ((context.PreEntityImages != null) && context.PreEntityImages.Contains("accpre")) ? context.PreEntityImages["accpre"] : null;

                Entity accountEntity = new Entity
                {
                    LogicalName = "account",
                    Id = accountPreImageEntity.Id
                };

                step = 3;
                if (accountPreImageEntity.LogicalName != "account")
                    return;

                try
                {
                    step = 5;
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                    //Get current user information....
                    step = 6;
                    WhoAmIResponse response = (WhoAmIResponse)service.Execute(new WhoAmIRequest());

                    step = 7;
                    accountEntity["fdx_lastassignedowner"] = new EntityReference("systemuser", ((EntityReference)accountPreImageEntity.Attributes["ownerid"]).Id);

                    step = 8;
                    accountEntity["fdx_lastassigneddate"] = DateTime.UtcNow;

                    step = 9;
                    service.Update(accountEntity);
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException(string.Format("An error occurred in the Account_OnAssign plug-in at Step {0}.", step), ex);
                }
                catch (Exception ex)
                {
                    tracingService.Trace("Account_OnAssign: step {0}, {1}", step, ex.ToString());
                    throw;
                }
            }
        }
    }
}
