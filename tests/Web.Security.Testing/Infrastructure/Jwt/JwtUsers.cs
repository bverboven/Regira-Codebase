namespace Web.Security.Testing.Infrastructure.Jwt;

public static class JwtUsers
{
    public static IList<JwtUser> Value => new List<JwtUser>
    {
        new JwtUser {UserId = "TEST_USER_ID", Name = "TestUser"}
    };
}