using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

int playerId = 0;
IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
int port = 27001;
char[,] board = new char[3, 3];
TcpListener listener = new TcpListener(ipAddress, port);
Dictionary<int, TcpClient> clients = new Dictionary<int, TcpClient>();
// Dictionary<int, NetworkStream> streams = new Dictionary<int, NetworkStream>();
// int nextClientId = 1; 
NetworkStream clientStream = null;

bool player1Turn = true;  
bool gameEnded = false;

listener.Start();

BinaryWriter bw;
BinaryReader br;

while (true)
{

    try
    {
        var client = listener.AcceptTcpClient();
        clientStream = client.GetStream();
        br = new BinaryReader(clientStream);
        bw = new BinaryWriter(clientStream);

        bw.Write(playerId++);

        Console.WriteLine(playerId);

        while (true)
        {
            client = listener.AcceptTcpClient();
            Console.WriteLine("Connected to client.");

            InitializeBoard();
            PlayGame();

        }

    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
    finally
    {
        if (clientStream != null)
        {
            clientStream.Close();
        }
        if (clientStream != null)
        {
            clientStream.Close();
        }
    }
}

void InitializeBoard()
{
    for (int i = 0; i < 3; i++)
    {
        for (int j = 0; j < 3; j++)
        {
            board[i, j] = ' ';
        }
    }
}


void PlayGame()
{
    while (!gameEnded)
    {
        SendGameBoard();

        int[] move = GetMove();

        board[move[0], move[1]] = player1Turn ? 'X' : 'O';

        if (CheckForWinner())
        {
            EndGame("Player " + (player1Turn ? "1" : "2") + " wins!");
        }
        else if (CheckForTie())
        {
            EndGame("The game is a tie.");
        }

        player1Turn = !player1Turn;
    }
}

void SendGameBoard()
{
    string gameBoard = board[0, 0] + "|" + board[0, 1] + "|" + board[0, 2] + "|" +
                       board[1, 0] + "|" + board[1, 1] + "|" + board[1, 2] + "|" +
                       board[2, 0] + "|" + board[2, 1] + "|" + board[2, 2];
    byte[] bytes = Encoding.ASCII.GetBytes(gameBoard);
    bw.Write(bytes, 0, bytes.Length);
}

int[] GetMove()
{
    byte[] buffer = new byte[1024];
    int bytesRead = br.Read(buffer, 0, buffer.Length);
    string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
    int[] move = Array.ConvertAll(data.Split(','), int.Parse);
    return move;
}

bool CheckForTie()
{
    for (int i = 0; i < 3; i++)
    {
        for (int j = 0; j < 3; j++)
        {
            if (board[i, j] == ' ')
            {
                return false;
            }
        }
    }
    return true;
}

bool CheckForWinner()
{
    for (int i = 0; i < 3; i++)
    {
        if (board[i, 0] == board[i, 1] && board[i, 1] == board[i, 2] && board[i, 0] != ' ')
        {
            return true;
        }

        if (board[0, i] == board[1, i] && board[1, i] == board[2, i] && board[0, i] != ' ')
        {
            return true;
        }
    }

    if (board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2] && board[0, 0] != ' ')
    {
        return true;
    }
    if (board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0] && board[0, 2] != ' ')
    {
        return true;
    }

    return false;
}

void EndGame(string message)
{
    gameEnded = true;
    byte[] bytes = Encoding.ASCII.GetBytes(message);
    bw.Write(bytes, 0, bytes.Length);
}