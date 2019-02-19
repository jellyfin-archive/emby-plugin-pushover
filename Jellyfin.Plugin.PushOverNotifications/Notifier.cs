using System.Collections.Generic;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Notifications;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.PushOverNotifications.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.PushOverNotifications
{
    public class Notifier : INotificationService
    {
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;

        public Notifier(ILoggerFactory logManager, IHttpClient httpClient)
        {
            _logger = logManager.CreateLogger("Pushover Plugin");
            _httpClient = httpClient;
        }

        public bool IsEnabledForUser(User user)
        {
            var options = GetOptions(user);

            return options != null && IsValid(options) && options.Enabled;
        }

        private PushOverOptions GetOptions(User user)
        {
            return Plugin.Instance.Configuration.Options
                .FirstOrDefault(i => string.Equals(i.MediaBrowserUserId, user.Id.ToString("N"), StringComparison.OrdinalIgnoreCase));
        }

        public string Name
        {
            get { return Plugin.Instance.Name; }
        }

        public async Task SendNotification(UserNotification request, CancellationToken cancellationToken)
        {
            var options = GetOptions(request.User);

            var parameters = new Dictionary<string, string>
                {
                    {"token", options.Token},
                    {"user", options.UserKey},
                };

            // TODO: Improve this with escaping based on what PushOver api requires
            var messageTitle = request.Name.Replace("&", string.Empty);

            if (!string.IsNullOrEmpty(options.DeviceName))
                parameters.Add("device", options.DeviceName);

            if (string.IsNullOrEmpty(request.Description))
                parameters.Add("message", messageTitle);
            else
            {
                parameters.Add("title", messageTitle);
                parameters.Add("message", request.Description);
            }

            _logger.LogDebug("PushOver to Token : {0} - {1} - {2}", options.Token, options.UserKey, request.Description);

            var httpRequestOptions = new HttpRequestOptions
            {
                Url = "https://api.pushover.net/1/messages.json",
                CancellationToken = cancellationToken
            };

            httpRequestOptions.SetPostData(parameters);

            using (await _httpClient.Post(httpRequestOptions).ConfigureAwait(false))
            {

            }
        }

        private bool IsValid(PushOverOptions options)
        {
            return !string.IsNullOrEmpty(options.UserKey) &&
                !string.IsNullOrEmpty(options.Token);
        }
    }
}
