using BookingTour.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

public class VnpayService : IVnPayService
{
    private readonly VnpayConfig _vnpayConfig;
    private readonly IConfiguration _config;

    public VnpayService(IOptions<VnpayConfig> vnpayConfig, IConfiguration config)
    {
        _vnpayConfig = vnpayConfig.Value;
        _config = config;
    }

    public string CreatePaymentUrl(HttpContext context,Invoice invoices)
    {
        var tick = DateTime.Now.Ticks.ToString();
        var pay = new VnPayLibrary();
        var urlCallBack = _config["Vnpay:PaymentBackReturnUrl"]; // Đảm bảo rằng URL trả về trong cấu hình là chính xác

        // Kiểm tra các thông số cấu hình quan trọng
        if (string.IsNullOrEmpty(_config["Vnpay:Version"]) || string.IsNullOrEmpty(_config["Vnpay:HashSecret"]))
        {
            throw new InvalidOperationException("VnPay configuration is not properly set.");
        }

        pay.AddRequestData("vnp_Version", _config["Vnpay:Version"]);
        pay.AddRequestData("vnp_Command", _config["Vnpay:Command"]);
        pay.AddRequestData("vnp_TmnCode", _config["Vnpay:TmnCode"]);
        pay.AddRequestData("vnp_Amount", ((int)(invoices.TotalAmount * 100)).ToString()); // Lưu ý rằng VnPay yêu cầu số tiền ở đơn vị "đồng"
        pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
        pay.AddRequestData("vnp_CurrCode", _config["Vnpay:CurrCode"]);
        pay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context)); // Đảm bảo Utils.GetIpAddress(context) trả về IP chính xác
        pay.AddRequestData("vnp_Locale", _config["Vnpay:Locale"]);
        pay.AddRequestData("vnp_OrderInfo", invoices.IdInvoice.ToString());
        pay.AddRequestData("vnp_OrderType", "VNP");
        pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
        pay.AddRequestData("vnp_TxnRef", tick);

        // Tạo URL yêu cầu thanh toán
        var paymentUrl = pay.CreateRequestUrl(_config["Vnpay:Url"], _config["Vnpay:HashSecret"]);

        return paymentUrl;
    }

    public VnPaymentResponseModel PaymentExecute(IQueryCollection collections)
    {
        var vnPay = new VnPayLibrary();

        // Thêm dữ liệu trả về vào đối tượng vnPay
        foreach (var (key, value) in collections)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnPay.AddResponseData(key, value.ToString());
            }
        }

        // Lấy các giá trị cần thiết từ dữ liệu trả về
        var orderId = Convert.ToInt64(vnPay.GetResponseData("vnp_TxnRef"));
        var vnPayTranId = Convert.ToInt64(vnPay.GetResponseData("vnp_TransactionNo"));
        var vnpResponseCode = vnPay.GetResponseData("vnp_ResponseCode");
        var vnpSecureHash = collections.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value;
        var orderInfo = vnPay.GetResponseData("vnp_OrderInfo");
       
        // Kiểm tra chữ ký
        var checkSignature = vnPay.ValidateSignature(vnpSecureHash, _config["Vnpay:HashSecret"]);

        if (!checkSignature)
        {
            return new VnPaymentResponseModel()
            {
                Success = false,
             
            };
        }
     
        // Sử dụng giá trị string
  
        var orderDescriptionLong = long.Parse(orderInfo);
        return new VnPaymentResponseModel()
        {
            
            Success = true,
            PaymentMethod = "VnPay",
            OrderDescription = orderDescriptionLong,
            OrderId = orderId.ToString(),
            PaymentId = vnPayTranId.ToString(),
            TransactionId = vnPayTranId.ToString(),
            Token = vnpSecureHash,
            VnPayResponseCode = vnpResponseCode
        };
    }
}
