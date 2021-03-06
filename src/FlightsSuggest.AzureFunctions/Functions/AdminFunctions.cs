using System;
using System.IO;
using System.Threading.Tasks;
using FlightsSuggest.AzureFunctions.Implementation;
using FlightsSuggest.AzureFunctions.Implementation.Container;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace FlightsSuggest.AzureFunctions.Functions
{
    public static class AdminFunctions
    {
        [FunctionName("RewindSubscriberOffset")]
        public static Task<IActionResult> RewindSubscriberOffsetAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req,
            ILogger log,
            ExecutionContext context
        )
        {
            return Function.ExecuteAsync(log, nameof(RewindSubscriberOffsetAsync), async () =>
            {
                var subscriberId = req.Query["subscriberId"];
                var timelineName = req.Query["timelineName"];
                var date = req.Query["date"];
                var offset = DateTime.Parse(date).Ticks;

                var flightNotifier = Container.Build(context, log).GetFlightNotifier();
                await flightNotifier.RewindSubscriberOffsetAsync(subscriberId, timelineName, offset);

                return new OkObjectResult("done");
            });
        }

        [FunctionName("RewindVkOffset")]
        public static Task<IActionResult> RewindVkOffsetAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req,
            ILogger log,
            ExecutionContext context
        )
        {
            return Function.ExecuteAsync(log, nameof(RewindVkOffsetAsync), async () =>
            {
                var vkGroup = req.Query["vkGroup"];
                var date = req.Query["date"];
                var offset = DateTime.Parse(date).Ticks;

                var flightNotifier = Container.Build(context, log).GetFlightNotifier();
                await flightNotifier.RewindVkOffsetAsync(vkGroup, offset);

                return new OkObjectResult("done");
            });
        }

        [FunctionName("ShowSubscribers")]
        public static Task<IActionResult> ShowSubscribersAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req,
            ILogger log,
            ExecutionContext context
        )
        {
            return Function.ExecuteAsync(log, nameof(ShowSubscribersAsync), async () =>
            {
                var flightNotifier = Container.Build(context, log).GetFlightNotifier();

                var subscribers = await flightNotifier.SelectSubscribersAsync();

                return new OkObjectResult(subscribers);
            });
        }

        [FunctionName("ShowSubscriberInfo")]
        public static Task<IActionResult> ShowSubscriberInfoAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req,
            ILogger log,
            ExecutionContext context
        )
        {
            return Function.ExecuteAsync(log, nameof(ShowSubscriberInfoAsync), async () =>
            {
                var subscriberId = req.Query["subscriberId"];

                var flightNotifier = Container.Build(context, log).GetFlightNotifier();
                var offsets = await flightNotifier.SelectOffsetsAsync(subscriberId);

                return new OkObjectResult(offsets);
            });
        }

        [FunctionName("ShowVkTimelineOffsets")]
        public static Task<IActionResult> ShowVkTimelineOffsetsAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req,
            ILogger log,
            ExecutionContext context
        )
        {
            return Function.ExecuteAsync(log, nameof(ShowVkTimelineOffsetsAsync), async () =>
            {
                var flightNotifier = Container.Build(context, log).GetFlightNotifier();
                var offsets = await flightNotifier.SelectVkOffsetsAsync();

                return new OkObjectResult(offsets);
            });
        }

        [FunctionName("SetTelegramWebhook")]
        public static Task<IActionResult> SetTelegramWebhookAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            ILogger log,
            ExecutionContext context
        )
        {
            return Function.ExecuteAsync(log, nameof(SetTelegramWebhookAsync), async () =>
            {
                string url;
                using (var streamReader = new StreamReader(req.Body))
                {
                    url = await streamReader.ReadToEndAsync();
                }

                var configuration = ConfigurationProvider.Provide(context);
                var telegramBotClient = new TelegramBotClient(configuration.TelegramBotToken);

                await telegramBotClient.SetWebhookAsync(url);

                return new OkObjectResult("done");
            });
        }
    }
}