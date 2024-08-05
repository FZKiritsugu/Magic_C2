using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Threading;
using System.Data.SQLite;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

// �����Ự
namespace Client
{
    public partial class PendingSession
    {
        private Thread thread;
        private PendingSessionInfo selectedPendingSessionInfo;
        private ObservableCollection<PendingSessionInfo> pendingSessionInfoList = new ObservableCollection<PendingSessionInfo>();

        public PendingSession()
        {
            InitializeComponent();

            Function.SetTheme(false, pendingSessionInfoList_DataGrid);

            // ���ô��ڹرպ���
            Closing += WindowClosing;

            // ������ȡ�����Ự��Ϣ�б��߳�
            thread = new Thread(() => GetPendingSessionInfoList());
            thread.Start();
        }

        // ��ȡ�����Ự��Ϣ�б�
        private void GetPendingSessionInfoList()
        {
            while (true)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        pendingSessionInfoList.Clear();
                        using (SQLiteConnection conn = new SQLiteConnection(@"Data Source = config\client.db"))
                        {
                            conn.Open();
                            string sql = "select * from PendingSessionInfo order by sid";
                            using (SQLiteCommand sqlCommand = new SQLiteCommand(sql, conn))
                            {
                                using (SQLiteDataReader dataReader = sqlCommand.ExecuteReader())
                                {
                                    while (dataReader.Read())
                                    {
                                        PendingSessionInfo pendingSessionInfo = new PendingSessionInfo()
                                        {
                                            sid = (string)dataReader["sid"],
                                            publicIP = (string)dataReader["publicIP"],
                                            tag = (string)dataReader["tag"],
                                            listenerName = (string)dataReader["listenerName"],
                                            connectTime = (string)dataReader["connectTime"],
                                            heartbeat = Convert.ToInt32(dataReader["heartbeat"]),
                                            currentHeartbeat = Session.CalculateCurrentHeartbeat(Convert.ToInt32(dataReader["heartbeat"])),
                                            determineData = (string)dataReader["determineData"],
                                            pending = (string)dataReader["pending"]
                                        };
                                        pendingSessionInfoList.Add(pendingSessionInfo);
                                    }
                                }
                            }
                        }
                        pendingSessionInfoList_DataGrid.ItemsSource = pendingSessionInfoList;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "����", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }));
                Thread.Sleep(500);
            }
        }

        // ѡ�д����Ự��Ϣ
        private void SelectPendingSessionInfo_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedPendingSessionInfo = (sender as DataGridRow)?.Item as PendingSessionInfo;
        }

        // ��ʾ�ж�����
        private void DisplayDetData_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPendingSessionInfo == null)
            {
                return;
            }
            try
            {
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(selectedPendingSessionInfo.determineData)))
                {
                    using (System.Drawing.Image img = System.Drawing.Image.FromStream(ms))
                    {
                        ms.Position = 0;
                        Window imageWindow = new Window
                        {
                            Title = "�ж�ͼƬ",
                            Width = 1000,
                            Height = 560,
                            WindowStartupLocation = WindowStartupLocation.CenterScreen
                        };
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = ms;
                        bitmapImage.EndInit();
                        Image image = new Image() { Source = bitmapImage, Stretch = System.Windows.Media.Stretch.Fill };
                        imageWindow.Content = image;
                        imageWindow.ShowDialog();
                    }
                }
            }
            catch
            {
                MessageBox.Show(Encoding.GetEncoding("GBK").GetString(Convert.FromBase64String(selectedPendingSessionInfo.determineData)), "�ж�����", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // ���»�ȡ�ж�����
        private void ReacquireDetData_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPendingSessionInfo == null)
            {
                return;
            }
            if (MessageBox.Show("�Ƿ����»�ȡ�ж����ݣ�", "����", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Dictionary<string, string> postParameter = new Dictionary<string, string>
                {
                    { "sid", selectedPendingSessionInfo.sid },
                    { "command", "ReacquireDetData" }
                };
                new HttpRequest("?packageName=ToolBar&structName=PendingSession&funcName=SetPendingSessionCommand", postParameter).GeneralRequest();
            }
        }

        // ������ʽ���߽׶�
        private void StartNextStage_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPendingSessionInfo == null)
            {
                return;
            }
            if (MessageBox.Show("�Ƿ�������", "����", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Dictionary<string, string> postParameter = new Dictionary<string, string>
                {
                    { "sid", selectedPendingSessionInfo.sid },
                    { "command", "StartNextStage" }
                };
                new HttpRequest("?packageName=ToolBar&structName=PendingSession&funcName=SetPendingSessionCommand", postParameter).GeneralRequest();
            }
        }

        // �رս���
        private void CloseProcess_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPendingSessionInfo == null)
            {
                return;
            }
            if (MessageBox.Show("�Ƿ�رս��̣�", "����", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Dictionary<string, string> postParameter = new Dictionary<string, string>
                {
                    { "sid", selectedPendingSessionInfo.sid },
                    { "command", "CloseProcess" }
                };
                new HttpRequest("?packageName=ToolBar&structName=PendingSession&funcName=SetPendingSessionCommand", postParameter).GeneralRequest();
            }
        }

        // ɾ�������Ự��Ϣ
        private void DeletePendingSessionInfo_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPendingSessionInfo == null)
            {
                return;
            }
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(@"Data Source = config\client.db"))
                {
                    conn.Open();
                    string sql = "delete from PendingSessionInfo where sid=@sid";
                    using (SQLiteCommand sqlCommand = new SQLiteCommand(sql, conn))
                    {
                        sqlCommand.Parameters.AddWithValue("@sid", selectedPendingSessionInfo.sid);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
                selectedPendingSessionInfo = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ��Ӵ����Ự��Ϣ
        public static void AddPendingSessionInfo(PendingSessionInfo pendingSessionInfo)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(@"Data Source = config\client.db"))
                {
                    conn.Open();
                    string sql = "replace into PendingSessionInfo (sid, publicIP, tag, listenerName, connectTime, heartbeat, determineData, pending) values (@sid, @publicIP, @tag, @listenerName, @connectTime, @heartbeat, @determineData, @pending)";
                    using (SQLiteCommand sqlCommand = new SQLiteCommand(sql, conn))
                    {
                        sqlCommand.Parameters.AddWithValue("@sid", pendingSessionInfo.sid);
                        sqlCommand.Parameters.AddWithValue("@publicIP", pendingSessionInfo.publicIP);
                        sqlCommand.Parameters.AddWithValue("@tag", pendingSessionInfo.tag);
                        sqlCommand.Parameters.AddWithValue("@listenerName", pendingSessionInfo.listenerName);
                        sqlCommand.Parameters.AddWithValue("@connectTime", pendingSessionInfo.connectTime);
                        sqlCommand.Parameters.AddWithValue("@heartbeat", pendingSessionInfo.heartbeat);
                        sqlCommand.Parameters.AddWithValue("@determineData", pendingSessionInfo.determineData);
                        sqlCommand.Parameters.AddWithValue("@pending", pendingSessionInfo.pending);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show(ex.Message, "����", MessageBoxButton.OK, MessageBoxImage.Error);
                }));
            }
        }

        // ��ֹ��ȡ�����Ự��Ϣ�б��߳�
        private void WindowClosing(object sender, CancelEventArgs e)
        {
            thread.Abort();
        }
    }
}