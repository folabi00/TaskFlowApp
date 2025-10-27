using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Core.Models;

namespace TaskFlow.Infrastructure.Helpers
{
    public class MessageFormatter
    {
        public static string FormatEmailConfirmationBody(Guid userId, User user, string token, string baseUrl)
        {
            var link = $"{baseUrl}/api/User/confirm-email?userId={userId}&token={Uri.EscapeDataString(token)}";

            var body = $@"
            <p>Hi, {user.LastName.ToUpper()}, {user.FirstName}</p>
            <p>Click <a href='{link}'>this link</a> to confirm your email address. This link will expire in 1 hour </p><br> </br>
            <p>If you did not create an account, ignore this email.</p>";
            return body;
        }

        public static TEnum? ParseEnum<TEnum>(string input, bool ignoreCase = true) where TEnum : struct, Enum
        {
            if (Enum.TryParse<TEnum>(input, ignoreCase, out var result))
                return result;

            return null;
        }
    }
}
