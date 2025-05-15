using Microsoft.AspNetCore.Identity; 
using System.Collections.Generic;

namespace backend.Models
{
    public class User : IdentityUser<int>
    {

        public List<string> Roles { get; set; } = new();

        public List<Subscription> Subscriptions { get; set; } = new(); 

    }
}
