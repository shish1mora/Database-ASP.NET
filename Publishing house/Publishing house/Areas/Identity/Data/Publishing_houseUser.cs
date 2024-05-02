using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Publishing_house.Areas.Identity.Data;

// Add profile data for application users by adding properties to the Publishing_houseUser class
public class Publishing_houseUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

}

