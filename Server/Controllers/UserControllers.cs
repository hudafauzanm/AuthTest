using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Server.Data;
using Server.Models;

namespace Server.Controllers
{
    [ApiController]
    [Route("/user")]
    public class UserControllers : ControllerBase
    {
        
        public IConfiguration Configuration;
        public AppDbContext AppDbContext {get;set;}

        public UserControllers(IConfiguration configuration,AppDbContext appDbContext)
        {
            AppDbContext = appDbContext;
            Configuration = configuration;
        }

        [HttpGet("profile")]
        public IActionResult Get()
        {
            var users = AppDbContext.User.ToList();
            return Ok(users);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody]User input)
        {   
            var x = from l in AppDbContext.User select l.username.ToString();
            if(x.Contains(input.username))
            {
                 return Ok("Nama Sudah");
            }
            AppDbContext.User.Add(input);
            AppDbContext.SaveChanges();
            return Ok();
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody]User login)
        {
            IActionResult response = Unauthorized();

            var user = AuthenticatedUser(login);
            if(user != null)
            {
                var token = GenerateJwtToken(user);
                TextWriter tw = new StreamWriter("Token.txt", true);
                tw.WriteLine(token);
                tw.Close(); 
                return Ok(new {token = token});
            }
            return Ok();
        }
        
        [Authorize]
        [HttpGet("welcome")]
        public IActionResult Welcome()
        {
            return Ok("Dapat");
        }

        private string GenerateJwtToken(User user)
        {
            var secuityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(secuityKey,SecurityAlgorithms.HmacSha256);
            
            var claims = new[]
            {
                // new Claim(JwtRegisteredClaimNames.Sub,user.username),
                new Claim(JwtRegisteredClaimNames.Sub,Convert.ToString(user.id)),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer:Configuration["Jwt:Issuer"],
                audience:Configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials:credentials);

            var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);
            
            return encodedToken;
        }

        private User AuthenticatedUser(User login)
        {
            User user = null;
            var x = from data in AppDbContext.User select new {data.username,data.password,data.id};
            foreach(var i in x)
            {
                if(i.username == (login.username) && (i.password == (login.password)))
                {
                    user = new User
                    {
                        id = i.id,
                        username = login.username,
                        password = login.password,
                    };
                }
            }
            return user;
        }

    }
}