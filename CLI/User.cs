using System;
using System.Collections.Generic;
using Todos;

namespace Users
{
    public class User
    {
        public int id{get;set;}
        public string username{get;set;}
        public string password{get;set;}
        public ICollection<Todo> Todos{get;set;}
    }
}