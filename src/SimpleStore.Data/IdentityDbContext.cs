using Microsoft.EntityFrameworkCore;
using SimpleStore.Data.Identity;

namespace SimpleStore.Data;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<ApplicationUser>(options);
