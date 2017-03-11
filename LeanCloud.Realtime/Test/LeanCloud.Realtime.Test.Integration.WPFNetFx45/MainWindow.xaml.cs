using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LeanCloud.Realtime.Test.Integration.WPFNetFx45
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public AVRealtime realtime;
        public AVIMClient client;

        public MainWindow()
        {
            Websockets.Net.WebsocketConnection.Link();
            InitializeComponent();
        }

        private async void btn_LogIn_Click(object sender, RoutedEventArgs e)
        {
            realtime = new AVRealtime("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");
            client = await realtime.CreateClient("junwu");
            var textMessageLisenter = new AVIMTextMessageListener();
            client.RegisterListener(textMessageLisenter);
        }
    }
}
