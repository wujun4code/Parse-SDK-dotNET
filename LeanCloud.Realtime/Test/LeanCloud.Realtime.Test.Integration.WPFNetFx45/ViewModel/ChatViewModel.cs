using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanCloud.Realtime.Test.Integration.WPFNetFx45.ViewModel
{
    public class ChatViewModel : ViewModelBase
    {
        public AVIMConversation SelectedConversation { get; set; }

        public async void LoadHistory(int limit)
        {

        }

        public ObservableCollection<ConversationGroup> ConversationGroups { get; set; }
    }

    /// <summary>
    /// 对话分组，类似于用户标签，比如我的家人，我的朋友这种分组
    /// </summary>
    public class ConversationGroup
    {
        public ObservableCollection<AVIMConversation> Conversations { get; set; }
    }
}
