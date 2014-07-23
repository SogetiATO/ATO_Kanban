using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATO_Kanban.Models
{
    public class Todo
    {
        public int ID { get; set; }
        public int AssigneeID { get; set; }
        public virtual User Assignee { get; set; }
        public int? ClaimedByID { get; set; }
        public virtual User ClaimedBy { get; set; }
        public int PriorityID { get; set; }
        public virtual Priority Priority { get; set; }
        public int StatusID { get; set; }
        public virtual Status Status { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public bool EmailATO { get; set; }
        public bool RequiresApproval { get; set; }
        public bool Optional { get; set; }
        public string ReasonForRevision { get; set; }
        public bool IsPublic { get; set; }
    }
}