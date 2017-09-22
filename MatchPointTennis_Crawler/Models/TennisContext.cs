namespace MatchPointTennis_Crawler.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class TennisContext : DbContext
    {
        public TennisContext()
            : base("name=TennisContext")
        {
        }

        public virtual DbSet<tblMatch> tblMatches { get; set; }
        public virtual DbSet<tblTeamMatch> tblTeamMatches { get; set; }
        public virtual DbSet<tklArea> tklAreas { get; set; }
        public virtual DbSet<tklDistrict> tklDistricts { get; set; }
        public virtual DbSet<tklFlight> tklFlights { get; set; }
        public virtual DbSet<tklLeague> tklLeagues { get; set; }
        public virtual DbSet<tklSection> tklSections { get; set; }
        public virtual DbSet<tklSubFlight> tklSubFlights { get; set; }
        public virtual DbSet<tklTeam> tklTeams { get; set; }
        public virtual DbSet<tklUserList> tklUserLists { get; set; }
        public virtual DbSet<tklUserTeam> tklUserTeams { get; set; }
        public virtual DbSet<tklYear> tklYears { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<tblMatch>()
                .Property(e => e.MatchType)
                .IsUnicode(false);

            modelBuilder.Entity<tblTeamMatch>()
                .HasMany(e => e.tblMatches)
                .WithRequired(e => e.tblTeamMatch)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tklArea>()
                .Property(e => e.Area)
                .IsUnicode(false);

            modelBuilder.Entity<tklArea>()
                .HasMany(e => e.tklLeagues)
                .WithRequired(e => e.tklArea)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tklDistrict>()
                .Property(e => e.District)
                .IsUnicode(false);

            modelBuilder.Entity<tklDistrict>()
                .HasMany(e => e.tklAreas)
                .WithRequired(e => e.tklDistrict)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tklFlight>()
                .Property(e => e.FlightGender)
                .IsUnicode(false);

            modelBuilder.Entity<tklFlight>()
                .HasMany(e => e.tklSubFlights)
                .WithRequired(e => e.tklFlight)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tklLeague>()
                .Property(e => e.LeagueName)
                .IsUnicode(false);

            modelBuilder.Entity<tklLeague>()
                .HasMany(e => e.tklFlights)
                .WithRequired(e => e.tklLeague)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tklSection>()
                .Property(e => e.Section)
                .IsUnicode(false);

            modelBuilder.Entity<tklSection>()
                .HasMany(e => e.tklDistricts)
                .WithRequired(e => e.tklSection)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tklSubFlight>()
                .Property(e => e.SubFlight)
                .IsUnicode(false);

            modelBuilder.Entity<tklTeam>()
                .Property(e => e.TeamName)
                .IsUnicode(false);

            modelBuilder.Entity<tklTeam>()
                .Property(e => e.TeamFacility)
                .IsUnicode(false);

            modelBuilder.Entity<tklTeam>()
                .HasMany(e => e.tblMatches)
                .WithOptional(e => e.tklTeam)
                .HasForeignKey(e => e.WinningTeamID);

            modelBuilder.Entity<tklTeam>()
                .HasMany(e => e.tblTeamMatches)
                .WithRequired(e => e.tklTeam)
                .HasForeignKey(e => e.HomeTeamID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tklTeam>()
                .HasMany(e => e.tblTeamMatches1)
                .WithRequired(e => e.tklTeam1)
                .HasForeignKey(e => e.VisitingTeamID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tklUserList>()
                .Property(e => e.FullName)
                .IsUnicode(false);

            modelBuilder.Entity<tklUserList>()
                .Property(e => e.City)
                .IsUnicode(false);

            modelBuilder.Entity<tklUserList>()
                .Property(e => e.State)
                .IsUnicode(false);

            modelBuilder.Entity<tklUserList>()
                .HasMany(e => e.tblMatches)
                .WithOptional(e => e.tklUserList)
                .HasForeignKey(e => e.Home_PlayerID_1);

            modelBuilder.Entity<tklUserList>()
                .HasMany(e => e.tblMatches1)
                .WithOptional(e => e.tklUserList1)
                .HasForeignKey(e => e.Home_PlayerID_2);

            modelBuilder.Entity<tklUserList>()
                .HasMany(e => e.tblMatches2)
                .WithOptional(e => e.tklUserList2)
                .HasForeignKey(e => e.Visiting_PlayerID_1);

            modelBuilder.Entity<tklUserList>()
                .HasMany(e => e.tblMatches3)
                .WithOptional(e => e.tklUserList3)
                .HasForeignKey(e => e.Visiting_PlayerID_2);

            modelBuilder.Entity<tklYear>()
                .HasMany(e => e.tklLeagues)
                .WithRequired(e => e.tklYear)
                .HasForeignKey(e => e.LeagueYear)
                .WillCascadeOnDelete(false);
        }
    }
}
