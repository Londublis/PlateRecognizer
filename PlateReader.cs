using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Azure;
using Newtonsoft.Json;

namespace PlateRecognizer
{
    public class PlateReader
    {
        /// <summary>
        /// Read a plate number from pictures.
        /// </summary>
        /// <param name="postUrl">API Url.</param>
        /// <param name="token">Authentification Token.</param>
        /// <returns></returns>
        public static async Task<PlateReaderResult?> Read(string postUrl, byte[] bytes, string? region, string token)
        {
            PlateReaderResult? result;

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("token", token);
            client.DefaultRequestHeaders.Add("region", region);

            using var content = new MultipartFormDataContent

                {
                    { new StringContent(Convert.ToBase64String(bytes)), "upload" },

                };

            try
            {
                var response = await client.PostAsync(postUrl, content);
                var responseText = GetResponseText(response);

                result = JsonConvert.DeserializeObject<PlateReaderResult>(responseText);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return result;
        }

        public static string? GetResponseText(HttpResponseMessage? response)
        {
            string responseText = string.Empty;

            using (var reader = new StreamReader(response.Content.ReadAsStream()))
            {
                responseText = reader.ReadToEnd();
            }
            return responseText;
        }
    }
}
