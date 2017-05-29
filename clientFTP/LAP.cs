namespace clientFTP
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class LAP : DbContext
    {
        public LAP()
            : base("name=LAP")
        {
        }

        public virtual DbSet<LogAndPass> LogAndPass { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
