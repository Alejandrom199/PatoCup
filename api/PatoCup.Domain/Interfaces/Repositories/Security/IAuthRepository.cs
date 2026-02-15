using System.Threading.Tasks;
using PatoCup.Domain.Entities.Security;

namespace PatoCup.Domain.Interfaces.Repositories.Security
{
    public interface IAuthRepository
    {
        /// <summary>
        /// Ejecuta el SP de login para validar credenciales y obtener datos del usuario
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <returns>Retorna el Usuario si es válido, o null si falla</returns>
        Task<User?> LoginAsync(string username);

        Task<bool> ChangePassword(int id, string newPassword);
    }
}