using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Services;
using Jellyfin.Plugin.PushOverNotifications.Configuration;
using System.Threading.Tasks;
using System.Threading;

namespace Jellyfin.Plugins.PushOverNotifications.Api
{
    [Route("/Notification/Pushover/Test/{UserID}", "POST", Summary = "Tests Pushover")]
    public class TestNotification : IReturnVoid
    {
        [ApiMember(Name = "UserID", Description = "User Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "GET")]
        public string UserID { get; set; }
    }

    class ServerApiEndpoints : IService
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger IloggerFactory;

        public ServerApiEndpoints(ILoggerFactory logManager, IHttpClient httpClient)
        {
            IloggerFactory = logManager.CreateLogger(GetType().Name);
            _httpClient = httpClient;
        }
        private PushOverOptions GetOptions(String userID)
        {
            return Plugin.Instance.Configuration.Options
                .FirstOrDefault(i => string.Equals(i.MediaBrowserUserId, userID, StringComparison.OrdinalIgnoreCase));
        }

        public void Post(TestNotification request)
        {
            var task = PostAsync(request);

            Task.WaitAll(task);
        }

        private async Task PostAsync(TestNotification request)
        {
            var options = GetOptions(request.UserID);

            var parameters = new Dictionary<string, string>
            {
                {"token", options.Token},
                {"user", options.UserKey},
                {"title", "Test Notification" },
                {"message", "This is a test notification from Jellyfin"}
            };

            IloggerFactory.LogDebug("Pushover <TEST> to {0} - {1}", options.Token, options.UserKey);

            var httpRequestOptions = new HttpRequestOptions
            {
                Url = "https://api.pushover.net/1/messages.json",
                CancellationToken = CancellationToken.None
            };

            httpRequestOptions.SetPostData(parameters);

            using (await _httpClient.Post(httpRequestOptions).ConfigureAwait(false))
            {

            }
        }
    }
}
