using System;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using AuthServer.Data;
using AuthServer.Helpers;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using AuthServer.Models;
using Microsoft.Extensions.Options;

namespace AuthServer.Services
{
  public class AuthService : IAuthService
  {
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppSettings _appSettings;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly DataContext _context;

    public AuthService(UserManager<IdentityUser> userManager, IOptions<AppSettings> appSettings, TokenValidationParameters tokenValidationParameters, DataContext context, RoleManager<IdentityRole> roleManager)
    {
      _userManager = userManager;
      _appSettings = appSettings.Value;
      _tokenValidationParameters = tokenValidationParameters;
      _context = context;
      _roleManager = roleManager;
    }

    public async Task<AuthResponse> RegisterAsync(string email, string password)
    {
      var existingUser = await _userManager.FindByEmailAsync(email);

      if (existingUser != null)
      {
        return new AuthResponse
        {
          Errors = new[] { "User with this email address already exists" }
        };
      }

      var newUserId = Guid.NewGuid();
      var newUser = new IdentityUser
      {
        Id = newUserId.ToString(),
        Email = email,
        UserName = email
      };

      var createdUser = await _userManager.CreateAsync(newUser, password);

      if (!createdUser.Succeeded)
      {
        return new AuthResponse
        {
          Errors = createdUser.Errors.Select(x => x.Description)
        };
      }

      return await GenerateAuthResponseForUserAsync(newUser);
    }

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
      var user = await _userManager.FindByEmailAsync(email);

      if (user == null)
      {
        return new AuthResponse
        {
          Errors = new[] { "User does not exist" }
        };
      }

      var userHasValidPassword = await _userManager.CheckPasswordAsync(user, password);

      if (!userHasValidPassword)
      {
        return new AuthResponse
        {
          Errors = new[] { "User/password combination is wrong" }
        };
      }

      return await GenerateAuthResponseForUserAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken)
    {
      var validatedToken = GetPrincipalFromToken(token);

      if (validatedToken == null)
      {
        return new AuthResponse { Errors = new[] { "Invalid Token" } };
      }

      var expiryDateUnix =
          long.Parse(validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

      var expiryDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
          .AddSeconds(expiryDateUnix);

      if (expiryDateTimeUtc > DateTime.UtcNow)
      {
        return new AuthResponse { Errors = new[] { "This token hasn't expired yet" } };
      }

      var jti = validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

      var storedRefreshToken = await _context.RefreshTokens.SingleOrDefaultAsync(x => x.Token == refreshToken);

      if (storedRefreshToken == null)
      {
        return new AuthResponse { Errors = new[] { "This refresh token does not exist" } };
      }

      if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
      {
        return new AuthResponse { Errors = new[] { "This refresh token has expired" } };
      }

      if (storedRefreshToken.Invalidated)
      {
        return new AuthResponse { Errors = new[] { "This refresh token has been invalidated" } };
      }

      if (storedRefreshToken.Used)
      {
        return new AuthResponse { Errors = new[] { "This refresh token has been used" } };
      }

      if (storedRefreshToken.JwtId != jti)
      {
        return new AuthResponse { Errors = new[] { "This refresh token does not match this JWT" } };
      }

      storedRefreshToken.Used = true;
      _context.RefreshTokens.Update(storedRefreshToken);
      await _context.SaveChangesAsync();

      var user = await _userManager.FindByIdAsync(validatedToken.Claims.Single(x => x.Type == "id").Value);
      return await GenerateAuthResponseForUserAsync(user);
    }

    private ClaimsPrincipal GetPrincipalFromToken(string token)
    {
      var tokenHandler = new JwtSecurityTokenHandler();

      try
      {
        var tokenValidationParameters = _tokenValidationParameters.Clone();
        tokenValidationParameters.ValidateLifetime = false;
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
        if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
        {
          return null;
        }

        return principal;
      }
      catch
      {
        return null;
      }
    }

    private bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
    {
      return (validatedToken is JwtSecurityToken jwtSecurityToken) &&
             jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                 StringComparison.InvariantCultureIgnoreCase);
    }

    private async Task<AuthResponse> GenerateAuthResponseForUserAsync(IdentityUser user)
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

      var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("id", user.Id)
            };

      var userClaims = await _userManager.GetClaimsAsync(user);
      claims.AddRange(userClaims);

      var userRoles = await _userManager.GetRolesAsync(user);
      foreach (var userRole in userRoles)
      {
        claims.Add(new Claim(ClaimTypes.Role, userRole));
        var role = await _roleManager.FindByNameAsync(userRole);
        if (role == null) continue;
        var roleClaims = await _roleManager.GetClaimsAsync(role);

        foreach (var roleClaim in roleClaims)
        {
          if (claims.Contains(roleClaim))
            continue;

          claims.Add(roleClaim);
        }
      }

      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(_appSettings.ExpiresInHours)),
        SigningCredentials =
              new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
      };

      var token = tokenHandler.CreateToken(tokenDescriptor);

      var refreshToken = new RefreshToken
      {
        JwtId = token.Id,
        UserId = user.Id,
        CreationDate = DateTime.UtcNow,
        ExpiryDate = DateTime.UtcNow.AddMonths(6)
      };

      await _context.RefreshTokens.AddAsync(refreshToken);
      await _context.SaveChangesAsync();

      return new AuthResponse
      {
        Success = true,
        Token = tokenHandler.WriteToken(token),
        RefreshToken = refreshToken.Token
      };
    }
  }
}