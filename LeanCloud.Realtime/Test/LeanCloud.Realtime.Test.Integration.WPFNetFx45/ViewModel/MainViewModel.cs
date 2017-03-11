using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LeanCloud.Realtime.Test.Integration.WPFNetFx45.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public AVRealtime realtime { get; internal set; }
        public AVIMClient CurrentClient { get; internal set; }
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
            Websockets.Net.WebsocketConnection.Link();
            realtime = new AVRealtime("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");
            AVRealtime.WebSocketLog(AppendLog);
            ConnectAsync = new RelayCommand(() => ConnectExecuteAsync(), () => true);
        }
        public ICommand ConnectAsync { get; private set; }

        private async void ConnectExecuteAsync()
        {
            Connecting = true;
            CurrentClient = await realtime.CreateClient(ClienId);
            Connecting = false;
            Connected = true;
        }
        private StringBuilder sbLog = new StringBuilder();
        public void AppendLog(string log)
        {
            sbLog.AppendLine(log);
            RaisePropertyChanged("Log");
        }

        private string _clientId;
        public string ClienId
        {
            get
            {
                return _clientId;
            }
            set
            {
                if (_clientId == value)
                    return;
                _clientId = value;
                RaisePropertyChanged("ClienId");
            }
        }

        private bool _connecting;
        public bool Connecting
        {
            get
            {
                return _connecting;
            }
            set
            {
                if (_connecting == value)
                    return;
                _connecting = value;
                RaisePropertyChanged("Connecting");
            }
        }

        private bool _connected;
        public bool Connected
        {
            get
            {
                return _connected;
            }
            set
            {
                if (_connected == value)
                    return;
                _connected = value;
                RaisePropertyChanged("Connected");
            }
        }

        private string _log;
        public string Log
        {
            get { return sbLog.ToString(); }
        }

    }
}