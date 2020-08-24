using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using core_api.Extentions;
using core_api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace core_api.Data
{
    public interface IAuthService
    {
        Task<ServiceResponse<UserProfileDto>> Login(string email, string password);
        Task<ServiceResponse> ConfirmEmail(string token, string email);

        Task<ServiceResponse<object, string>> ForgetPassword(string email);
        Task<ServiceResponse> ChangePassword(string email, string currentPassword, string newPassword);

        Task<ServiceResponse<UserDto, string>> Invite(InviteUserRequest request);
        Task<ServiceResponse<UserProfileDto, string>> Register(RegisterRequest request);

        Task<ServiceResponse<UserProfileDto>> ConfirmInvitation(string token, string email, string newPassword);
        Task<ServiceResponse> ResetPassword(string token, string email, string newPassword);
        Task<ServiceResponse<UserProfileDto>> FacebookSignIn(string token);
    }

    public class AuthService : IAuthService
    {
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthService(AppSettings appSettings,
            IMapper mapper, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _appSettings = appSettings;
            _mapper = mapper;

            _userManager = userManager;
            _signInManager = signInManager;

        }

        public async Task<ServiceResponse> ChangePassword(string email, string currentPassword, string newPassword)
        {
            var response = new ServiceResponse();

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                if (await _userManager.IsLockedOutAsync(user))
                {
                    response.FailManyAccessAttempts();
                    return response;
                }

                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
                if (result.Succeeded)
                {
                    return response;
                }
            }
            response.FailCredntials();
            return response;
        }

        public async Task<ServiceResponse> ConfirmEmail(string token, string email)
        {
            var response = new ServiceResponse();

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (!result.Succeeded)
                {
                    response.Fail(result);
                    return response;
                }
            }
            else
            {
                response.FailCredntials();
            }
            return response;
        }

        public async Task<ServiceResponse<object, string>> ForgetPassword(string email)
        {
            var response = new ServiceResponse<object, string>();

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                response.Extra = await _userManager.GeneratePasswordResetTokenAsync(user);
            }
            return response;
        }

        public async Task<ServiceResponse<UserProfileDto>> Login(string email, string password)
        {
            var response = new ServiceResponse<UserProfileDto>();

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                if (await _userManager.IsLockedOutAsync(user))
                {
                    response.FailManyAccessAttempts();
                    return response;
                }

                var signInResult = await _signInManager.CheckPasswordSignInAsync(user, password, true);

                if (signInResult.Succeeded)
                {
                    var userDto = _mapper.Map<UserProfileDto>(user);
                    userDto.Token = user.CreateToken(_appSettings.SecurityConfig.JwtConfig);
                    response.Data = userDto;
                    return response;
                }

            }

            response.FailCredntials();
            return response;
        }

        public async Task<ServiceResponse<UserProfileDto, string>> Register(RegisterRequest request)
        {
            var response = new ServiceResponse<UserProfileDto, string>();

            var user = _mapper.Map<User>(request);
            // role configuration
            user.Role = Role.None;

            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                var userDto = _mapper.Map<UserProfileDto>(user);
                userDto.Token = user.CreateToken(_appSettings.SecurityConfig.JwtConfig);
                response.Extra = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                response.Data = userDto;
            }
            else
            {
                response.FailCreateUser(result);
            }

            return response;
        }

        public async Task<ServiceResponse<UserDto, string>> Invite(InviteUserRequest request)
        {
            var response = new ServiceResponse<UserDto, string>();

            // role configuration
            if (request.Role == Role.Admin)
            {
                response.FailForbiden();
                return response;
            }

            var user = _mapper.Map<User>(request);

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                var userDto = _mapper.Map<UserDto>(user);
                response.Extra = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                response.Data = userDto;
            }
            else
            {
                response.FailCreateUser(result);
            }

            return response;
        }

        public async Task<ServiceResponse<UserProfileDto>> ConfirmInvitation(string token, string email, string newPassword)
        {
            var response = new ServiceResponse<UserProfileDto>();

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    await _userManager.AddPasswordAsync(user, newPassword);

                    var userDto = _mapper.Map<UserProfileDto>(user);
                    userDto.Token = user.CreateToken(_appSettings.SecurityConfig.JwtConfig);
                    response.Data = userDto;

                    return response;
                }
                else
                {
                    response.Fail(result);
                    return response;
                }
            }
            response.FailCredntials();

            return response;
        }

        public async Task<ServiceResponse> ResetPassword(string token, string email, string newPassword)
        {
            var response = new ServiceResponse();

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (result.Succeeded)
                {
                    return response;
                }
                else
                {
                    response.Fail(result);
                    return response;
                }
            }
            response.FailCredntials();

            return response;
        }

        public async Task<ServiceResponse<UserProfileDto>> FacebookSignIn(string token)
        {
            var response = new ServiceResponse<UserProfileDto>();

            var Client = new HttpClient();

            // generate app access token
            var appAccessTokenResponse =
                await Client.GetStringAsync($"https://graph.facebook.com/oauth/access_token?" +
                $"client_id={_appSettings.SecurityConfig.Facebook.Id}&" +
                $"client_secret={_appSettings.SecurityConfig.Facebook.Secret}" +
                $"&grant_type=client_credentials");

            var appAccessToken = JsonConvert.DeserializeObject<FacebookAppAccessToken>(appAccessTokenResponse);
            // validate user access token
            var userAccessTokenValidationResponse =
                await Client.GetStringAsync($"https://graph.facebook.com/debug_token?" +
                $"input_token={token}&" +
                $"access_token={appAccessToken.AccessToken}");
            var userAccessTokenValidation = JsonConvert.DeserializeObject<FacebookUserAccessTokenValidation>(userAccessTokenValidationResponse);

            if (!userAccessTokenValidation.Data.IsValid)
            {
                response.FailOperation("Invalid facebook token.");
                return response;
            }

            // request user data from fb
            var userInfoResponse = await Client.GetStringAsync($"https://graph.facebook.com/v2.8/me?" +
                $"fields=id,email,name,picture&" +
                $"access_token={token}");

            if (String.IsNullOrWhiteSpace(userInfoResponse))
            {
                response.FailOperation("Invalid user profile.");
                return response;
            }

            var userInfo = JsonConvert.DeserializeObject<FacebookUserData>(userInfoResponse);

            var user = await _userManager.FindByEmailAsync(userInfo.Email);

            if (user == null)
            {
                user = new User
                {
                    FullName = userInfo.Name,
                    Email = userInfo.Email,
                    EmailConfirmed = true,
                    UserName = userInfo.Email,
                    PhotoUrl = userInfo.Picture.Data.Url,
                    // role configuration
                    Role = Role.None
                };

                var result = await _userManager.CreateAsync(user, token);

                if (!result.Succeeded)
                {
                    response.FailOperation();
                    return response;
                }
                // signin
                await _signInManager.PasswordSignInAsync(user.Email, token, false, false);

                var userDto = _mapper.Map<UserProfileDto>(user);
                userDto.Token = user.CreateToken(_appSettings.SecurityConfig.JwtConfig);
                response.Data = userDto;

                return response;

            }
            else
            {
                var signInResult = await _signInManager.CheckPasswordSignInAsync(user, token, true);

                if (signInResult.Succeeded)
                {
                    var userDto = _mapper.Map<UserProfileDto>(user);
                    userDto.Token = user.CreateToken(_appSettings.SecurityConfig.JwtConfig);
                    response.Data = userDto;
                    return response;
                }
            }

            response.FailOperation("Email exists please login or reset password.");
            return response;


        }
    }
}