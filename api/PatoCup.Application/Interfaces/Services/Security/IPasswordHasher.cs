namespace PatoCup.Application.Interfaces.Services.Security
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string passwordHash, string inputPassword);
    }
}