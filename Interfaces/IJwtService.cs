using RestAPISystemKnowledge.Models.Entities;

namespace RestAPISystemKnowledge.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user, string roleName);
    }
}
