using System.Net;
using System.Net.Sockets;

IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
int port = 1234;
TcpListener listener = new TcpListener(ipAddress, port);


Dictionary<int, TcpClient> clients = new Dictionary<int, TcpClient>();
Dictionary<int, NetworkStream> streams = new Dictionary<int, NetworkStream>();

int nextClientId = 0;
listener.Start();
while (true)
{
    TcpClient client = listener.AcceptTcpClient();
    int clientId = nextClientId++;
    clients.Add(clientId, client);
    NetworkStream stream = client.GetStream();
    streams.Add(clientId, stream);

    foreach (int otherClientId in clients.Keys)
    {
        if (otherClientId != clientId)
        {
            BinaryWriter bw = new BinaryWriter(streams[clientId]);
            bw.Write(true);
            bw = new BinaryWriter(streams[otherClientId]);
            bw.Write(false);
            break;
        }
    }

    Task.Run(() =>
    {
        var br = new BinaryReader(streams[clientId]);
        while (true)
        {
            try
            {
                var receivedData = br.ReadString();


                foreach (int otherClientId in clients.Keys)
                {
                    if (otherClientId != clientId)
                    {
                        var bw = new BinaryWriter(streams[otherClientId]);
                        bw.Write(receivedData);
                    }
                }
            }
            catch
            {
                clients.Remove(clientId);
                streams.Remove(clientId);
                return;
            }
        }
    });
}