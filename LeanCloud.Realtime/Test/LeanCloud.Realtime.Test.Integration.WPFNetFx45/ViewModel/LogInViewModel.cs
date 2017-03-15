using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LeanCloud.Realtime.Test.Integration.WPFNetFx45.ViewModel
{
    public class LogInViewModel : ViewModelBase
    {
        public LogInViewModel()
        {
            AVRealtime.WebSocketLog(AppendLog);
            ConnectAsync = new RelayCommand(() => ConnectExecuteAsync(), () => true);
        }
        public AVRealtime realtime { get; internal set; }
        public AVIMClient client { get; internal set; }
        public ICommand ConnectAsync { get; private set; }

        private async void ConnectExecuteAsync()
        {
            Connecting = true;
            client = await realtime.CreateClient(ClienId);
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

        public string Log
        {
            get { return sbLog.ToString(); }
        }
    }
}
