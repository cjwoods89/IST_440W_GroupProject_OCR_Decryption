using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using IST440W_OCR_Decryption.Models;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using IST440W_OCR_Decryption.DTOModels;
using System.Linq;

namespace IST440W_OCR_Decryption.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ReadController : Controller
    {
        static string subscriptionKey;
        static string endpoint;
        static string uriBase;
        static string readURIBase;


        // Set main class variables
        public ReadController()
        {
            subscriptionKey = "24ed91821f084f98ba0007e6b513d32b";
            endpoint = "https://ist440wfall2020ocrdecryption.cognitiveservices.azure.com/";
            uriBase = endpoint + "vision/v2.1/ocr";
            readURIBase = endpoint + "vision/v3.0/read/analyze";
        }

        // Task that returns ReadResultDTO, includes turning image into byte array
        [HttpPost, DisableRequestSizeLimit]
        public async Task<ReadResultDTO> Post()
        {
            StringBuilder sb = new StringBuilder();
            ReadResultDTO readResultDTO = new ReadResultDTO();
            try
            {
                if (Request.Form.Files.Count > 0)
                {
                    var file = Request.Form.Files[Request.Form.Files.Count - 1];

                    if (file.Length > 0)
                    {
                        var memoryStream = new MemoryStream();
                        file.CopyTo(memoryStream);
                        byte[] imageFileBytes = memoryStream.ToArray();
                        memoryStream.Flush();

                        string JSONResult = await ReadTextFromStream(imageFileBytes);

                        TextRecognitionResult textResult = JsonConvert.DeserializeObject<TextRecognitionResult>(JSONResult);
                        foreach (Line textLine in textResult.Lines)
                        {
                            foreach (Word word in textLine.Words)
                            {
                                sb.Append(word.Text);
                                sb.Append(' ');
                            }
                            sb.AppendLine();
                        }

                        readResultDTO.DetectedText = sb.ToString();
                        readResultDTO.Language = "en";
                    }
                }
                return readResultDTO;
            }
            catch
            {
                readResultDTO.DetectedText = "Error occurred. Try again";
                readResultDTO.Language = "unk";
                return readResultDTO;
            }
        }

        // Task that converts image to byte array
        static async Task<string> ReadTextFromStream(byte[] byteData)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                string requestParameters = "language=unk&detectOrientation=true";
                string uri = readURIBase + "?" + requestParameters;
                HttpResponseMessage response;

                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    response = await client.PostAsync(uri, content);
                }

                string contentString = await response.Content.ReadAsStringAsync();
                string result = JToken.Parse(contentString).ToString();
                return result;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }


        // Task that returns all available languages
        [HttpGet]
        public async Task<List<AvailableLanguageDTO>> GetAvailableLanguages()
        {
            string endpoint = "https://api.cognitive.microsofttranslator.com/languages?api-version=3.0&scope=translation";
            var client = new HttpClient();
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(endpoint);
                var response = await client.SendAsync(request).ConfigureAwait(false);
                string result = await response.Content.ReadAsStringAsync();

                AvailableLanguage deserializedOutput = JsonConvert.DeserializeObject<AvailableLanguage>(result);

                List<AvailableLanguageDTO> availableLanguage = new List<AvailableLanguageDTO>();

                foreach (KeyValuePair<string, LanguageDetails> translation in deserializedOutput.Translation)
                {
                    AvailableLanguageDTO language = new AvailableLanguageDTO();
                    language.LanguageID = translation.Key;
                    language.LanguageName = translation.Value.Name;

                    availableLanguage.Add(language);
                }
                return availableLanguage;
            }
        }
    }
}