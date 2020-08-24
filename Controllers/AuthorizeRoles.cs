using System;
using Microsoft.AspNetCore.Authorization;

namespace core_api.Controllers
{
    public class AuthorizeRoles : AuthorizeAttribute
    {
        public AuthorizeRoles(params string[] roles)
        {
            Roles = String.Join(",", roles);
        }
    }
}
