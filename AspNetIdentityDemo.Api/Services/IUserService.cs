using AspNetIdentityDemo.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AspNetIdentityDemo.Api.Services
{
    public interface IUserService
    {

        Task<UserManagerResponse> RegisterUserAsync(RegisterViewModel model);

        Task<UserManagerResponse> LoginUserAsync(LoginViewModel model);
    }

    public class UserService : IUserService
    {

        private UserManager<IdentityUser> _userManger;
        private IConfiguration _configuration; 
        public UserService(UserManager<IdentityUser> userManager, IConfiguration configuration) 
        {
            _userManger = userManager;
            _configuration = configuration;
        }

        public async Task<UserManagerResponse> RegisterUserAsync(RegisterViewModel model)
        {
            if (model == null)
                throw new NullReferenceException("Reigster Model is null");

            if (model.Password != model.ConfirmPassword)
                return new UserManagerResponse
                {
                    Message = "Confirm password doesn't match the password",
                    IsSuccess = false,
                };

            var identityUser = new IdentityUser
            {
                Email = model.Email,
                UserName = model.Email,
            };

            var result = await _userManger.CreateAsync(identityUser, model.Password);

            if(result.Succeeded)
            {
                // TODO: Send a confirmation Email 
                return new UserManagerResponse
                {
                    Message = "User created successfully!",
                    IsSuccess = true,
                };
            }

            return new UserManagerResponse
            {
                Message = "User did not create",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description)
            };

        }

        public async Task<UserManagerResponse> LoginUserAsync(LoginViewModel model)
        {
            var user = await _userManger.FindByEmailAsync(model.Email);

            if(user == null)
            {
                return new UserManagerResponse
                {
                    Message = "There is no user with that Email address",
                    IsSuccess = false,
                };
            }

            var result = await _userManger.CheckPasswordAsync(user, model.Password);

            if(!result)
                return new UserManagerResponse
                {
                    Message = "Invalid password",
                    IsSuccess = false,
                };

            var claims = new[]
            {
                new Claim("Email", model.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthSettings:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["AuthSettings:Issuer"],
                audience: _configuration["AuthSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            string tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);

            return new UserManagerResponse
            {
                Message = tokenAsString,
                IsSuccess = true,
                ExpireDate = token.ValidTo
            };
        }

    }
}
