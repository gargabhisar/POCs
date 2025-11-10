using DelhiveryOne.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DelhiveryOne.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class CreateOrderController : ControllerBase
    {
        [HttpPost, ActionName("CreateOrder")]
        public async Task<IActionResult> CreateOrder(Shipment shipment)
        {
            try
            {
                var payload = new DelhiveryPayload
                {
                    shipments = new List<Shipment>() { shipment }
                };

                string jsonData = JsonSerializer.Serialize(payload);
                string formData = $"format=json&data={jsonData}";

                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://track.delhivery.com/api/cmu/create.json"),
                    Headers =
                    {
                        { "Authorization", "Token 8114291a1294c9817302b0b69b352c2de2476a24" },
                        { "Accept", "application/json" },
                    },
                    Content = new StringContent(formData)
                    {
                        Headers =
                        {
                            ContentType = new MediaTypeHeaderValue("text/plain")
                        }
                    }
                };
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    return Ok(body);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
