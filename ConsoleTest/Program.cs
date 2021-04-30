using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using IPStoXmlDataExport;
using Intermech.Interfaces;
using Intermech.Interfaces.XmlExchange;

namespace ConsoleTest
{
    public class IPSConnector
    {
        private static IUserSession _session;

        private static IUserSession Connect(string loginName, string password, bool isMainBase = false)
        {
            IUserSession UserSession = null;
            try
            {
                // Инициализируем канал связи с сервером приложений по настройкам в конфигурационном файле
                RemotingConfiguration.Configure(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, true);
                //тестовая
                string serverURL = "tcp://SAPRTEST-SRV.ad.bmz:8008/IntermechRemoting/Server.rem";

                if (isMainBase)
                {
                    //основная
                    serverURL = "tcp://TE-SRV010:8008/IntermechRemoting/Server.rem";
                }
                //string serverURL = RemotingConfiguration.GetRegisteredWellKnownClientTypes()[0].ObjectUrl;

                IMServer server = (IMServer)Activator.GetObject(typeof(IMServer), serverURL);
                UserSession = server.CreateSession();
                // Получаем смещение для текущего часового пояса и вызываем у сессии функцию авторизации
                DateTime now = DateTime.Now;
                TimeSpan ts = now - now.ToUniversalTime();
                UserSession.Login(loginName, new Intermech.Protection.PswPackage(password, 'a'), Environment.MachineName, ts, 0);
                Console.WriteLine("Connect to " + serverURL);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return UserSession;
        }

        public static IUserSession Session
        {
            get
            {
                if (_session == null)
                {
                    _session = Connect("protchenkoIV", "Byajhvfwbz28");
                    return _session;
                }

                return _session;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            IUserSession session = IPSConnector.Session;

            string path = @"D:\MyDoc\protchenkoiv\Рабочий стол\!XML\новая выгрузка\result.xml";
            XmlExchangeExportSettings exportSett = new XmlExchangeExportSettings();

            XmlExchangeExportHelper.LoadSettings(79869831, session, out exportSett);// 78914316
            //   66824300  37688804 42292460(тепловоз)
            //ExportTask exportTask = new ExportTask(66824300, path);

            ExportTask exportTask = new ExportTask(session, 66824300, path, exportSett);

            ExportedAsm asm = new ExportedAsm(66824300, exportSett, session);

            XMLDataWriter.DataWriter(path).Write(asm);

            //asm.GetAllConsistance();
            ;
            exportTask
                .AddObject(ExportedObject.Create(1001, "test", 12345), 66824300)
                .Exclude(el => el.ObjectType == 1317)
                .Single(el => el.ObjectType == 1052)
                .Single(el => el.ObjectType == 1176)
                .DoAction(obj => obj.ObjectType == 1001, obj => obj.typeName = "newType")
                .CreateOrChangeAttribute(new Tuple<int, int>(1075, 0), new Tuple<int, int>(1110, 1125))

                .StartExportTask(writeExceptionToLog: false);
        }
    }
}
