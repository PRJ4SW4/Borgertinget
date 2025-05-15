using Microsoft.AspNetCore.Identity;

namespace backend.utils;

public class CostumErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError PasswordTooShort(int length)
    {
        return new IdentityError
        {
            Code = "PasswordTooShort",
            Description = $"Kodeordet skal være mindst {length} tegn langt.",
        };
    }

    public override IdentityError PasswordRequiresDigit()
    {
        return new IdentityError
        {
            Code = "PasswordRequiresDigit",
            Description = "Kodeordet skal indeholde mindst ét tal.",
        };
    }

    public override IdentityError PasswordRequiresLower()
    {
        return new IdentityError
        {
            Code = "PasswordRequiresLower",
            Description = "Kodeordet skal indeholde mindst ét lille bogstav.",
        };
    }

    public override IdentityError PasswordRequiresUpper()
    {
        return new IdentityError
        {
            Code = "PasswordRequiresUpper",
            Description = "Kodeordet skal indeholde mindst ét stort bogstav.",
        };
    }

    public override IdentityError DuplicateEmail(string email)
    {
        return new IdentityError
        {
            Code = "DuplicateEmail",
            Description = $"Emailadressen '{email}' er allerede i brug.",
        };
    }

    public override IdentityError DuplicateUserName(string userName)
    {
        return new IdentityError
        {
            Code = "DuplicateUserName",
            Description = $"Brugernavnet '{userName}' er allerede i brug.",
        };
    }
}
