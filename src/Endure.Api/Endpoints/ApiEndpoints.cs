namespace Endure.Api.Endpoints;

internal static class ApiEndpoints
{
    private const string ApiBase = "api";

    internal static class Users
    {
        public const string Tag = "Users";
        private const string UserBase = $"{ApiBase}/users";
        public const string Register = $"{UserBase}/register";
        public const string Login = $"{UserBase}/login";
        public const string GetMyUser = $"{UserBase}/me";
    }
}
