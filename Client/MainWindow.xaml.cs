using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
namespace Client;

public partial class MainWindow : Window
{
    private readonly string _character;
    private bool _isTurn;

    TcpClient tcpClient;
    BinaryWriter bw;
    BinaryReader br;

    public MainWindow()
    {
        InitializeComponent();

        tcpClient = new();
        var ip = IPAddress.Parse("127.0.0.1");
        var port = 27001;
        tcpClient.Connect(ip, port);

        var serverStream = tcpClient.GetStream();
        bw = new BinaryWriter(serverStream);
        br = new BinaryReader(serverStream);

        var player = br.Read();

        if(player==0)
            _character = "X";
        else _character = "O";
        _isTurn = false;
       
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if(sender is Button b)
        {
            b.Content = _character;
        }
    }
}
