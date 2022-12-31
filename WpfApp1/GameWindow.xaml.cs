using SimpleTCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        SimpleTcpServer server;
        SimpleTcpClient client;
        List<TcpClient> clients;
        int current_round = 0;
        int manCount;
        int roundCount;
        int participateCount = 0;
        string serverAnswer;
        int serverParticipate;
        int selfScore = 0;
        Role role;
        int played_rounds = 0;
        Dictionary<TcpClient, String> clientMessages;
        Dictionary<TcpClient, int> clientScores = new Dictionary<TcpClient, int>();
        List<String> riddles = new List<string>
        { "Do you think justice really exists?",
           "If a stranger offered to give your family 1 million dollars if you go to jail for 10 years, would you do it?" ,
            "Would you take the blame for a major crime to save a best friend’s life?",
             "If you could find out every person who ever had a crush on you, would you?",
               "Would you want to be a millionaire if it meant you would never find true love?" ,
                 "If you could know what other people really thought of you, would you want to know?"};

        public GameWindow(int round,int maxPlayer,Role role)
        {
            InitializeComponent();
            manCount = maxPlayer;
            roundCount = round;
            switch (role)
            {
                case Role.Host:
                    if (server == null)
                    {
                        server = new SimpleTcpServer();
                        server.StringEncoder = Encoding.UTF8;
                        server.Delimiter = 13;
                        server.DataReceived += Server_DataReceived;
                        server.ClientConnected += Server_ClientConnected;
                        clientMessages = new Dictionary<TcpClient, String>();
                        clients = new List<TcpClient>();
                        IPAddress ip = IPAddress.Parse("127.0.0.1");
                        server.Start(ip, 4567);
                        role = Role.Host;
                    }
                    break;
                case Role.Player:
                    if (client == null)
                    {
                        client = new SimpleTcpClient();
                        client.StringEncoder = Encoding.UTF8;
                        client.Delimiter = 13;
                        client.DataReceived += Client_DataReceived;
                        client.Connect("127.0.0.1", 4567);
                        nextButton.Visibility = Visibility.Hidden;
                    }
                    break;
                default:
                    break;
            }
            this.role = role;
        }

        private void Server_ClientConnected(object? sender, System.Net.Sockets.TcpClient e)
        {
            clients.Add(e);
            clientScores.Add(e, 0);
        }

        private void Client_DataReceived(object? sender, Message e)
        {
            if (e.MessageString.Equals("right"))
            {
                this.selfScore += 2;
            }else if(e.MessageString.Equals("you have the closest answer"))
            {
                this.selfScore += 1;
            } else if(e.MessageString.Contains("you win!")){
                MessageBox.Show("you win!");

            }


            this.Dispatcher.Invoke(() =>
            {
                questionBox.Text = e.MessageString;
                scoreTxtBox.Text = selfScore.ToString();
            });
        }

        private void Server_DataReceived(object? sender, Message e)
        {
            var cli = clients.Find(c => c == e.TcpClient);
            clientMessages.Add(cli, e.MessageString);
            if (Int32.Parse(e.MessageString) > 0)
                participateCount++;
            if (role == Role.Host)
                writeAnswer();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(clients.Count < manCount - 1)
            {
                MessageBox.Show("wait for all player to connect in");
                return;
            }

            current_round++;
            participateCount = 0;
            clientMessages.Clear();
            serverAnswer = "";
            int rand = new Random().Next(riddles.Count-1);
            server.Broadcast(Encoding.UTF8.GetBytes(riddles[rand]));
            this.Dispatcher.Invoke(() =>
            {
                questionBox.Text = riddles[rand];
            });

            played_rounds++;
            if(played_rounds== roundCount)
            {
                nextButton.IsEnabled = false;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (role == Role.Player)
            {
                client.TcpClient.Client.Send(Encoding.UTF8.GetBytes(answerBox.Text.Length > 0 ? answerBox.Text : "1"));
            }
            else
            {
                serverParticipate = 1;
                serverAnswer = answerBox.Text.Length > 0 ? answerBox.Text : "1";
            }
            if (role == Role.Host)
                writeAnswer();
         }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (role == Role.Player)
            {
                client.TcpClient.Client.Send(Encoding.UTF8.GetBytes("-100000"));
                serverParticipate = 0;
            }
            else if (role == Role.Host)
            {
                serverAnswer = "-1000000";
                writeAnswer();
            }
        }

        private void writeAnswer()
        {
            if (clientMessages.Count == manCount - 1 && serverAnswer.Length > 0)
            {
                participateCount += serverParticipate;
                var haveWinner = (from s in clientMessages.Values where Int32.Parse(s) == participateCount select s).FirstOrDefault();
                var s_ans = Int32.Parse(serverAnswer);
                if (haveWinner == null && s_ans != participateCount)
                {
                    if (Math.Abs(s_ans - participateCount) <=1 )
                    {   
                        if (role == Role.Host)
                        {
                            selfScore += 1;
                            this.Dispatcher.Invoke(() =>
                            {
                                scoreTxtBox.Text = selfScore.ToString();
                                questionBox.Text = "you have the closest answer";
                            });
                        }
                    }
                    else
                    {
                        if (role == Role.Host)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                scoreTxtBox.Text = selfScore.ToString();
                                questionBox.Text = "wrong";
                            });
                        }
                    }

                    foreach (var c in clientMessages)
                    {
                        if (Math.Abs(Int32.Parse(c.Value) - participateCount)<=1)
                        {
                            c.Key.Client.Send(Encoding.UTF8.GetBytes("you have the closest answer"));
                            clientScores[c.Key] += 1;
                        }
                        else
                        {
                            c.Key.Client.Send(Encoding.UTF8.GetBytes("wrong"));
                        }
                    }

                }
                else
                {
                    foreach (var c in clientMessages)
                    {
                        if (Int32.Parse(c.Value) == participateCount)
                        {
                            c.Key.Client.Send(Encoding.UTF8.GetBytes("right"));
                            clientScores[c.Key] += 2;
                        }
                        else
                        {
                            c.Key.Client.Send(Encoding.UTF8.GetBytes("wrong"));
                        }
                    }

                    if (role == Role.Host)
                    {
 
                            selfScore += s_ans == participateCount ? 2 : 0;
                            this.Dispatcher.Invoke(() =>
                            {
                                scoreTxtBox.Text = selfScore.ToString();
                                questionBox.Text = s_ans == participateCount ? "right" : "wrong";
                            });
                    }
                }
                if(current_round == roundCount)
                {
                    int max = 0;
                    TcpClient maxPlayer;
                    foreach(var s in clientScores)
                    {
                        if(s.Value > max)
                        {
                            maxPlayer = s.Key; max = s.Value;
                        }
                    }

                    max = max > selfScore ? max : selfScore;
                    
                    if(role == Role.Host) { 
                        if(selfScore == max)
                        {
                            MessageBox.Show("you win");
                        }
                    }

                    foreach(var s in clientScores)
                    {
                        if(s.Value == max)
                        {
                            s.Key.Client.Send(Encoding.UTF8.GetBytes("you win!"));
                        }
                    }

                }
            }
        }
    }
}
