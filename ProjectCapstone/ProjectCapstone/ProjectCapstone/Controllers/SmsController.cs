using BusinessObject.DTOS;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmsController : ControllerBase
    {
        private readonly string _accountSid;
        private readonly string _authToken; 
        private readonly string _twilioPhoneNumber; 

        public SmsController(IConfiguration configuration)
        {
            _accountSid = configuration["Twilio:AccountSid"];
            _authToken = configuration["Twilio:AuthToken"];
            _twilioPhoneNumber = configuration["Twilio:PhoneNumber"];

            TwilioClient.Init(_accountSid, _authToken);
        }

        [HttpPost("SendSms")]
        public async Task<IActionResult> SendSms([FromBody] SmsRequest smsRequest)
        {
            try
            {
                var message = await MessageResource.CreateAsync(
                    to: new PhoneNumber(smsRequest.RecipientPhoneNumber),
                    from: new PhoneNumber(_twilioPhoneNumber),
                    body: smsRequest.Body
                );

                return Ok($"Message sent! SID: {message.Sid}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
