using DoAn.Models;
using DoAn.Models;
using ProGCoder_MomoAPI.Models.Momo;
using ProGCoder_MomoAPI.Models.Order;

namespace DoAn.Services;

public interface IMomoService
{
    Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model);
    MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
}