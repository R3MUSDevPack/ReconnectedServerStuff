namespace r3mus.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class LiveWardecs : DbContext
    {
        public LiveWardecs()
            : base("name=LiveWardecs")
        {
        }

        public virtual DbSet<Wardec> Wardecs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
