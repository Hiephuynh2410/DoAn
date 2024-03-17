using DoAn.Models;

namespace DoAn.Areas.Admin.Services
{
    public class ForgotPassServices
    {
        private readonly DlctContext _dbContext;

        public ForgotPassServices(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string GetOldPassword(string email, string enteredPassword)
        {
            var user = _dbContext.Staff.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return null;
            }

            return BCrypt.Net.BCrypt.Verify(enteredPassword, user?.Password) ? user.Password : null;
        }

    }
}
