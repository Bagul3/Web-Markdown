using Common;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DataService.Helper
{
    public class LoggingHandler : DelegatingHandler
    {
        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            new LogWriter().LogWrite("Request:");
            new LogWriter().LogWrite(request.ToString());
            if (request.Content != null)
            {
                new LogWriter().LogWrite(await request.Content.ReadAsStringAsync());
            }
            Console.WriteLine();

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            new LogWriter().LogWrite("Response:");
            new LogWriter().LogWrite(response.ToString());
            if (response.Content != null)
            {
                new LogWriter().LogWrite(await response.Content.ReadAsStringAsync());
            }
            Console.WriteLine();

            return response;
        }
    }
}
