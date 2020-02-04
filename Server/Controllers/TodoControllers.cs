using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Server.Data;
using Server.Models;

namespace Server.Controllers
{
    [ApiController]
    [Route("/todo")]
    public class TodoControllers : ControllerBase
    {
        
        public IConfiguration Configuration;
        public AppDbContext AppDbContext {get;set;}
        public User user;

        public TodoControllers(IConfiguration configuration,AppDbContext appDbContext)
        {
            AppDbContext = appDbContext;
            Configuration = configuration;
        }

        public List<Todo> Todos()
        {
            string text = System.IO.File.ReadAllLines("Token.txt").Last();  
            var token = text;
            var jwtSec  = new JwtSecurityTokenHandler();
            var securityToken = jwtSec.ReadToken(token) as JwtSecurityToken;
            var sub = securityToken?.Claims.First(Claim => Claim.Type == "sub").Value;
            var todos = from x in AppDbContext.Todo where x.user_id == Convert.ToInt32(sub) select x;

            return todos.ToList();
        }
        public User GetUser()
        {
            string text = System.IO.File.ReadAllLines("Token.txt").Last();  
            var token = text;
            var jwtSec  = new JwtSecurityTokenHandler();
            var securityToken = jwtSec.ReadToken(token) as JwtSecurityToken;
            var sub = securityToken?.Claims.First(Claim => Claim.Type == "sub").Value;
            var user = from u in AppDbContext.User from x in u.Todo where u.id == Convert.ToInt32(sub) select u;

            return user.First(); 
        }
        
        [Authorize]
        [HttpGet("list")]
        public IActionResult Get()
        {
            var x = Todos();
            return Ok(x);
        }

        [Authorize]
        [HttpPost("add")]
        public IActionResult Register([FromBody]Todo input)
        {   
            string text = System.IO.File.ReadAllLines("Token.txt").Last();  
            var token = text;
            var jwtSec  = new JwtSecurityTokenHandler();
            var securityToken = jwtSec.ReadToken(token) as JwtSecurityToken;
            var sub = securityToken?.Claims.First(Claim => Claim.Type == "sub").Value;
            input.status = "Progress";
            input.user_id = Convert.ToInt32(sub);
            AppDbContext.Todo.Add(input);
            AppDbContext.SaveChanges();
            return Ok(AppDbContext.Todo.Include("User").ToList());
        }  

        [Authorize]
        [HttpGet("done/{id}")]
        public IActionResult Done(int id)
        {   
            var todo = AppDbContext.Todo.Find(id);
            todo.status = "Done";
            AppDbContext.SaveChanges();
            return Ok(Todos());
        } 

        [Authorize]
        [HttpPatch("update/{id}")]
        public IActionResult Update(int id,[FromBody]Todo input)
        {   
            var todo = AppDbContext.Todo.Find(id);
            todo.activity = input.activity;
            AppDbContext.SaveChanges();
            
            return Ok(Todos());
        } 

        [Authorize]
        [HttpGet("delete/{id}")]
        public IActionResult Delete(int id)
        {   
            var delete = new Todo(){id=id};
            var hapus = AppDbContext.Todo.Remove(delete);
            AppDbContext.SaveChanges();
            return Ok(Todos());
        }

        [Authorize]
        [HttpGet("clear")]
        public IActionResult Clear()
        {   
            var todo = Todos();
            foreach(var x in todo)
            {
                var clear = AppDbContext.Todo.Find(x.id);
                AppDbContext.Attach(clear);
                AppDbContext.Remove(clear); 
            }
            AppDbContext.SaveChanges(); 
            return Ok(Todos());
        }
    }
}