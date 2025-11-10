using Microsoft.AspNetCore.Mvc;
using System.Text;
using static System.Net.WebRequestMethods;

namespace DelhiveryOne.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class PincodeController : ControllerBase
    {
        [HttpGet, ActionName("CheckPincode")]
        public async Task<IActionResult> CheckPincode(int pincode)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://track.delhivery.com/c/api/pin-codes/json/?filter_codes={pincode}");
                request.Headers.Add("Authorization", "Token 8114291a1294c9817302b0b69b352c2de2476a24");
                var response = await client.SendAsync(request);

                return Ok(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}