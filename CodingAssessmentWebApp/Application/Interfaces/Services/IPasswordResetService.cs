using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IPasswordResetService
    {
        bool ValidateToken(string email, string submittedToken);
        string GenerateResetToken(string email);
    }
}
