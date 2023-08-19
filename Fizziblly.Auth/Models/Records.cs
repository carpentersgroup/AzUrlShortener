using System.Security.Claims;

namespace Fizzibly.Auth.Models
{
    public record JwtAuthRequest(Microsoft.AspNetCore.Http.HttpRequest HttpRequestMessage, ClaimsPrincipal? Principal, string? Token);
    public record AppOnlyAuthRequest(Microsoft.AspNetCore.Http.HttpRequest HttpRequestMessage, ClaimsPrincipal? Principal, string? Token) : JwtAuthRequest(HttpRequestMessage, Principal, Token);
    public record UserAuthRequest(Microsoft.AspNetCore.Http.HttpRequest HttpRequestMessage, ClaimsPrincipal? Principal, string? Token) : JwtAuthRequest(HttpRequestMessage, Principal, Token);
}