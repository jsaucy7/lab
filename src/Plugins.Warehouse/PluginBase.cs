using Microsoft.Xrm.Sdk;
using System;

namespace Plugins.Warehouse
{
    public interface ILocalPluginContext
    {
        IPluginExecutionContext PluginExecutionContext { get; }
        IOrganizationServiceFactory OrgSvcFactory { get; }
        ITracingService TracingService { get; }
    }

    internal sealed class LocalPluginContext : ILocalPluginContext
    {
        public LocalPluginContext(
            IPluginExecutionContext pluginExecutionContext,
            IOrganizationServiceFactory orgSvcFactory,
            ITracingService tracingService)
        {
            PluginExecutionContext = pluginExecutionContext;
            OrgSvcFactory = orgSvcFactory;
            TracingService = tracingService;
        }

        public IPluginExecutionContext PluginExecutionContext { get; private set; }
        public IOrganizationServiceFactory OrgSvcFactory { get; private set; }
        public ITracingService TracingService { get; private set; }
    }

    public abstract class PluginBase : IPlugin
    {
        private readonly Type _pluginType;

        protected PluginBase(Type pluginType)
        {
            _pluginType = pluginType ?? throw new ArgumentNullException(nameof(pluginType));
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var executionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var localPluginContext = new LocalPluginContext(executionContext, serviceFactory, tracingService);

            try
            {
                ExecuteDataversePlugin(localPluginContext);
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception exception)
            {
                tracingService.Trace("Unhandled exception in {0}: {1}", _pluginType.FullName, exception);
                throw;
            }
        }

        protected abstract void ExecuteDataversePlugin(ILocalPluginContext localPluginContext);
    }
}
