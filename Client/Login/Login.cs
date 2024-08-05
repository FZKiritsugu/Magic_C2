using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Data.SQLite;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Collections.ObjectModel;

// ��¼
namespace Client
{
    public partial class Login
    {
        private LoginInfo selectedLoginInfo;
        private ObservableCollection<LoginInfo> loginInfoList = new ObservableCollection<LoginInfo>();

        public Login()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Function.SetTheme(false, null);

            GetLoginInfo();
        }

        // ��ȡ��¼��Ϣ
        private void GetLoginInfo()
        {
            try
            {
                loginInfoList.Clear();
                using (SQLiteConnection conn = new SQLiteConnection(@"Data Source = config\client.db"))
                {
                    conn.Open();
                    string sql = "select * from LoginInfo";
                    using (SQLiteCommand sqlCommand = new SQLiteCommand(sql, conn))
                    {
                        using (SQLiteDataReader dataReader = sqlCommand.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                LoginInfo loginInfo = new LoginInfo()
                                {
                                    id = Convert.ToInt32(dataReader["id"]).ToString(),
                                    host = (string)dataReader["host"],
                                    port = (string)dataReader["port"],
                                    accessKey = (string)dataReader["accessKey"],
                                    username = (string)dataReader["username"],
                                    password = (string)dataReader["password"]
                                };
                                loginInfoList.Add(loginInfo);
                            }
                        }
                    }
                }
                loginInfoList_DataGrid.ItemsSource = loginInfoList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ��ʾ��¼��Ϣ
        private void DisplayLoginInfo_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedLoginInfo = (sender as DataGridRow)?.Item as LoginInfo;
            if (selectedLoginInfo == null)
            {
                return;
            }
            host_TextBox.Text = selectedLoginInfo.host;
            port_TextBox.Text = selectedLoginInfo.port;
            accessKey_TextBox.Text = selectedLoginInfo.accessKey;
            username_TextBox.Text = selectedLoginInfo.username;
            password_PasswordBox.Password = selectedLoginInfo.password;
        }

        // ��¼
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(@"C:\Windows\Temp\Magic C2 v1.0.0"))
            {
                if (!Function.CheckEnvironment())
                {
                    return;
                }
                string disclaimer = "��������\n1. ����Ŀ���������簲ȫ������ѧϰ�о���ּ����߰�ȫ�����������з��µĹ���������\n2. ��ִ��Ҫ������Ŀ������͸���ԵȰ�ȫҵ������ȷ���ѻ���㹻�ķ�����Ȩ���ڷ������簲ȫ���������½��С�\n3. ����Ŀ�ɸ��˶�����������δ��ȫ���������ԣ���ʹ���������⻷���в��Ա���Ŀ�Ĺ��ܡ�\n4. ����Ŀ��ȫ��Դ�����𽫱���Ŀ�����κ���ҵ��;��\n5. ��ʹ������ʹ�ñ���Ŀ�Ĺ����д����κ�Υ����Ϊ������κβ���Ӱ�죬��ʹ�������ге����Σ�����Ŀ�����޹ء�\n\n����ͬ����������������� ���ǡ��������벻Ҫʹ�ñ���Ŀ��";
                if (MessageBox.Show(disclaimer, "��������", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No)
                {
                    return;
                }
                File.Create(@"C:\Windows\Temp\Magic C2 v1.0.0");
            }

            // ���͵�¼����
            HttpRequest.serverUrl = "http://" + host_TextBox.Text + ":" + port_TextBox.Text;
            HttpRequest.cookieContainer = new CookieContainer();
            HttpRequest.cookieContainer.Add(new Uri(HttpRequest.serverUrl), new Cookie("accessKey", BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(accessKey_TextBox.Text))).Replace("-", "").ToLower()));
            HttpRequest.cookieContainer.Add(new Uri(HttpRequest.serverUrl), new Cookie("username", username_TextBox.Text));
            HttpRequest.cookieContainer.Add(new Uri(HttpRequest.serverUrl), new Cookie("password", BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(password_PasswordBox.Password))).Replace("-", "").ToLower()));
            Dictionary<string, string> postParameter = new Dictionary<string, string> { };
            string response = Encoding.UTF8.GetString(new HttpRequest("?packageName=&structName=&funcName=Login", postParameter).GeneralRequest());
            if (response == "" || response == "HttpRequestError")
            {
                return;
            }

            // �жϵ�¼��Ϣ�Ƿ��Ѵ���
            bool loginInfoExist = false;
            foreach (LoginInfo loginInfo in loginInfoList)
            {
                if (loginInfo.host == host_TextBox.Text && loginInfo.port == port_TextBox.Text && loginInfo.accessKey == accessKey_TextBox.Text && loginInfo.username == username_TextBox.Text && loginInfo.password == password_PasswordBox.Password)
                {
                    loginInfoExist = true;
                    break;
                }
            }

            // ��¼�µ�¼��Ϣ
            if (!loginInfoExist)
            {
                try
                {
                    using (SQLiteConnection conn = new SQLiteConnection(@"Data Source = config\client.db"))
                    {
                        conn.Open();
                        string sql = "insert into LoginInfo (host, port, accessKey, username, password) values (@host, @port, @accessKey, @username, @password)";
                        using (SQLiteCommand sqlCommand = new SQLiteCommand(sql, conn))
                        {
                            sqlCommand.Parameters.AddWithValue("@host", host_TextBox.Text);
                            sqlCommand.Parameters.AddWithValue("@port", port_TextBox.Text);
                            sqlCommand.Parameters.AddWithValue("@accessKey", accessKey_TextBox.Text);
                            sqlCommand.Parameters.AddWithValue("@username", username_TextBox.Text);
                            sqlCommand.Parameters.AddWithValue("@password", password_PasswordBox.Password);
                            sqlCommand.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "����", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // �򿪻Ự������
            SessionController sessionController = new SessionController();
            sessionController.Show();
            Close();
        }

        // ɾ����¼��Ϣ
        private void DeleteLoginInfo_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("�Ƿ�ɾ����¼��Ϣ��", "����", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    using (SQLiteConnection conn = new SQLiteConnection(@"Data Source = config\client.db"))
                    {
                        conn.Open();
                        string sql = "delete from LoginInfo where id = @id";
                        using (SQLiteCommand sqlCommand = new SQLiteCommand(sql, conn))
                        {
                            sqlCommand.Parameters.AddWithValue("@id", selectedLoginInfo.id);
                            sqlCommand.ExecuteNonQuery();
                        }
                    }
                    GetLoginInfo();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "����", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}