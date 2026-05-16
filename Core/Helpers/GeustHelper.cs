using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Helpers
{
    public static class GuestHelper
    {
        public static string GetGuestId(HttpContext httpContext)
        {
            if (httpContext.Request.Cookies
                .ContainsKey("GuestId"))
            {
                return httpContext
                    .Request
                    .Cookies["GuestId"]!;
            }

            var guestId = Guid.NewGuid().ToString();

            httpContext.Response.Cookies.Append(
                "GuestId",
                guestId,
                new CookieOptions
                {
                    Expires =DateTimeOffset.UtcNow.AddDays(30),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None
                });

            return guestId;
        }
    }
}
