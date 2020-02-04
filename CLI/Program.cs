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

            Token Token = new Token();
            
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
                    Console.WriteLine(Token.token);

                });
            });
            root.Command("list",app => 
            {
                app.Description = "Get screenshots from a list of file";
                
                app.OnExecuteAsync(async cancellationToken => 
                {   
                    if(Token.token!="")
                    {
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",Token.token);
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
                    var add = new Todo()
                    {
                        activity = text.Value,
                    };
                    var data = JsonSerializer.Serialize(add);
                    var hasil = new StringContent(data,Encoding.UTF8,"application/json");
                    var response = await client.PostAsync("https://localhost:5001/todos",hasil);
                });
            });

            root.Command("clear",app => 
            {
                app.Description = "Get screenshots from a list of file";
                
                
                app.OnExecuteAsync(async cancellationToken => 
                {
                    Prompt.GetYesNo("Yakin kah?",false);
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,"https://localhost:5001/todos");
                    HttpResponseMessage response = await client.SendAsync(request);
                    var json = await response.Content.ReadAsStringAsync();
                    
                    var list = JsonSerializer.Deserialize<List<Todo>>(json);
                    var x = from l in list select l.id;
                    foreach(var y in x)
                    {
                        var responses = await client.DeleteAsync($"https://localhost:5001/todos/{y}");
                    }
                });
            });

            root.Command("update",app => 
            {
                app.Description = "Get screenshots from a list of file";

                var text = app.Argument("Text","Masukkan Text",true);
                app.OnExecuteAsync(async cancellationToken => 
                {
                    var add = "{" + "\"activity\":" + $"\"{text.Values[1]}\"" + "}";
                    var hasil = new StringContent(add,Encoding.UTF8,"application/json");
                    var responses = await client.PatchAsync($"https://localhost:5001/todos/{text.Values[0]}",hasil);
                });
            });

            root.Command("delete",app => 
            {
                app.Description = "Get screenshots from a list of file";
                var text = app.Argument("Text","Masukkan Text");
                app.OnExecuteAsync(async cancellationToken => 
                {   
                    var responses = await client.DeleteAsync($"https://localhost:5001/todos/{text.Value}");
                });
            });

            root.Command("done",app => 
            {
                app.Description = "Get screenshots from a list of file";

                var text = app.Argument("Text","Masukkan Text");
                app.OnExecuteAsync(async cancellationToken => 
                {   
                    var add = "{" + "\"status\":" + "\"Done\"" + "}";
                    var hasil = new StringContent(add,Encoding.UTF8,"application/json");
                    var responses = await client.PatchAsync($"https://localhost:5001/todos/done/{text.Value}",hasil);
                });
            });

            return root.Execute(args);
    }
    }
}
