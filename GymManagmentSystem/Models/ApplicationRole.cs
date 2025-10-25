using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace GymManagmentSystem.Models
{
    [CollectionName("Roles")]
    public class ApplicationRole : MongoIdentityRole<string>
    {
        public ApplicationRole() : base()
        {
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
        }
    }
}

