﻿using System.Threading.Tasks;
using FlightsSuggest.Core.Configuration;
using FlightsSuggest.Core.Infrastructure.Vkontakte;
using FlightsSuggest.Core.Notifications;
using FlightsSuggest.Core.Timelines;

namespace FlightsSuggest.AzureFunctions.Implementation
{
    public class FlightNotifier
    {
        private readonly IFlightsConfiguration configuration;
        private readonly VkontakteClient vkontakteClient;

        public FlightNotifier(
            IFlightsConfiguration configuration
            )
        {
            this.configuration = configuration;
            vkontakteClient = new VkontakteClient(configuration.VkApplicationId, configuration.VkAccessToken);
        }

        public FlightNews[] Sended { get; private set; }

        public async Task NotifyAsync()
        {
            var offsetStorage = new AzureTableOffsetStorage(configuration);

            var vkontakteTimeline = new VkontakteTimeline(
                "vandroukiru",
                offsetStorage,
                new InMemoryFlightNewsStorage(),
                vkontakteClient,
                new FlightNewsFactory()
            );

            await vkontakteTimeline.ActualizeAsync();

            var inMemoryNotificationSender = new InMemoryNotificationSender();
            var notificationSenders = new[] { inMemoryNotificationSender, };
            var subscriber = new Subscriber("nick", null, false, new[] { new TermNotificationTrigger("Греци"), });
            var notifier = new Notifier(notificationSenders, new[] { subscriber, }, new[] { vkontakteTimeline }, offsetStorage);

            await notifier.NotifyAsync();
            Sended = inMemoryNotificationSender.Sended;
        }
    }
}