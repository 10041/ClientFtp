using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Data;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace clientFTP
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        ObservableCollection<FileInfo> cont = new ObservableCollection<FileInfo>();
        Stack<string> directory = new Stack<string>();
        FTP ftp = new FTP();
        string ip = "ftp://127.0.0.1/";
        string path = "";
        string NoDbName = "admin";
        string NoDbPass = "admin";

        string StatusText
        {
            set
            {
                Action<string> setStatusText = s => mess.Text = s;
                mess.Dispatcher.Invoke(setStatusText, value);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            fileInfo.ItemsSource = cont;
            directory.Push("");
            InputDomen.Text = ip;
            using (LAP lap = new LAP())
            {
                foreach(var item in lap.LogAndPass)
                {
                    ftp.setUserAndPass(item.Login, item.Pass);
                }
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Application app = Application.Current;
            app.Shutdown();
        }

        private void ButtonMinized_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ButtonMaximazen_Click(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void Title_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mess.Text = string.Empty;
                if (InLogin.Text == string.Empty || InPass.Password == string.Empty)
                {
                    mess.Text = "Введите логин и пароль.";
                    return;
                }
                bool enter = false;
                if(InLogin.Text == NoDbName && InPass.Password == NoDbPass)
                {
                    enter = true;
                    InLogin.Text = string.Empty;
                }
                else
                {
                    using (DUser db = new DUser())
                    {
                        foreach (Users item in db.Users)
                        {
                            if (item.name == InLogin.Text && item.pass == InPass.Password)
                            {
                                enter = true;
                            }
                        }
                    }
                }
                if (enter)
                {
                    path += InLogin.Text + "/";
                    ftp.GetDirectory(ip, path, ref cont);
                    UserName.Content = InLogin.Text;
                    InLogin.Text = string.Empty;
                    InPass.Password = string.Empty;
                    this.LogOut.Visibility = Visibility.Visible;
                    StartWindow.Visibility = Visibility.Collapsed;
                    UserName.Visibility = Visibility.Visible;
                    NavigationPanel.Visibility = Visibility.Visible;
                    fileInfo.Visibility = Visibility.Visible;
                    ImageLeftCorner.Visibility = Visibility.Visible;
                }
                else
                    mess.Text = "Пользователь не найден";
            }
            catch(Exception q)
            {
                InLogin.Text = string.Empty;
                InPass.Password = string.Empty;
                MessageBox.Show(q.Message);
            }
        }

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            UserName.Visibility = Visibility.Collapsed;
            StartWindow.Visibility = Visibility.Visible;
            LogOut.Visibility = Visibility.Collapsed;
            NavigationPanel.Visibility = Visibility.Collapsed;
            fileInfo.Visibility = Visibility.Collapsed;
            ImageLeftCorner.Visibility = Visibility.Collapsed;
            mess.Text = "";
            path = "";
        }

        private void Registration_Click(object sender, RoutedEventArgs e)
        {
            StartWindow.Visibility = Visibility.Collapsed;
            RegistrationWindow.Visibility = Visibility.Visible;
            mess.Text = string.Empty;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mess.Text = string.Empty;
                if (fileInfo.SelectedItem != null)
                {
                    FileInfo file = new FileInfo();
                    file = fileInfo.SelectedItem as FileInfo;
                    if (file.type == "DIR")
                    {
                        path += file.name + "/";
                        directory.Push(file.name);
                        ftp.GetDirectory(ip, path, ref cont);
                    }
                    else
                    {
                        mess.Text = "Скачивание файла...";
                        mess.Text = ftp.Download(ip, path, file.name);
                        Process.Start(file.name);
                    }
                }
            }
            catch (Exception q)
            {
                MessageBox.Show(q.Message);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            mess.Text = string.Empty;
            if (fileInfo.SelectedItem != null)
            {
                FileInfo file = new FileInfo();
                file = fileInfo.SelectedItem as FileInfo;
                try
                {
                    if(file.type == "DIR")
                    {
                        mess.Text = ftp.DeleteDir(ip, path, file.name);
                        ftp.GetDirectory(ip, path, ref cont);
                    }
                    else
                    {
                        mess.Text = ftp.DeleteFile(ip, path, file.name);
                        ftp.GetDirectory(ip, path, ref cont);
                    }
                }
                catch (Exception q)
                {
                    MessageBox.Show(q.Message);
                }
                
            }
        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            mess.Text = string.Empty;
            try
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.ShowDialog();
                if (fileDialog.FileName != string.Empty)
                {
                    if (ftp.getSizeFileInMB(fileDialog.FileName) > 512)
                    {
                        MessageBox.Show("Файл не должен быть больше 512 МБ.", "Ошибка");
                        return;
                    }
                    ftp.GetDirectory(ip, path, ref cont);
                    foreach(var item in cont)
                    {
                        if(item.name.Equals(fileDialog.FileName.Substring(fileDialog.FileName.LastIndexOf("\\") + 1)))
                        {
                            MessageBox.Show("Такой файл уже существует на сервере", "Ошибка");
                            return;
                        }
                    }
                    StatusText = "Загрузка...";
                    mess.Text = ftp.UploadFile(ip, path, fileDialog.FileName.Substring(fileDialog.FileName.LastIndexOf("\\") + 1), fileDialog.FileName);
                    GC.Collect();
                    ftp.GetDirectory(ip, path, ref cont);
                }
            }
            catch (Exception q)
            {
                MessageBox.Show(q.Message);
            }     
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mess.Text = string.Empty;
                ftp.GetDirectory(ip, path, ref cont);
            }
            catch (Exception q)
            {
                MessageBox.Show(q.Message);
            }
        }

        private void fileInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                mess.Text = string.Empty;
                if (fileInfo.SelectedItem != null)
                {
                    FileInfo file = new FileInfo();
                    file = fileInfo.SelectedItem as FileInfo;
                    if (file.type == "DIR")
                    {
                        path += file.name + "/";
                        directory.Push(file.name);
                        ftp.GetDirectory(ip, path, ref cont);
                    }
                    else
                    {
                        mess.Text = "Скачивание файла...";
                        mess.Text = ftp.Download(ip, path, file.name);
                        Process.Start(file.name);
                    }
                }
            }
            catch (Exception q)
            {
                MessageBox.Show(q.Message);
            }
            
        }

        private void ToRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Login.Text != String.Empty && Pass1.Password != String.Empty && Pass2.Password != String.Empty)
                {
                    if (Pass1.Password == Pass2.Password)
                    {
                        using (DUser db = new DUser())
                        {
                            var users = db.Users;
                            if (users.Count() > 0)
                            {
                                foreach (Users item in users)
                                {
                                    if (item.name == Login.Text)
                                    {
                                        MessageBox.Show("Пользователь с таким именем уже существует.", "Ошибка");
                                        return;
                                    }
                                }
                            }
                            db.Users.Add(new Users { name = Login.Text, pass = Pass1.Password });
                            
                            db.SaveChanges();
                        }
                        ftp.CreateDir(ip, "", Login.Text);
                        MessageBox.Show("Добро пожаловать " + Login.Text);
                        Login.Text = string.Empty;
                        Pass1.Password = string.Empty;
                        Pass2.Password = string.Empty;
                        StartWindow.Visibility = Visibility.Visible;
                        RegistrationWindow.Visibility = Visibility.Collapsed;
                    }
                    else
                        mess.Text = "Пароли должны совпадать!";
                }
                else
                    mess.Text = "Все поля должны быть заполнены!";
            }
            catch(Exception q)
            {
                MessageBox.Show(q.Message);
            }
            
        }

        private void CancelRegister_Click(object sender, RoutedEventArgs e)
        {
            StartWindow.Visibility = Visibility.Visible;
            RegistrationWindow.Visibility = Visibility.Collapsed;
            mess.Text = string.Empty;
            Login.Text = string.Empty;
            Pass1.Password = string.Empty;
            Pass2.Password = string.Empty;
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mess.Text = string.Empty;
                if (fileInfo.SelectedItem != null)
                {
                    FileInfo file = new FileInfo();
                    file = fileInfo.SelectedItem as FileInfo;
                    mess.Text = ftp.Download(ip, path, file.name);
                }
            }
            catch(Exception q)
            {
                MessageBox.Show(q.Message);
            }
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            mess.Text = string.Empty;
            StartWindow.Visibility = Visibility.Collapsed;
            SettingWindow.Visibility = Visibility.Visible;
        }

        private void OkSetting_Click(object sender, RoutedEventArgs e)
        {
            mess.Text = string.Empty;
            if (InputDomen.Text != string.Empty)
            {
                if (InputDomen.Text.Substring(0, 6) == "ftp://" && InputDomen.Text.Substring(InputDomen.Text.Length - 1) == "/")
                {
                    string temp = string.Empty;
                    for (int i = 6; i < InputDomen.Text.Length; i++)
                    {
                        if (InputDomen.Text[i] == '/')
                            break;
                        temp += InputDomen.Text[i];
                    }
                    if (ftp.IsAddressValid(temp))
                    {
                        ip = InputDomen.Text;
                        SettingWindow.Visibility = Visibility.Collapsed;
                        StartWindow.Visibility = Visibility.Visible;
                    }
                    else
                        mess.Text = "неверно задан IP";
                }
                else
                    mess.Text = "Неправильная структура домена, должен начинаться с ftp:// и заканчиваться /";
            }
            else
                mess.Text = "Укажите домен или нажмите отмена";
        }

        private void CanselSetting_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow.Visibility = Visibility.Collapsed;
            StartWindow.Visibility = Visibility.Visible;
            mess.Text = string.Empty;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            mess.Text = string.Empty;
            if (directory.Peek() != string.Empty)
            {
                path = path.Remove(path.Length - directory.Pop().Length - 1);
                ftp.GetDirectory(ip, path, ref cont);
            }
        }

        private void fileInfo_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop, true) == true)
            {
                e.Effects = DragDropEffects.All;
            }
            e.Handled = true;
        }

        private void fileInfo_PreviewDrop(object sender, DragEventArgs e)
        {
            try
            {
                string[] filename = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                string temp = string.Empty;
                foreach (var item in filename)
                {
                    temp += item;
                }
                if (ftp.getSizeFileInMB(temp) > 512)
                {
                    MessageBox.Show("Файл не должен быть больше 512 МБ.", "Ошибка");
                    return;
                }
                ftp.GetDirectory(ip, path, ref cont);
                foreach (var item in cont)
                {
                    if (item.name.Equals(temp.Substring(temp.LastIndexOf("\\") + 1)))
                    {
                        MessageBox.Show("Такой файл уже существует на сервере", "Ошибка");
                        return;
                    }
                }
                mess.Text = ftp.UploadFile(ip, path, temp.Substring(temp.LastIndexOf("\\") + 1), temp);
                GC.Collect();
                ftp.GetDirectory(ip, path, ref cont);
                e.Handled = true;
            }
            catch(Exception q)
            {
                MessageBox.Show(q.Message);
            }
        }
    }
}
