using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructures.EmailServices
{
    public class EmailSenderHandler : IRequestHandler<EmailSenderCommand, bool>
    {
        public async Task<bool> Handle(EmailSenderCommand request, CancellationToken cancellationToken)
        {
            string url = "api/Communication/sendoneemail";
            var client = InitializeHttpClient();
            client.DefaultRequestHeaders.Add("channel", "pdel987654321ch1");

            var serialized = JsonConvert.SerializeObject(request);


            var content = new StringContent(serialized, Encoding.UTF8, "application/json");

            using (var httpResponse = await client.PostAsync(url, content))
            {
                if (httpResponse == null)
                {
                    throw new Exception("CAN NOT send email at this time");
                }

                return httpResponse.IsSuccessStatusCode ? true : false;

            }

        }

        private HttpClient InitializeHttpClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://elo.projectdriveng.com.ng/"); //cdacollections.projectdriveng.com.ng https://elo.projectdriveng.com.ng/
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;

        }
    }
}
