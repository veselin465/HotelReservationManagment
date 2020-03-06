using Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class HotelReservationDb : DbContext
    {
        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<Reservation> Reservations { get; set; }
        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<ClientReservation> ClientReservation { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.\SQLEXPRESS;Database=HotelReservationDb;Trusted_Connection=True;MultipleActiveResultSets=true");
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<ClientReservation>()
                .HasKey(cr => new { cr.ClientId, cr.ReservationId });
        }

    }
}
