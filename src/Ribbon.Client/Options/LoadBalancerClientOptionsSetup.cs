﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ribbon.Client.Impl;
using Ribbon.Client.Options;
using Ribbon.LoadBalancer;
using Ribbon.LoadBalancer.Impl;
using System;

namespace Ribbon.Client
{
    public class LoadBalancerClientOptionsSetup : IConfigureNamedOptions<LoadBalancerClientOptions>, IPostConfigureOptions<LoadBalancerClientOptions>
    {
        private readonly IOptionsMonitor<LoadBalancerOptions> _loadBalancerOptionsMonitor;
        private readonly IOptionsMonitor<RetryHandlerConfig> _retryHandlerOptionsMonitor;
        private readonly IOptionsMonitor<LoadBalancerClientConfig> _loadBalancerClientConfigMonitor;

        public LoadBalancerClientOptionsSetup(IOptionsMonitor<LoadBalancerOptions> loadBalancerOptionsMonitor, IOptionsMonitor<RetryHandlerConfig> retryHandlerOptionsMonitor, IOptionsMonitor<LoadBalancerClientConfig> loadBalancerClientConfigMonitor)
        {
            _loadBalancerOptionsMonitor = loadBalancerOptionsMonitor;
            _retryHandlerOptionsMonitor = retryHandlerOptionsMonitor;
            _loadBalancerClientConfigMonitor = loadBalancerClientConfigMonitor;
        }

        #region Implementation of IConfigureOptions<in LoadBalancerClientOptions>

        /// <inheritdoc/>
        public void Configure(LoadBalancerClientOptions options)
        {
            Configure(Microsoft.Extensions.Options.Options.DefaultName, options);
        }

        #endregion Implementation of IConfigureOptions<in LoadBalancerClientOptions>

        #region Implementation of IConfigureNamedOptions<in LoadBalancerClientOptions>

        /// <inheritdoc/>
        public void Configure(string name, LoadBalancerClientOptions options)
        {
            var loadBalancerOptions = _loadBalancerOptionsMonitor.Get(name);

            var loadBalancer = new DynamicServerListLoadBalancer(name, loadBalancerOptions);

            options.LoadBalancer = loadBalancer;

            var loadBalancerClientConfig = _loadBalancerClientConfigMonitor.Get(name);

            var clientType = Type.GetType(loadBalancerClientConfig.ClientTypeName);
            if (clientType != null)
            {
                options.Creator = (clientName, services) =>
                    (IClient)ActivatorUtilities.CreateInstance(services, clientType, clientName);
            }
        }

        #endregion Implementation of IConfigureNamedOptions<in LoadBalancerClientOptions>

        #region Implementation of IPostConfigureOptions<in LoadBalancerClientOptions>

        /// <inheritdoc/>
        public void PostConfigure(string name, LoadBalancerClientOptions options)
        {
            if (options.RetryHandler == null)
            {
                var retryHandlerOptions = _retryHandlerOptionsMonitor.Get(name);
                options.RetryHandler = new DefaultLoadBalancerRetryHandler(retryHandlerOptions);
            }
        }

        #endregion Implementation of IPostConfigureOptions<in LoadBalancerClientOptions>
    }
}