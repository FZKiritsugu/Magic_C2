using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;

// ��������
namespace Client
{
    public partial class ListenerConfig
    {
        private ListenerInfo selectedListenerInfo;

        public ListenerConfig()
        {
            InitializeComponent();

            Function.SetTheme(false, listenerInfoList_DataGrid);

            protocol_ComboBox.Items.Add("HTTP");
            GetListenerInfoList();
        }

        // ��ȡ��������Ϣ�б�
        private void GetListenerInfoList()
        {
            Dictionary<string, string> postParameter = new Dictionary<string, string> { };
            listenerInfoList_DataGrid.ItemsSource = new HttpRequest("?packageName=ToolBar&structName=ListenerConfig&funcName=GetListenerInfoList", postParameter).GetListRequest<ListenerInfo>();
        }

        // ��ʾ��������Ϣ
        private void DisplayListenerInfo_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedListenerInfo = (sender as DataGridRow)?.Item as ListenerInfo;
            if (selectedListenerInfo == null)
            {
                return;
            }
            name_TextBox.Text = selectedListenerInfo.name;
            description_TextBox.Text = selectedListenerInfo.description;
            protocol_ComboBox.SelectedValue = selectedListenerInfo.protocol;
            port_TextBox.Text = selectedListenerInfo.port;
            Function.SetRadioResult(connectType_WrapPanel, selectedListenerInfo.connectType);
        }

        // ���ļ�������Ϣ
        private void CuListenerInfo(string method)
        {
            // ����
            string id = null;
            if (method == "UpdateListenerInfo")
            {
                if (selectedListenerInfo != null)
                {
                    id = selectedListenerInfo.id;
                }
                else
                {
                    return;
                }
            }
            string name = name_TextBox.Text;
            string description = description_TextBox.Text;
            string protocol = (string)protocol_ComboBox.SelectedValue;
            if (protocol == null)
            {
                MessageBox.Show("δѡ��Э��", "����", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string port = port_TextBox.Text;
            if (port == "")
            {
                MessageBox.Show("�˿ڲ���Ϊ��", "����", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string connectType = Function.GetRadioResult(connectType_WrapPanel);
            if (connectType == null)
            {
                MessageBox.Show("δѡ����������", "����", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (connectType == "����")
            {
                MessageBox.Show("�ݲ�֧������", "����", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ���Ʋ���
            foreach (ListenerInfo listenerInfo in listenerInfoList_DataGrid.ItemsSource)
            {
                if (listenerInfo.name == name)
                {
                    if (method == "AddListenerInfo" || method == "UpdateListenerInfo" && listenerInfo.id != id)
                    {
                        MessageBox.Show("���Ʋ������ظ�", "����", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }

            Dictionary<string, string> postParameter = new Dictionary<string, string>
            {
                { "name", name },
                { "description", description },
                { "protocol", protocol },
                { "port", port },
                { "connectType", connectType }
            };
            if (method == "UpdateListenerInfo")
            {
                postParameter.Add("id", id);
            }
            new HttpRequest("?packageName=ToolBar&structName=ListenerConfig&funcName=" + method, postParameter).GeneralRequest();

            GetListenerInfoList();
        }

        // ��Ӽ�������Ϣ
        private void AddListenerInfo_Click(object sender, RoutedEventArgs e)
        {
            CuListenerInfo("AddListenerInfo");
        }

        // �޸ļ�������Ϣ
        private void UpdateListenerInfo_Click(object sender, RoutedEventArgs e)
        {
            CuListenerInfo("UpdateListenerInfo");
        }

        // ɾ����������Ϣ
        private void DeleteListenerInfo_Click(object sender, RoutedEventArgs e)
        {
            if (selectedListenerInfo == null)
            {
                return;
            }
            if (MessageBox.Show("�Ƿ�ɾ����������", "����", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Dictionary<string, string> postParameter = new Dictionary<string, string>
                {
                    { "id", selectedListenerInfo.id }
                };
                new HttpRequest("?packageName=ToolBar&structName=ListenerConfig&funcName=DeleteListenerInfo", postParameter).GeneralRequest();

                GetListenerInfoList();
            }
            selectedListenerInfo = null;
        }
    }
}