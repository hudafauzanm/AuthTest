using System;
using Todos;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using McMaster.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.Linq;
using Users;
using Tokens;

namespace CLI
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var client = new HttpClient();
            string text = System.IO.File.ReadAllLines("token.txt").Last();
            var token = text; 
            Token Token =  new Token();
            var root = new CommandLineApplication()
            {
                Name = "#9 Get screenshots from a list of file",
                Description = "Get screenshots from a list of file",
                ShortVersionGetter = () => "1.0.0",
            };

            root.Command("register",app =>
            {
                app.Description = "Registrasi";

                var text = app.Argument("Text","Masukkan Text",true);
                app.OnExecuteAsync(async cancellationToken => 
                {
                    Prompt.GetYesNo("Yakin kah?",false);
                    var add = new User()
                    {
                        username = text.Values[0],
                        password = text.Values[1]
                    };
                    var data = JsonSerializer.Serialize(add);
                    var hasil = new StringContent(data,Encoding.UTF8,"application/json");
                    var response = await client.PostAsync("https://localhost:5001/user/register",hasil);
                   
                });
            });
            
            root.Command("login",app =>
            {
                app.Description = "Login";

                var text = app.Argument("Text","Masukkan Text",true);
                app.OnExecuteAsync(async cancellationToken => 
                {
                    Prompt.GetYesNo("Yakin kah?",false);
                    var add = new
                    {
                        username = text.Values[0],
                        password = text.Values[1]
                    };
                    var data = JsonSerializer.Serialize(add);
                    var hasil = new StringContent(data,Encoding.UTF8,"application/json");
                    var response = await client.PostAsync("https://localhost:5001/user/login",hasil);
                    var json = await response.Content.ReadAsStringAsync();
                    Token = JsonSerializer.Deserialize<Token>(json);
                    Token.SaveToken();
                });
            });
            root.Command("list",app => 
            {
                app.Description = "Get screenshots from a list of file";
                
                app.OnExecuteAsync(async cancellationToken => 
                {   
                    if(token!="")
                    {
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,"https://localhost:5001/todo/list");
                        HttpResponseMessage response = await client.SendAsync(request);
                        var json = await response.Content.ReadAsStringAsync();
                        
                        var list = JsonSerializer.Deserialize<List<Todo>>(json);
                        Console.WriteLine("TO DO LIST OF MINE");
                        foreach(var x in list)
                        {
                            Console.WriteLine(x.id+"."+" | "+x.activity+" | "+ x.status);
                        }
                    }
                });
            });

            root.Command("add",app => 
            {
                app.Description = "Get screenshots from a list of file";

                var text = app.Argument("Text","Masukkan Text");
                
                app.OnExecuteAsync(async cancellationToken => 
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
                    var add = new Todo()
                    {
                        activity = text.Value,
                    };
                    var data = JsonSerializer.Serialize(add);
                    var hasil = new StringContent(data,Encoding.UTF8,"application/json");
                    var response = await client.PostAsync("https://localhost:5001/todo/add",hasil);
                });
            });

            root.Command("clear",app => 
            {
                app.Description = "Get screenshots from a list of file";
                
                
                app.OnExecuteAsync(async cancellationToken => 
                {
                    Prompt.GetYesNo("Yakin kah?",false);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,"https://localhost:5001/todo/clear");
                    HttpResponseMessage response = await client.SendAsync(request);
                });
            });

            root.Command("update",app => 
            {
                app.Description = "Get screenshots from a list of file";

                var text = app.Argument("Text","Masukkan Text",true);
                app.OnExecuteAsync(async cancellationToken => 
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
                    var add = "{" + "\"activity\":" + $"\"{text.Values[1]}\"" + "}";
                    var hasil = new StringContent(add,Encoding.UTF8,"application/json");
                    var responses = await client.PatchAsync($"https://localhost:5001/todo/update/{text.Values[0]}",hasil);
                });
            });

            root.Command("delete",app => 
            {
                app.Description = "Get screenshots from a list of file";
                var text = app.Argument("Text","Masukkan Text");
                app.OnExecuteAsync(async cancellationToken => 
                {   
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,$"https://localhost:5001/todo/delete/{text.Value}");
                    HttpResponseMessage response = await client.SendAsync(request);
                });
            });

            root.Command("done",app => 
            {
                app.Description = "Get screenshots from a list of file";

                var text = app.Argument("Text","Masukkan Text");
                app.OnExecuteAsync(async cancellationToken => 
                {   
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,$"https://localhost:5001/todo/done/{text.Value}");
                    HttpResponseMessage response = await client.SendAsync(request);
                });
            });

            return root.Execute(args);
    }
    }
}
