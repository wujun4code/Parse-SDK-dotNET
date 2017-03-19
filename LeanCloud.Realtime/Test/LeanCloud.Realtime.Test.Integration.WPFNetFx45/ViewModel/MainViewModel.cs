using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LeanCloud.Realtime.Test.Integration.WPFNetFx45.ViewModel;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

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

        private UserControl _leftContent;
        private UserControl _centerContent;
        private UserControl _bottomContent;
        private UserControl _logContent;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            Websockets.Net.WebsocketConnection.Link();
            realtime = new AVRealtime("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");


            this.CenterContent = new LogIn();
            var logInVM = ServiceLocator.Current.GetInstance<LogInViewModel>();
            logInVM.realtime = realtime;
            logInVM.client = CurrentClient;

            logInVM.PropertyChanged += LogInVM_PropertyChanged;

            this.LogContent = new WebSocketLog();
            var logVM = ServiceLocator.Current.GetInstance<WebSocketLogViewModel>();
        }

        private async void LogInVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Connected")
            {
                var logInVM = ServiceLocator.Current.GetInstance<LogInViewModel>();
                if (realtime.State == AVRealtime.Status.Online)
                {
                    var chatVM = ServiceLocator.Current.GetInstance<ChatViewModel>();
                    chatVM.client = logInVM.client;
                    chatVM.client.OnSessionClosed += Client_OnSessionClosed;
                    await chatVM.InitSessionGroups();
                }
                this.CenterContent = new Chat();
                this.LeftContent = new ConversationGroup();
                this.BottomContent = new Compose();

            }
        }

        private void Client_OnSessionClosed(object sender, AVIMSessionClosedEventArgs e)
        {
            var logVM = ServiceLocator.Current.GetInstance<WebSocketLogViewModel>();
            logVM.AppendLog("session closed:" + e.Code + "||" + e.Reason + "||" + e.Detail);
        }

        public UserControl CenterContent
        {
            get { return _centerContent; }
            set
            {
                _centerContent = value;
                RaisePropertyChanged("CenterContent");
            }
        }

        public UserControl LeftContent
        {
            get { return _leftContent; }
            set
            {
                this._leftContent = value;
                RaisePropertyChanged("LeftContent");
            }
        }

        public UserControl BottomContent
        {
            get { return _bottomContent; }
            set
            {
                this._bottomContent = value;
                RaisePropertyChanged("BottomContent");
            }
        }
        public UserControl LogContent
        {
            get { return _logContent; }
            set
            {
                this._logContent = value;
                RaisePropertyChanged("LogContent");
            }
        }
    }
}