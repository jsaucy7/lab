using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TALXIS.DevKit.Build.Dataverse.Plugin;

namespace Plugins.Warehouse
{
    [PluginStep("Create", "lab_warehousetransaction", PluginStage.PreValidation)]
    public class ValidateWarehouseTransactionPlugin : PluginBase
    {
        public ValidateWarehouseTransactionPlugin()
            : base(typeof(ValidateWarehouseTransactionPlugin))
        {
        }

        public ValidateWarehouseTransactionPlugin(string unsecureConfiguration, string secureConfiguration)
            : this()
        {
        }

        protected override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
        {
            var context = localPluginContext.PluginExecutionContext;
            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity target))
            {
                return;
            }

            if (target.LogicalName != "lab_warehousetransaction"
                || !target.Contains("lab_quantity")
                || !target.Contains("lab_itemid")
                || !target.Contains("lab_transactiontype"))
            {
                return;
            }

            var transactionType = (OptionSetValue)target["lab_transactiontype"];
            if (transactionType.Value != 100000001)
            {
                return;
            }

            int quantity;
            int available;
            try
            {
                quantity = (int)target["lab_quantity"];
                var itemReference = (EntityReference)target["lab_itemid"];
                var service = localPluginContext.OrgSvcFactory.CreateOrganizationService(context.UserId);
                var item = service.Retrieve(
                    "lab_warehouseitem",
                    itemReference.Id,
                    new ColumnSet("lab_availablequantity"));

                available = item.Contains("lab_availablequantity")
                    ? (int)item["lab_availablequantity"]
                    : 0;
            }
            catch (Exception exception)
            {
                localPluginContext.TracingService.Trace("Plugin exception: {0}", exception);
                throw;
            }

            if (quantity > available)
            {
                throw new InvalidPluginExecutionException(
                    $"Not enough product in stock. Available: {available}, requested: {quantity}.");
            }
        }
    }
}