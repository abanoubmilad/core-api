using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace core_api.Models
{

    public enum ServiceError
    {
        None,
        Unauthorized,
        Forbid,
        BadRequest,
        NotFound,
        Failed
    }

    public class ServiceResponse
    {
        [JsonIgnore]
        public ServiceError ServiceError { get; set; } = ServiceError.None;

        public string Error { get; set; }

        public bool Succeeded() => ServiceError == ServiceError.None;

        // errors
        public void FailIfNoChanges(int changedCount)
        {
            if (changedCount == 0)
            {
                FailOperation();
            }
        }
        public void FailOperation(string error = null)
        {
            ServiceError = ServiceError.Failed;
            Error = error ?? "Could not perform operation.";
        }
        public void FailNotFound()
        {
            ServiceError = ServiceError.NotFound;
        }
        public void Fail(IdentityResult identityResult)
        {
            ServiceError = ServiceError.Unauthorized;
            Error = identityResult.ToString();
        }
        public void FailCreateUser(IdentityResult identityResult)
        {
            ServiceError = ServiceError.BadRequest;
            Error = identityResult.ToString();
        }
        public void FailCredntials()
        {
            ServiceError = ServiceError.Unauthorized;
            Error = "Access denied, Invalid credentials";
        }
        public void FailManyAccessAttempts()
        {
            ServiceError = ServiceError.Forbid;
            Error = "Access denied, Too many attempts, account is locked out";
        }
        public void FailForbiden()
        {
            ServiceError = ServiceError.Forbid;
            Error = "You do not have permission to perform this operation.";
        }
    }

    public class ServiceResponse<T> : ServiceResponse
    {
        public T Data { get; set; }
    }

    public class ServiceResponse<T, E> : ServiceResponse
    {
        public T Data { get; set; }
        [JsonIgnore]
        public E Extra { get; set; }
    }
}