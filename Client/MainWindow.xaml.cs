using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Client;

public partial class MainWindow : Window
{
    private bool xTurn = true;
    private int[,] board = new int[3, 3];
    private TcpClient _tcpClient;
    Button[,] buttons;
    private readonly string _character;

    TcpClient tcpClient;
    BinaryWriter bw;
    BinaryReader br;

    public MainWindow()
    {
        InitializeComponent();
        buttons = new Button[3, 3] {
        { btn00, btn01, btn02 },
        {btn00, btn01, btn12},
        { btn00, btn01, btn22 }
    };


        tcpClient = new();
        var ip = IPAddress.Parse("127.0.0.1");
        var port = 27001;
        tcpClient.Connect(ip, port);

        var serverStream = tcpClient.GetStream();
        bw = new BinaryWriter(serverStream);
        br = new BinaryReader(serverStream);

        var player = br.Read();

        Title="Player "+player.ToString();

        if (player%2==0)
            _character = "X";
        else _character = "O";

        Task.Run(ReceiveUpdates);
    }
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Button button = (Button)sender;
        int row = Grid.GetRow(button);
        int column = Grid.GetColumn(button);

        if (xTurn)
        {
            board[row, column] = 1;
            button.Content = "X";
        }
        else
        {
            board[row, column] = 2;
            button.Content = "O";
        }

        button.IsEnabled = false;
        xTurn = !xTurn;

        CheckForWinner();
        string gameState = GetGame();
        byte[] data = Encoding.ASCII.GetBytes(gameState);
        bw.Write(data, 0, data.Length);
    }
    private Task ReceiveUpdates()
    {
        while (true)
        {
            byte[] data = new byte[1024];
            int bytesRead = br.Read(data, 0, data.Length);

            if (bytesRead > 0)
            {
                string receivedData = Encoding.ASCII.GetString(data, 0, bytesRead);
                UpdateGame(receivedData);
            }
        }
    }

    private string GetGame()
    {
        string gameState = "";

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                gameState += board[i, j];
            }
        }

        return gameState;
    }

    private void UpdateGame(string gameState)
    {
        int index = 0;

        foreach (Button button in buttons)
        {
            int value = int.Parse(gameState[index].ToString());
            board[index / 3, index % 3] = value;

            button.Content = value == 1 ? "X" : value == 2 ? "O" : "";
            button.IsEnabled = value == 0;

            index++;
        }

        xTurn = !xTurn;
    }

    private void CheckForWinner()
    {

        for (int i = 0; i < 3; i++)
        {
            if (board[i, 0] != 0 && board[i, 0] == board[i, 1] && board[i, 1] == board[i, 2])
            {
                ShowWinner(board[i, 0]);
                return;
            }
        }


        for (int i = 0; i < 3; i++)
        {
            if (board[0, i] != 0 && board[0, i] == board[1, i] && board[1, i] == board[2, i])
            {
                ShowWinner(board[0, i]);
                return;
            }
        }


        if (board[0, 0] != 0 && board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2])
        {
            ShowWinner(board[0, 0]);
            return;
        }

        if (board[0, 2] != 0 && board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0])
        {
            ShowWinner(board[0, 2]);
            return;
        }

        bool isTie = true;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (board[i, j] == 0)
                {
                    isTie = false;
                    break;
                }
            }

            if (!isTie)
            {
                break;
            }
        }

        if (isTie)
        {
            ShowTie();
        }
    }

    private void ShowWinner(int player)
    {
        MessageBox.Show($"Player {(player == 1 ? "X" : "O")} wins!");
        ResetGame();
    }

    private void ShowTie()
    {
        MessageBox.Show("It's a tie!");
        ResetGame();
    }

    private void ResetGame()
    {
        board = new int[3, 3];
        xTurn = true;

        foreach (var element in Grid.Children)
        {
            if (element is Button button)
            {
                button.Content = "";
                button.IsEnabled = true;
            }
        }
    }
}
