using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HrmSystem.API.Data;
using HrmSystem.API.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HrmSystem.API.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // Hash password bằng SHA256
    public static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var passwordHash = HashPassword(request.Password);

        var account = await _context.Accounts
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a =>
                a.Username == request.Username &&
                a.PasswordHash == passwordHash);

        if (account == null) return null;

        var token = GenerateToken(account);

        return new LoginResponse
        {
            Token = token,
            FullName = account.Employee.FullName,
            Role = account.Role
        };
    }

    private string GenerateToken(Models.Account account)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new Claim(ClaimTypes.Name, account.Username),
            new Claim(ClaimTypes.Role, account.Role),
            new Claim("EmployeeId", account.EmployeeId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(
                double.Parse(_config["JwtSettings:ExpiryMinutes"]!)),
            signingCredentials: new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}