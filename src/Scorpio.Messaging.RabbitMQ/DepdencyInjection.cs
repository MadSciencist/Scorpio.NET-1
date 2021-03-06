﻿using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Scorpio.Messaging.Abstractions;
using System;

namespace Scorpio.Messaging.RabbitMQ
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRabbitMqConnection(this IServiceCollection services, IConfiguration config)
        {
            return services.AddSingleton<IRabbitMqConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<RabbitMqConnection>>();

                var hostname = config["RabbitMq:host"];
                var port = config["RabbitMq:port"];
                var user = config["RabbitMq:userName"];
                var password = config["RabbitMq:password"];
                var virtualHost = config["RabbitMq:virtualHost"];

                logger.LogInformation("************************************");
                logger.LogInformation($"RabbitMQ connecting: {user}:{password}@{hostname}:{port}");
                logger.LogInformation("************************************");

                var factory = new ConnectionFactory
                {
                    HostName = hostname,
                    Port = int.Parse(port),
                    UserName = user,
                    Password = password,
                    VirtualHost = virtualHost
                };

                return new RabbitMqConnection(factory, logger);
            });
        }

        public static IServiceCollection AddRabbitMqEventBus(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IEventBusSubscriptionManager, GenericEventBusSubscriptionManager>();
            services.AddSingleton<IEventBus, RabbitMqEventBus>(provider =>
            {
                var conn = provider.GetRequiredService<IRabbitMqConnection>();
                var logger = provider.GetRequiredService<ILogger<RabbitMqEventBus>>();
                var scope = provider.GetRequiredService<ILifetimeScope>();
                var subsManager = provider.GetRequiredService<IEventBusSubscriptionManager>();

                var rabbitConfig = new RabbitConfig
                {
                    ExchangeName = config["RabbitMq:exchangeName"],
                    MyQueueName = config["RabbitMq:myQueueName"],
                    MessageTimeToLive = config["RabbitMq:messageTTL"]
                };

                return new RabbitMqEventBus(conn, logger, scope, subsManager, rabbitConfig);
            });

            return services;
        }
    }
}
