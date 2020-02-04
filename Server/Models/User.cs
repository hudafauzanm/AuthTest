using System.Collections.Generic;

namespace Server.Models
{
    public class User
    {   
        public int id{get;set;}
        public string username{get;set;}
        public string password{get;set;}
        public ICollection<Todo> Todo {get;set;}
    }
}