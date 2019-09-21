using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using Remote_Controller_Server.Properties;
using System.Runtime.InteropServices;
using System.Management;

namespace TcpListenerApp
{
    class Program
    {
        const int port = 3389; // порт для прослушивания подключений
        static NotifyIcon icon = new NotifyIcon();

        // Импорт метода из библиотеки для завершения работы
        [DllImport("kernel32.dll")]
        public static extern void ExitProcess([In] uint uExitCode);

        static void Main()
        {
            icon.Icon = Resources.main;
            icon.Visible = true;

            // IP сервера
            IPAddress localAddr = null;

            // сервер (слушатель)
            TcpListener server = null;

            // объект для запуска процессов
            var p = new Process();

            // путь для selfile
            string file = "";

            try
            {
                // и если есть файл - ставим IP из него
                if (File.Exists("ip.ini"))
                    localAddr = IPAddress.Parse(File.ReadAllText("ip.ini"));
                else
                    localAddr = Dns.GetHostByName(Dns.GetHostName()).AddressList[0];
            }
            catch
            {
                // ставим по умолчанию локальный IP
                localAddr = Dns.GetHostByName(Dns.GetHostName()).AddressList[0];
            }

            server = new TcpListener(localAddr, port);
            // запуск слушателя
            server.Start();

            string response; // ответ от сервер

            while (true)
            {
                Console.WriteLine("\nWaiting for connetion... ");

                // получаем входящее подключение
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Client is connected! Query execution...");

                // получаем сетевой поток для чтения и записи
                NetworkStream stream = client.GetStream();
                    
                // принимаем команду
                byte[] data = new byte[10485760];
                // получаем количество считанных байтов
                int bytes = stream.Read(data, 0, data.Length);
                string message = Encoding.UTF8.GetString(data, 0, bytes);

                // выводим ее
                Console.WriteLine("Command: " + message);

                switch (message)
                {
                    case "shutdown":
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = "/c shutdown /s /t 0";
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();

                        // сообщение для отправки клиенту
                        response = "successful!";
                        // преобразуем сообщение в массив байтов
                        data = Encoding.UTF8.GetBytes(response);
                        // отправка сообщения
                        stream.Write(data, 0, data.Length);
                        break;
                    case "restart":
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = "/c shutdown /r /t 0";
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();

                        // сообщение для отправки клиенту
                        response = "successful!";
                        // преобразуем сообщение в массив байтов
                        data = Encoding.UTF8.GetBytes(response);
                        // отправка сообщения
                        stream.Write(data, 0, data.Length);
                        break;
                    case "quit":
                        // сообщение для отправки клиенту
                        response = "Successful!";
                        // преобразуем сообщение в массив байтов
                        data = Encoding.UTF8.GetBytes(response);
                        // отправка сообщения
                        stream.Write(data, 0, data.Length);

                        // закрываем поток
                        stream.Close();
                        // закрываем подключение
                        client.Close();

                        icon.Visible = false;
                        System.Threading.Thread.Sleep(1000);
                        ExitProcess(0);
                        break;
                    case "screen":
                        // Получаем скриншот
                        Console.WriteLine("Getting screenshot...");
                        Bitmap screen = TakeScreenShot(Screen.PrimaryScreen);
                        // создаем поток данных
                        byte[] BmpScreen = CopyImageToByteArray(screen);
                        Console.WriteLine("Creating data stream...");
                        //Отправляем массив байтов с изображением
                        stream.Write(BmpScreen, 0, BmpScreen.Length);
                        Console.WriteLine("Sending...");
                        break;
                    case "gsp":
                        // преобразуем сообщение в массив байтов
                        data = Encoding.UTF8.GetBytes(Application.ExecutablePath);
                        // отправка сообщения
                        stream.Write(data, 0, data.Length);
                        break;
                    case "getproclist":
                        // преобразуем сообщение в массив байтов
                        data = Encoding.UTF8.GetBytes(GetProcessesList());
                        // отправка сообщения
                        stream.Write(data, 0, data.Length);
                        break;
                    case "rfile":
                        try
                        {
                            // преобразуем сообщение в массив байтов
                            data = File.ReadAllBytes(file);
                        }
                        catch (Exception e)
                        {
                            data = (Encoding.UTF8.GetBytes("Error! " + e.Message));
                        }
                        // отправка сообщения
                        stream.Write(data, 0, data.Length);
                        break;
                    case "_TEST_CONNECTING":
                        // преобразуем сообщение в массив байтов
                        data = Encoding.UTF8.GetBytes("TRUE");
                        // отправка сообщения
                        stream.Write(data, 0, data.Length);
                        break;
                    default:
                        // временная переменная для выделения команды из сообщения
                        string command = "";
                        // временная переменная для последующего обрезания смс от команды
                        for (; ; )
                        {
                            if (message[0] == '|')
                            {
                                message = message.Remove(0, 1);
                                break;
                            }
                            command += message[0];
                            message = message.Remove(0, 1);
                        }


                        Console.WriteLine("Message: " + message);

                        if (command == "run")
                            try
                            {
                                Process.Start(message);
                                // сообщение для отправки клиенту
                                response = "successful!";
                                // преобразуем сообщение в массив байтов
                                data = Encoding.UTF8.GetBytes(response);
                                // отправка сообщения
                                stream.Write(data, 0, data.Length);
                            }
                            catch
                            {
                                // сообщение для отправки клиенту
                                response = "not successful!";
                                // преобразуем сообщение в массив байтов
                                data = Encoding.UTF8.GetBytes(response);
                                // отправка сообщения
                                stream.Write(data, 0, data.Length);
                            }

                        if (command == "say")
                        {
                            icon.BalloonTipText = message;
                            icon.ShowBalloonTip(5000);
                            // сообщение для отправки клиенту
                            response = "successful!";
                            // преобразуем сообщение в массив байтов
                            data = Encoding.UTF8.GetBytes(response);
                            // отправка сообщения
                            stream.Write(data, 0, data.Length);
                        }

                        if (command == "message")
                        {
                            MessageBox.Show(message);
                            // сообщение для отправки клиенту
                            response = "successful!";
                            // преобразуем сообщение в массив байтов
                            data = Encoding.UTF8.GetBytes(response);
                            // отправка сообщения
                            stream.Write(data, 0, data.Length);
                        }

                        if (command == "dfile")
                        {
                            if (File.Exists(message))
                            {
                                File.Delete(message);
                                // сообщение для отправки клиенту
                                response = "deleted!";
                            }
                            else if (message == "sf")
                            {
                                File.Delete(file);
                                response = "selected file was deleted!";
                            }
                            else
                                response = "file isn't found!";
                            // преобразуем сообщение в массив байтов
                            data = Encoding.UTF8.GetBytes(response);
                            // отправка сообщения
                            stream.Write(data, 0, data.Length);
                        }

                        if (command == "selfile")
                        {
                            file = message;
                            // сообщение для отправки клиенту
                            response = "successful!";
                            // преобразуем сообщение в массив байтов
                            data = Encoding.UTF8.GetBytes(response);
                            // отправка сообщения
                            stream.Write(data, 0, data.Length);
                        }

                        if (command == "cfile")
                        {
                            // теперь tmp будет хранить путь к файлу, message - текст файла
                            command = "";
                            for (; ; )
                            {
                                if (message[0] == '|')
                                {
                                    message = message.Remove(0, 1);
                                    break;
                                }
                                command += message[0];
                                message = message.Remove(0, 1);
                            }

                            try
                            {
                                File.WriteAllText(command, message);
                                // сообщение для отправки клиенту
                                response = "successful!";
                            }
                            catch(Exception e)
                            {
                                response = "Error! " + e.Message;
                            }
                            // преобразуем сообщение в массив байтов
                            data = Encoding.UTF8.GetBytes(response);
                            // отправка сообщения
                            stream.Write(data, 0, data.Length);
                        }

                        if (command == "wfile")
                        {
                            try
                            {
                                File.AppendAllText(file, message + '\n');
                                // сообщение для отправки клиенту
                                response = "successful!";
                            }
                            catch (Exception e)
                            {
                                response = "Error! " + e.Message;
                            }
                            // преобразуем сообщение в массив байтов
                            data = Encoding.UTF8.GetBytes(response);
                            // отправка сообщения
                            stream.Write(data, 0, data.Length);
                        }

                        if (command == "killproc")
                        {
                            try
                            {
                                Process[] proc = Process.GetProcesses();
                                foreach (Process process in proc)
                                    if (process.ProcessName == message)
                                        process.Kill();
                                // сообщение для отправки клиенту
                                response = "successful!";
                            }
                            catch (Exception e)
                            {
                                response = "Error! " + e.Message;
                            }
                            // преобразуем сообщение в массив байтов
                            data = Encoding.UTF8.GetBytes(response);
                            // отправка сообщения
                            stream.Write(data, 0, data.Length);
                        }

                        break;
                }

                // закрываем поток
                stream.Close();
                // закрываем подключение
                client.Close();
            }
        }

        /// <summary>
        /// Конвертация изображения в массив байтов
        /// </summary>
        /// <param name="theImage">изображение для конвертации</param>
        /// <returns>массив байтов</returns>
        public static byte[] CopyImageToByteArray(Image theImage)
        {
            MemoryStream memoryStream = new MemoryStream();
            {
                theImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Функция захвата скриншота
        /// </summary>
        /// <param name="currentScreen">экран, с которого будет захватываться скриншот</param>
        /// <returns></returns>
        public static Bitmap TakeScreenShot(Screen currentScreen)
        {
            Bitmap bmpScreenShot = new Bitmap(currentScreen.Bounds.Width, currentScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics gScreenShot = Graphics.FromImage(bmpScreenShot);
            gScreenShot.CopyFromScreen(currentScreen.Bounds.X, currentScreen.Bounds.Y, 0, 0, currentScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
            return bmpScreenShot;
        }

        /// <summary>
        /// Получает список запущенных процессов
        /// </summary>
        public static string GetProcessesList()
        {
            string list = "";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "Select Name, CommandLine From Win32_Process");

            foreach (ManagementObject instance in searcher.Get())
                list = list + instance["Name"] + '\n';
            return list;
        }
    }
}