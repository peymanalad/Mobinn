using Chamran.Deed.OpenIddict.Applications;
using Chamran.Deed.OpenIddict.Authorizations;
using Chamran.Deed.OpenIddict.Scopes;
using Chamran.Deed.OpenIddict.Tokens;
using Microsoft.EntityFrameworkCore;

namespace Chamran.Deed.EntityFrameworkCore
{
    public interface IOpenIddictDbContext
    {
        DbSet<OpenIddictApplication> Applications { get; }

        DbSet<OpenIddictAuthorization> Authorizations { get; }

        DbSet<OpenIddictScope> Scopes { get; }

        DbSet<OpenIddictToken> Tokens { get; }
    }
}

