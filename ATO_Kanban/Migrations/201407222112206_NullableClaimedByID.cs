namespace ATO_Kanban.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NullableClaimedByID : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Todoes", "ClaimedByID", "dbo.Users");
            DropIndex("dbo.Todoes", new[] { "ClaimedByID" });
            AlterColumn("dbo.Todoes", "ClaimedByID", c => c.Int());
            AddForeignKey("dbo.Todoes", "ClaimedByID", "dbo.Users", "ID");
            CreateIndex("dbo.Todoes", "ClaimedByID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Todoes", new[] { "ClaimedByID" });
            DropForeignKey("dbo.Todoes", "ClaimedByID", "dbo.Users");
            AlterColumn("dbo.Todoes", "ClaimedByID", c => c.Int(nullable: false));
            CreateIndex("dbo.Todoes", "ClaimedByID");
            AddForeignKey("dbo.Todoes", "ClaimedByID", "dbo.Users", "ID", cascadeDelete: true);
        }
    }
}
