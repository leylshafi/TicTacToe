using System.Net.Sockets;
using System.Net;

int playerId = 0;
var ip = IPAddress.Parse("127.0.0.1");
var port = 27001;

var listener = new TcpListener(ip, port);
listener.Start();

while (true)
{
    var client = listener.AcceptTcpClient();


    var clientStream = client.GetStream();
    var br = new BinaryReader(clientStream);
    var bw = new BinaryWriter(clientStream);

    bw.Write(playerId++);

    Console.WriteLine(playerId);
    
}