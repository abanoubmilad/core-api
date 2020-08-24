using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using core_api.Data;
using core_api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace core_api.Extentions
{
    public static class AuthExtentions
    {

        public static string GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public static string GetUserEmail(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirstValue(ClaimTypes.Email);
        }

        public static string GetUserRole(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirstValue(ClaimTypes.Role);
        }





        // role configuration
        // Role policies
        public static bool IsAdmin(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.GetUserRole() == Role.Admin;
        }
        public static bool CanManage(this ClaimsPrincipal claimsPrincipal)
        {
            var role = claimsPrincipal.GetUserRole();
            return role == Role.Admin || role == Role.Manager;
        }
        public static bool CanOrganize(this ClaimsPrincipal claimsPrincipal)
        {
            var role = claimsPrincipal.GetUserRole();
            return role == Role.Admin || role == Role.Manager || role == Role.Organizer;
        }

        public static bool HasAccessToUser(this ClaimsPrincipal claimsPrincipal, string userId)
        {
            return claimsPrincipal.IsAdmin() || claimsPrincipal.GetUserId() == userId;
        }



        // role configuration
        public static async Task<Firm> GetFirmIfHasMinPermissionOf(this ClaimsPrincipal claimsPrincipal,
            EfRepository repository,
            int firmId, Permission minPermission)
        {
            return claimsPrincipal.IsAdmin() ? await repository.FindByIdAsync<Firm>(firmId)
                       : await repository.Query<Firm>()
                           .Where(firm =>
                           firm.Id == firmId &&
                           firm.FirmUsers.Any(firmUser =>
                                   firmUser.UserId == claimsPrincipal.GetUserId() &&
                                   firmUser.Permission >= minPermission
                                   ))
                          .FirstOrDefaultAsync();
        }

        public static async Task<Meeting> GetMeetingIfHasMinPermissionOf(this ClaimsPrincipal claimsPrincipal,
                        EfRepository repository,
        long meetingId, Permission minPermission)
        {
            return claimsPrincipal.IsAdmin() ? await repository.FindByIdAsync<Meeting>(meetingId)
                       : await repository.Query<Meeting>()
                           .Where(meeting =>
                           meeting.Id == meetingId &&
                           meeting.Firm.FirmUsers.Any(firmUser =>
                                   firmUser.UserId == claimsPrincipal.GetUserId() &&
                                   firmUser.Permission >= minPermission
                                   ))
                          .FirstOrDefaultAsync();
        }

        public static async Task<Booking> GetBookingIfHasMinPermissionOf(this ClaimsPrincipal claimsPrincipal,
                        EfRepository repository,
        long bookingId, Permission minPermission)
        {
            return claimsPrincipal.IsAdmin() ? await repository.FindByIdAsync<Booking>(bookingId)
                       : await repository.Query<Booking>()
                           .Where(booking =>
                           booking.Id == bookingId &&
                           (booking.BookedBy.Id == claimsPrincipal.GetUserId() ||
                           booking.Meeting.Firm.FirmUsers.Any(firmUser =>
                                   firmUser.UserId == claimsPrincipal.GetUserId() &&
                                   firmUser.Permission >= minPermission
                           )))
                          .FirstOrDefaultAsync();
        }





        public static string CreateToken(this User user, JwtConfig jwtConfig)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(jwtConfig.LifetimeInMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key)),
                    SecurityAlgorithms.HmacSha512Signature
                )
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

    }
}
