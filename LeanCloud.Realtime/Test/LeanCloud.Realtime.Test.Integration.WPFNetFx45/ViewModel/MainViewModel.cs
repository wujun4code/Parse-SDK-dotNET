using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LeanCloud.Realtime.Test.Integration.WPFNetFx45.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

        private UserControl _leftContent;
        private UserControl _centerContent;
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            Websockets.Net.WebsocketConnection.Link();
            realtime = new AVRealtime("uay57kigwe0b6f5n0e1d4z4xhydsml3dor24bzwvzr57wdap", "kfgz7jjfsk55r5a8a3y4ttd3je1ko11bkibcikonk32oozww");
            this._centerContent = new LogIn();
            var logInVM = (this._centerContent.DataContext as LogInViewModel);
            logInVM.realtime = realtime;
            logInVM.client = CurrentClient;
        }

        public UserControl CenterContent
        {
            get { return _centerContent; }
            set { this.MutateVerbose(ref _centerContent, value, RaisePropertyChanged()); }
        }

        public UserControl LeftContent
        {
            get { return _leftContent; }
            set { this.MutateVerbose(ref _leftContent, value, RaisePropertyChanged()); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private Action<PropertyChangedEventArgs> RaisePropertyChanged()
        {
            return args => PropertyChanged?.Invoke(this, args);
        }
    }

    public static class NotifyPropertyChangedExtension
    {
        public static void MutateVerbose<TField>(this INotifyPropertyChanged instance, ref TField field, TField newValue, Action<PropertyChangedEventArgs> raise, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<TField>.Default.Equals(field, newValue)) return;
            field = newValue;
            raise?.Invoke(new PropertyChangedEventArgs(propertyName));
        }
    }
}