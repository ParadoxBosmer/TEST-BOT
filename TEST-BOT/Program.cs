// See https://aka.ms/new-console-template for more information

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using SharedLibrary;
using SharedLibrary.DTOs;

string baseURL = "http://localhost:8080/";

Console.WriteLine("DOBRODOŠAO U AIBG V5.0 IGRU \n\n\n");


Console.WriteLine("Ukucaj ime svog lika");
string name= Console.ReadLine();
Player player=new Player();
Console.WriteLine("Ukucaj sifru svoje igre");
string gameId = Console.ReadLine();

HttpClient client = new HttpClient();
string s = $"{baseURL}game/connect/{gameId}/{name}";
var result = await client.PostAsync(s,null);

if (result.IsSuccessStatusCode)
{
    string jsonString = await result.Content.ReadAsStringAsync();
    
    HTTP_ResponseDTO response = JsonConvert.DeserializeObject<HTTP_ResponseDTO>(jsonString);
    
    Console.WriteLine($"Mapa uspešno učitana!");
    Console.WriteLine(response.map.ToString());
    player = response.player;
    Console.WriteLine("Ovde stoji ime bota" +player.Name);
    Console.WriteLine("Id: "+ player.Id);
    Console.WriteLine("Position: "+ player.Position.X +" "+ player.Position.Y);
}
else
{
    Console.WriteLine($"Greška na serveru: {result.StatusCode}");
}

string move1 = $"{baseURL}player/move/gameId/{gameId}";
MoveRequest mr = new MoveRequest(){Direction = Direction.UP,newPosition = new Position(0,1),playerId=player.Id,Steps=1};


if (player.Position.X > 0)
{
    mr.Direction = Direction.DOWN;
    var newPos = new Position(player.Position.X, player.Position.Y-1);
    mr.newPosition = newPos;
}

if (!player.First)
{
    Console.WriteLine("Waiting for the first player to play his move");
    await Task.Delay(3000);
}

string jsonBody = JsonConvert.SerializeObject(mr);
var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

result = await client.PutAsync(move1,httpContent);
if (result.IsSuccessStatusCode)
{
    string jsonString = await result.Content.ReadAsStringAsync();

    HTTP_ResponseDTO response = JsonConvert.DeserializeObject<HTTP_ResponseDTO>(jsonString);
    
    Console.WriteLine($"Pomereno uspesno");
    player = response.player;
    Console.WriteLine($"filed je {player.Position.X},{player.Position.Y}");
    Console.WriteLine($"Trenutno stanje igraca je sledece \n ({player.Position.X},{player.Position.Y})");
}
else
{
    Console.WriteLine($"Greška na serveru: {result.StatusCode}");
}

Console.WriteLine("\nPritisni Enter za izlaz...");
Console.ReadLine();