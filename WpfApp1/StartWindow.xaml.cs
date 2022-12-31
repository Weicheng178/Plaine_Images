using System;
using System.Windows;
using System.Windows.Controls;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public enum Role
    {
        Host,
        Player
    }
    
    public partial class StartWindow : Window
    {
        Role role;

        public StartWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var round = Int32.Parse(roundTxtBox.Text);
            var players = Int32.Parse(playersTxtBox.Text);
            var gameWindow = new GameWindow(round,players,role);
            gameWindow.Show();
            this.Close();
        }

        private void roundTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            var gameWindow = new GameWindow(0, 0, Role.Player);
            gameWindow.Show();
            this.Close();
        }
    }
}
