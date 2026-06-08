using Microsoft.EntityFrameworkCore;
using TaktTusur.Equipment.Domain.EquipmentRequest;

namespace TaktTusur.Equipment.DataAccess;

public class EquipmentDbContext : DbContext
{
    public EquipmentDbContext(DbContextOptions<EquipmentDbContext> options) : base(options)
    {
    }

    public DbSet<EquipmentRequest> EquipmentRequests { get; set; } = null!;
}
