using System.Collections.Generic;
using System.Collections.ObjectModel;

// ϵͳ��־
namespace Client
{
    public partial class SystemLog
    {
        public SystemLog()
        {
            InitializeComponent();

            Function.SetTheme(false, systemLogInfoList_DataGrid);

            // ��ȡϵͳ��־��Ϣ�б�
            Dictionary<string, string> postParameter = new Dictionary<string, string> { };
            ObservableCollection<SystemLogInfo> systemLogInfoList = new HttpRequest("?packageName=ToolBar&structName=SystemLog&funcName=GetSystemLogInfoList", postParameter).GetListRequest<SystemLogInfo>();
            List<SystemLogInfo> list = new List<SystemLogInfo>(systemLogInfoList);
            list.Reverse();
            systemLogInfoList_DataGrid.ItemsSource = list;
        }
    }
}