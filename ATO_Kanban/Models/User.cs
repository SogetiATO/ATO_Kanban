using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATO_Kanban.Models
{
    public class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Grade { get; set; }
    }
}