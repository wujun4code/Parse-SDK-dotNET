using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LeanCloud.Realtime.Test.Integration.WPFNetFx45.ViewModel
{
    public class ChatViewModel : ViewModelBase
    {

        public ChatViewModel()
        {
            this.SessionGroups = new ObservableCollection<ConversationGroupViewModel>();
            SessionGroups.Add(new ConversationGroupViewModel()
            {
                Name = "群聊",
                Sessions = new ObservableCollection<ConversationSessionViewModel>()
            {
                new ConversationSessionViewModel()
                {
                     Name= "所有人"
                },
            }
            });
            SessionGroups.Add(new ConversationGroupViewModel() { Name = "私聊", });
            SessionGroups.Add(new ConversationGroupViewModel() { Name = "未加入", });


            this.SelectedSession = new ConversationSessionViewModel()
            {
                Name = "所有人",
            };
        }
        private ConversationSessionViewModel _selectedSession;
        public ConversationSessionViewModel SelectedSession
        {
            get
            {
                return _selectedSession;
            }
            set
            {
                _selectedSession = value;
                RaisePropertyChanged("SelectedSession");
            }
        }

        private ObservableCollection<ConversationGroupViewModel> _sessionGroups;
        public ObservableCollection<ConversationGroupViewModel> SessionGroups
        {
            get
            {
                return _sessionGroups;
            }
            set
            {
                _sessionGroups = value;
                RaisePropertyChanged("SessionGroups");
            }
        }

        public AVIMClient client { get; internal set; }

        public async Task InitSessionGroups()
        {
            var conversations = await client.GetQuery().FindAsync();
            if (conversations.Count() > 0)
            {
                var latestConversation = conversations.First();
                this.SelectedSession = new ConversationSessionViewModel(latestConversation);
                await SelectedSession.LoadHistory();
            }
        }
    }

    /// <summary>
    /// 对话分组，类似于用户标签，比如我的家人，我的朋友这种分组
    /// </summary>
    public class ConversationGroupViewModel : ViewModelBase
    {
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name == value)
                    return;
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
        public ObservableCollection<ConversationSessionViewModel> Sessions { get; set; }
    }

    public class ConversationSessionViewModel : ViewModelBase
    {
        public ConversationSessionViewModel(AVIMConversation conversation)
            : this()
        {
            this.ConversationInSession = conversation;
            Name = this.ConversationInSession.Name;

            var textMessageListener = new AVIMTextMessageListener();
            textMessageListener.OnTextMessageReceived += TextMessageListener_OnTextMessageReceived;

            conversation.RegisterListener(textMessageListener);
        }

        private void TextMessageListener_OnTextMessageReceived(object sender, AVIMTextMessageEventArgs e)
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                MessagesInSession.Add(new MessageViewModel(e.TextMessage));
            });
        }

        public ConversationSessionViewModel()
        {
            SendAsync = new RelayCommand(() => SendExecuteAsync(), () => true);
        }
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name == value)
                    return;
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        private string _inputText;
        public string InputText
        {
            get
            {
                return _inputText;
            }
            set
            {
                if (_inputText == value)
                    return;
                _inputText = value;
                RaisePropertyChanged("InputText");
            }
        }

        public ICommand SendAsync { get; private set; }
        private async void SendExecuteAsync()
        {
            var textMessage = new AVIMTextMessage(this.InputText);
            await ConversationInSession.SendMessageAsync(textMessage);
            this.InputText = "";
        }
        public AVIMConversation ConversationInSession { get; set; }

        private ObservableCollection<MessageViewModel> _messagesInSession;
        public ObservableCollection<MessageViewModel> MessagesInSession
        {
            get
            {
                return _messagesInSession;
            }
            set
            {
                _messagesInSession = value;
                RaisePropertyChanged("MessagesInSession");
            }
        }

        public async Task LoadHistory(int limit = 20)
        {
            if (MessagesInSession == null) MessagesInSession = new ObservableCollection<MessageViewModel>();
            if (ConversationInSession == null) return;
            var messages = await ConversationInSession.QueryMessageAsync(limit: limit);
            messages.ToList().ForEach(x =>
            {
                MessagesInSession.Add(new MessageViewModel(x));
            });
        }
    }

    public class MessageViewModel : ViewModelBase
    {
        public MessageViewModel(AVIMMessage avMessage)
        {
            if (avMessage is AVIMTextMessage)
            {
                this.Text = ((AVIMTextMessage)avMessage).TextContent;
            }
            else
            {
                this.Text = "当前客户端不支持显示此类型消息。";
            }
            this.Sender = avMessage.FromClientId;
            this.Code = this.Sender.First();
        }

        private bool _served;
        public bool Served
        {

            get
            {
                return _served;
            }
            set
            {
                if (_served == value)
                    return;
                _served = value;
                RaisePropertyChanged("Served");
            }
        }

        public bool _read;
        public bool Read
        {

            get
            {
                return _read;
            }
            set
            {
                if (_read == value)
                    return;
                _read = value;
                RaisePropertyChanged("Read");
            }
        }

        private string _text;
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text == value)
                    return;
                _text = value;
                RaisePropertyChanged("Text");
            }
        }

        private string _sender;
        public string Sender
        {
            get
            {
                return _sender;
            }
            set
            {
                if (_sender == value)
                    return;
                _sender = value;
                RaisePropertyChanged("Sender");
            }
        }

        private char _code;
        public char Code
        {
            get { return _code; }
            set
            {
                if (_code == value) return;
                _code = value;
                RaisePropertyChanged("Code");
            }
        }
    }
}
