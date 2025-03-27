namespace BookingTour.Areas.Admin.Controllers;
 using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public class SmsService
{
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _twilioPhoneNumber;

    public SmsService(string accountSid, string authToken, string twilioPhoneNumber)
    {
        _accountSid = accountSid;
        _authToken = authToken;
        _twilioPhoneNumber = twilioPhoneNumber;
        //TwilioClient.Init(_accountSid, _authToken);
    }

    public async Task SendSmsAsync(string toPhoneNumber, string message)
    {
        var to = new PhoneNumber(toPhoneNumber);
        var from = new PhoneNumber(_twilioPhoneNumber);

        await MessageResource.CreateAsync(
            to: to,
            from: from,
            body: message);
    }
}
