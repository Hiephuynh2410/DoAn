using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DoAn.Models;

namespace DoAn.ClientServices
{
    public class ClientLoginService
    {
        private readonly HttpClient _httpClient;

        public ClientLoginService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }
        public class ErrorResponse
        {
            public string Message { get; set; }
            public string[] Errors { get; set; }
        }
        public async Task<(bool Success, string ErrorMessage)> LoginAsync(Client loginModel)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/v1/ClientLogin/login", loginModel);

                if (response.IsSuccessStatusCode)
                {
                    return (true, null); 
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

                    if (errorResponse?.Message == "Invalid username or password")
                    {
                        return (false, "sai mk");
                    }

                    return (false, "An error occurred during login"); 
                }
                else
                {
                    return (false, "An error occurred during login"); 
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
