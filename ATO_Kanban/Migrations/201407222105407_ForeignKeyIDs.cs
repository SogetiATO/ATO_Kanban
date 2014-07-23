namespace ATO_Kanban.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ForeignKeyIDs : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Todoes", "Assignee_ID", "dbo.Users");
            DropForeignKey("dbo.Todoes", "ClaimedBy_ID", "dbo.Users");
            DropForeignKey("dbo.Todoes", "Priority_ID", "dbo.Priorities");
            DropIndex("dbo.Todoes", new[] { "Assignee_ID" });
            DropIndex("dbo.Todoes", new[] { "ClaimedBy_ID" });
            DropIndex("dbo.Todoes", new[] { "Priority_ID" });
            RenameColumn(table: "dbo.Todoes", name: "Assignee_ID", newName: "AssigneeID");
            RenameColumn(table: "dbo.Todoes", name: "ClaimedBy_ID", newName: "ClaimedByID");
            RenameColumn(table: "dbo.Todoes", name: "Priority_ID", newName: "PriorityID");
            AddForeignKey("dbo.Todoes", "AssigneeID", "dbo.Users", "ID", cascadeDelete: false);
            AddForeignKey("dbo.Todoes", "ClaimedByID", "dbo.Users", "ID", cascadeDelete: false);
            AddForeignKey("dbo.Todoes", "PriorityID", "dbo.Priorities", "ID", cascadeDelete: false);
            CreateIndex("dbo.Todoes", "AssigneeID");
            CreateIndex("dbo.Todoes", "ClaimedByID");
            CreateIndex("dbo.Todoes", "PriorityID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Todoes", new[] { "PriorityID" });
            DropIndex("dbo.Todoes", new[] { "ClaimedByID" });
            DropIndex("dbo.Todoes", new[] { "AssigneeID" });
            DropForeignKey("dbo.Todoes", "PriorityID", "dbo.Priorities");
            DropForeignKey("dbo.Todoes", "ClaimedByID", "dbo.Users");
            DropForeignKey("dbo.Todoes", "AssigneeID", "dbo.Users");
            RenameColumn(table: "dbo.Todoes", name: "PriorityID", newName: "Priority_ID");
            RenameColumn(table: "dbo.Todoes", name: "ClaimedByID", newName: "ClaimedBy_ID");
            RenameColumn(table: "dbo.Todoes", name: "AssigneeID", newName: "Assignee_ID");
            CreateIndex("dbo.Todoes", "Priority_ID");
            CreateIndex("dbo.Todoes", "ClaimedBy_ID");
            CreateIndex("dbo.Todoes", "Assignee_ID");
            AddForeignKey("dbo.Todoes", "Priority_ID", "dbo.Priorities", "ID");
            AddForeignKey("dbo.Todoes", "ClaimedBy_ID", "dbo.Users", "ID");
            AddForeignKey("dbo.Todoes", "Assignee_ID", "dbo.Users", "ID");
        }
    }
}
