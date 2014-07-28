using System.Data.Entity;

namespace ATO_Kanban.Models
{
    public class DataContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, add the following
        // code to the Application_Start method in your Global.asax file.
        // Note: this will destroy and re-create your database with every model change.
        // 
        // System.Data.Entity.Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseIfModelChanges<ATO_Kanban.Models.TodoContext>());

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer(
                new MigrateDatabaseToLatestVersion<DataContext, Migrations.Configuration>());
        }

        public DataContext() : base("name=DataContext")
        {
        }

        public DbSet<Todo> Todoes { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Priority> Priorities { get; set; }
    }
}
