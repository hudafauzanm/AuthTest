using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public class Todo
    {
        public int id{get;set;}
        public string activity{get;set;}
        public string status{get;set;}

        [ForeignKey("User")]
        public int user_id{get;set;}
        public User User {get;set;}
    }
}