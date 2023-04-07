using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Client;

public partial class MainWindow : Window
{
    private bool _xTurn;

    private int[,] _board;

    private TcpClient _client;
    private NetworkStream _stream;


    private Button[,] _buttons;
    private BinaryReader _br;
    private BinaryWriter _bw;

    public MainWindow()
    {
        InitializeComponent();
        _board = new int[3, 3];
        _buttons = new Button[3, 3]
        {
          { btn00, btn01, btn02 },
          { btn10, btn11, btn12 },
          { btn20, btn21, btn22 }
        };

        _client = new TcpClient("localhost", 1234);
        _stream = _client.GetStream();
        _br = new BinaryReader(_stream);
        _bw = new BinaryWriter(_stream);

        _xTurn = _br.ReadBoolean();

        if (_xTurn)
            Title = "Player: X";
        else
            Title = "Player: O";



        Task.Run(ReceiveUpdates);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            int row = Grid.GetRow(button);
            int column = Grid.GetColumn(button);

            if (_xTurn)
            {
                button.Content = "X";
                _board[row, column] = 1;
            }
            else
            {
                button.Content = "O";
                _board[row, column] = 2;
            }

            button.IsEnabled = false;

            CheckForWinner();
            _bw.Write(GetGameState());
            Grid.IsEnabled = false;
        }
    }

    private void ReceiveUpdates()
    {

        while (true)
        {
            Dispatcher.Invoke(() => { Grid.IsEnabled = true; });
            string received = _br.ReadString();
            UpdateGameState(received);
        }
    }

    private string GetGameState()
    {
        string gameState = "";

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                gameState += _board[i, j];
            }
        }

        return gameState;
    }

    private void UpdateGameState(string gameState)
    {
        for (int i = 0; i < 9; i++)
        {
            int row = i / 3;
            int col = i % 3;
            int value = int.Parse(gameState[i].ToString());
            _board[row, col] = value;

            Dispatcher.Invoke(() =>
            {
                _buttons[row, col].Content = value == 1 ? "X" : value == 2 ? "O" : "";
                _buttons[row, col].IsEnabled = value == 0;
            });
        }
    }

    private void CheckForWinner()
    {

        for (int i = 0; i < 3; i++)
        {
            if (_board[i, 0] != 0 && _board[i, 0] == _board[i, 1] && _board[i, 1] == _board[i, 2])
            {
                ShowWinner(_board[i, 0]);
                return;
            }
        }


        for (int i = 0; i < 3; i++)
        {
            if (_board[0, i] != 0 && _board[0, i] == _board[1, i] && _board[1, i] == _board[2, i])
            {
                ShowWinner(_board[0, i]);
                return;
            }
        }


        if (_board[0, 0] != 0 && _board[0, 0] == _board[1, 1] && _board[1, 1] == _board[2, 2])
        {
            ShowWinner(_board[0, 0]);
            return;
        }

        if (_board[0, 2] != 0 && _board[0, 2] == _board[1, 1] && _board[1, 1] == _board[2, 0])
        {
            ShowWinner(_board[0, 2]);
            return;
        }

        bool isTie = true;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (_board[i, j] == 0)
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
        _board = new int[3, 3];

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
