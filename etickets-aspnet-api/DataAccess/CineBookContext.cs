namespace etickets_aspnet_api.DataAccess
{
    public partial class CineBookContext : IdentityDbContext<ApplicationUser>
    {
        #region Constructor

        public CineBookContext(DbContextOptions<CineBookContext> options)
            : base(options)
        {
        }
        #endregion

        #region DbSets

        public virtual DbSet<Actor> Actors { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Cinema> Cinemas { get; set; }
        public virtual DbSet<Movie> Movies { get; set; }
        public virtual DbSet<UserOTP> UserOTPs { get; set; }
        public virtual DbSet<Cart> Carts { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderItems> OrderItems { get; set; }



        #endregion

        #region Model Configuration

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureActor(modelBuilder);
            ConfigureCategory(modelBuilder);
            ConfigureCinema(modelBuilder);
            ConfigureMovie(modelBuilder);

            OnModelCreatingPartial(modelBuilder);
        }

        private static void ConfigureActor(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Actor>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.ProfilePicture).HasMaxLength(255);

                entity.HasMany(d => d.Movies)
                      .WithMany(p => p.Actors)
                      .UsingEntity<Dictionary<string, object>>(
                          "ActorMovie",
                          r => r.HasOne<Movie>()
                                .WithMany()
                                .HasForeignKey("MoviesId")
                                .HasConstraintName("FK_ActorMovies_Movies"),
                          l => l.HasOne<Actor>()
                                .WithMany()
                                .HasForeignKey("ActorsId")
                                .HasConstraintName("FK_ActorMovies_Actors"),
                          j =>
                          {
                              j.HasKey("ActorsId", "MoviesId");
                              j.ToTable("ActorMovies");
                          });
            });
        }

        private static void ConfigureCategory(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100);
            });
        }

        private static void ConfigureCinema(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cinema>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(255);
                entity.Property(e => e.CinemaLogo).HasMaxLength(255);
            });
        }

        private static void ConfigureMovie(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Movie>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.ImgUrl).HasMaxLength(255);
                entity.Property(e => e.TrailerUrl).HasMaxLength(500);
                entity.Property(e => e.ImgUrl).HasDefaultValue("default.jpg").ValueGeneratedOnAdd();

                entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.StartDate).HasColumnType("datetime");
                entity.Property(e => e.EndDate).HasColumnType("datetime");
            });
        }

        #endregion

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
