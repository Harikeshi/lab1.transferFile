using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Threading;

public class UdpFileServer
{
    // Информация о файле (требуется для получателя)
    [Serializable]
    public class FileDetails
    {
        public string FILETYPE = "";
        public long FILESIZE = 0;
    }

    private static FileDetails fileDet = new FileDetails();

    // Поля, связанные с UdpClient
    private static IPAddress remoteIPAddress;
    private const int remotePort = 5002;
    private static UdpClient sender = new UdpClient();
    private static IPEndPoint endPoint;

    // Filestream object
    private static FileStream fs;
    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            // Получаем удаленный IP-адрес и создаем IPEndPoint
            remoteIPAddress = IPAddress.Parse("127.0.0.1");
            //Console.WriteLine("Server Started ... ");
            endPoint = new IPEndPoint(remoteIPAddress, remotePort);

           
            Console.Write("Enter File name:");
            int leng = 0;


           
                fs = new FileStream(@Console.ReadLine().ToString(), FileMode.Open, FileAccess.Read);
            
            Console.Write("Enter max size:");
            leng = Convert.ToInt32(Console.Read());

            if (fs.Length > leng*10000)
            {
                Console.WriteLine("Biggest file!!");
                sender.Close();
                fs.Close();
                return;
            }
           

                // Отправляем информацию о файле
                SendFileInfo();

                // Ждем 2 секунды
                Thread.Sleep(2000);

                // Отправляем сам файл
                SendFile();
            
            Console.ReadLine();

        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
    }
    public static void SendFileInfo()
    {

        // Получаем тип и расширение файла
        fileDet.FILETYPE = fs.Name.Substring((int)fs.Name.Length - 3, 3);

        // Получаем длину файла
        fileDet.FILESIZE = fs.Length;

        XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
        MemoryStream stream = new MemoryStream();

        // Сериализуем объект
        fileSerializer.Serialize(stream, fileDet);

        // Считываем поток в байты
        stream.Position = 0;
        Byte[] bytes = new Byte[stream.Length];
        stream.Read(bytes, 0, Convert.ToInt32(stream.Length));
        

        Console.WriteLine("Sending File...");

        // Отправляем информацию о файле
        sender.Send(bytes, bytes.Length, endPoint);
        stream.Close();

    }
    private static void SendFile()
    {
        // Создаем файловый поток и переводим его в байты
        Byte[] bytes = new Byte[fs.Length];
        fs.Read(bytes, 0, bytes.Length);

        try
        {
            // Отправляем файл
            sender.Send(bytes, bytes.Length, endPoint);
        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
        finally
        {
            // Закрываем соединение и очищаем поток
            fs.Close();
            sender.Close();
        }
        Console.WriteLine("File Sent.");
        Console.Read();
    }
}