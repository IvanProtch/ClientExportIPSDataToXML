using Intermech.Interfaces;
using Intermech.Interfaces.Compositions;
using Intermech.Interfaces.XmlExchange;
using Intermech.Kernel.Search;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting;
using System.Xml.Linq;

namespace IPStoXmlDataExport
{
    /// <summary>
    /// Загружает данные из ips
    /// </summary>
    internal class IPSDataReader
    {
        public enum ReadingMode
        {
            readObjectAsDataRow,
            readObjectAsID
        }

        private static IPSDataReader _dataReader;

        private XmlExchangeExportSettings _settings;
        private IUserSession _session;
        private List<int> _objTypes;
        private List<int> _attributs;
        private List<int> _relations;
        private ReadingMode _mode;

        private IPSDataReader(IUserSession session, XmlExchangeExportSettings settings, ReadingMode mode)
        {
            _mode = mode;
            this._settings = settings;
            this._session = session;

            if (_settings != null)
            {
                _objTypes = _settings.ObjSettings.Select(objSetting => session.GetObjectType((objSetting as XmlExchangeExportTypedBase).TypeGuid).ObjectType).ToList();
                _relations = _settings.RelSettings.Select(relSettings => session.GetRelationType((relSettings as XmlExchangeExportTypedBase).TypeGuid).RelationType).ToList();
                _attributs = _settings.AttrSettings.Select(attrSettings => session.GetAttributeType((attrSettings as XmlExchangeExportTypedBase).TypeGuid).AttributeID).ToList();
            }
            else
            {
                // все типы, простая связь и только основные атрибуты
                _objTypes = null;
                _relations = new List<int>() { 1, 1002, 1011, 1004, 1007, 1060 };
                _attributs = new List<int>();
            }
        }

        public static IPSDataReader DataReader(IUserSession session, XmlExchangeExportSettings settings, ReadingMode mode = ReadingMode.readObjectAsID)
        {
            if (_dataReader == null)
                return new IPSDataReader(session, settings, mode);
            return _dataReader;
        }

        /// <summary>
        /// Уровень проработки состава (-1 - полный состав)
        /// </summary>
        private const int ConsistanceLevel = 1;

        /// <summary>
        /// Загрузить данные в ExportData
        /// </summary>
        public ExportData Read(IUserSession session, ExportedObject headerObj)
        {
            this._session = session;

            ExportData export = new ExportData(headerObj);

            if (!headerObj.allowToLoadConsistance)
                return export;

            export.ExportedObjectsList.Add(new ExportedObject(export.HeaderObject.ObjectID, _settings, _session));

            switch (_mode)
            {
                case ReadingMode.readObjectAsDataRow:
                    {
                        export.ExportedObjectsList = GetObjectConsistanceDataTable(_session, export.HeaderObject.ObjectID, _relations, _objTypes, ConsistanceLevel)
                            .Rows.OfType<DataRow>()
                            .Select(row => new ExportedObject(row))
                            .ToList();

                        break;
                    }

                case ReadingMode.readObjectAsID:
                    {
                        export.ExportedObjectsList = GetObjectConsistance(session, export.HeaderObject.ObjectID, _relations, _objTypes, ConsistanceLevel)
                            .Select(item => new ExportedObject(item, _settings, _session))
                            .ToList();

                        break;
                    }
            }

            return export;
        }

        /// <summary>
        /// Получение списка ID состава/применяемости
        /// </summary>
        /// <param name="session"></param>
        /// <param name="objID"></param>
        /// <param name="relIDs"></param>
        /// <param name="childObjIDs"></param>
        /// <param name="lv">Количество уровней для обработки (-1 = загрузка полного состава / применяемости)</param>
        /// <returns></returns>
        private List<long> GetObjectConsistance(IUserSession session, long objID, List<int> relIDs, List<int> childObjIDs, int lv)
        {
            ICompositionLoadService _loadService = session.GetCustomService(typeof(ICompositionLoadService)) as ICompositionLoadService;
            List<ColumnDescriptor> col = new List<ColumnDescriptor>
            {
                new ColumnDescriptor(ObligatoryObjectAttributes.F_OBJECT_ID, AttributeSourceTypes.Object, ColumnContents.ID, ColumnNameMapping.Index, SortOrders.NONE, 0),
            };
            List<long> reslt = new List<long>();
            session.EditingContextID = objID;
            DataTable dt = _loadService.LoadComplexCompositions(
            session.SessionGUID,
            new List<ObjInfoItem> { new ObjInfoItem(objID) },
            relIDs,
            childObjIDs,
            col,
            true, // Режим получения данных (true - состав, false - применяемость)
            false, // Флаг группировки данных
            null, // Правило подбора версий
            null, // Условия на объекты при получении состава / применяемости
                  //SystemGUIDs.filtrationLatestVersions, // Настройки фильтрации объектов (последние)
                  //SystemGUIDs.filtrationBaseVersions, // Настройки фильтрации объектов (базовые)
            Intermech.SystemGUIDs.filtrationLatestVersions,
            null,
            lv // Количество уровней для обработки (-1 = загрузка полного состава / применяемости)
            );
            if (dt == null)
                return reslt;
            else
                return dt.Rows.OfType<DataRow>()
            .Select(element => (long)element[0]).ToList<long>();
        }

        /// <summary>
        /// Получение таблицы состава
        /// </summary>
        /// <param name="session"></param>
        /// <param name="objID"></param>
        /// <param name="relIDs"></param>
        /// <param name="childObjIDs"></param>
        /// <param name="lv"></param>
        /// <returns></returns>
        private DataTable GetObjectConsistanceDataTable(IUserSession session, long objID, List<int> relIDs, List<int> childObjIDs, int lv)
        {
            ICompositionLoadService _loadService = session.GetCustomService(typeof(ICompositionLoadService)) as ICompositionLoadService;

            // добавление атрибутов из конфига
            List<ColumnDescriptor> columns = _attributs.Select(attr => new ColumnDescriptor(attr)).ToList();

            // добавление обязательных атрибутов
            List<int> obligAttrs = new List<int>()
            {
                (int) ObligatoryObjectAttributes.F_OBJECT_ID, (int) ObligatoryObjectAttributes.CAPTION, (int) ObligatoryObjectAttributes.F_OBJECT_TYPE, (int) ObligatoryObjectAttributes.F_GUID
            };
            foreach (int attr in obligAttrs)
            {
                if (!_attributs.Contains(attr))
                    columns.Add(new ColumnDescriptor(attr));
            }

            session.EditingContextID = objID;
            DataTable dt = _loadService.LoadComplexCompositions(
            session.SessionGUID,
            new List<ObjInfoItem> { new ObjInfoItem(objID) },
            relIDs,
            childObjIDs,
            columns,
            true, // Режим получения данных (true - состав, false - применяемость)
            false, // Флаг группировки данных
            null, // Правило подбора версий
            null, // Условия на объекты при получении состава / применяемости
                  //SystemGUIDs.filtrationLatestVersions, // Настройки фильтрации объектов (последние)
                  //SystemGUIDs.filtrationBaseVersions, // Настройки фильтрации объектов (базовые)
            Intermech.SystemGUIDs.filtrationLatestVersions,
            null,
            lv // Количество уровней для обработки (-1 = загрузка полного состава / применяемости)
            );

            if (dt == null)
                return new DataTable();
            return dt;
        }
    }

    /// <summary>
    /// Записывает данные в xml
    /// </summary>
    public class XMLDataWriter
    {
        private static XMLDataWriter _dataWriter;
        private string _filePath;

        public XDocument Document { get; set; }

        private XMLDataWriter(string filePath)
        {
            _filePath = filePath;
            Document = new XDocument();
        }

        public static XMLDataWriter DataWriter(string filePath)
        {
            if (_dataWriter == null)
            {
                XMLDataWriter dataWriter = new XMLDataWriter(filePath);
                return dataWriter;
            }
            return _dataWriter;
        }

        /// <summary>
        /// Ассинхронная запись данных в xml документ
        /// </summary>
        /// <param name="data"></param>
        public void Write(ExportData data)
        {
            object locker = new object();

            if (!Document.Elements().Any())
                AddFirst(data.HeaderObject);

            XElement subEl = FindSubElement(Document.Elements().ToList(), data);
            foreach (var item in data.ExportedObjectsList)
                AppendObjectToXElement(subEl, item);

            // если найдем добавим всех потомков
            lock (locker)
            {
                Document.Save(_filePath);
            }
        }

        private XElement FindSubElement(IEnumerable<XElement> elems, ExportData data)
        {
            XElement subElem = null;
            if (elems.Any())
            {
                subElem = elems
                    .Where(elem => elem.Attributes().Count() > 0)
                    .Where(elem => elem.Attribute("id") != null)
                    .Where(elem => elem.Elements().Count() == 1) //записаны только атрибуты
                    .FirstOrDefault(elem => Convert.ToInt64(elem.Attribute("id").Value) == data.HeaderObject.ObjectID);
            }
            if (subElem != null)
                return subElem;
            else if (elems.Count() > 0)
                return FindSubElement(elems.Elements(), data);
            else
                return null;
        }

        private void AddFirst(ExportedObject header)
        {
            XStreamingElement mainElement = new XStreamingElement("main_item");
            mainElement.Add(new XAttribute("name", header.caprion));
            mainElement.Add(new XAttribute("type", header.ObjectType));
            mainElement.Add(new XAttribute("id", header.ObjectID));
            mainElement.Add(new XAttribute("guid", header.Guid));

            //атрибуты
            mainElement.Add(new XElement("attributs",
                from attribute in header.exportedAttributes
                select new XStreamingElement("attribute", attribute.Name,
                //свойства атрибута
                new XAttribute("name", attribute.Name),
                new XAttribute("type", attribute.AttributeId),
                new XAttribute("guid", attribute.Guid),
                //значения атрибута
                new XStreamingElement("attribute_values",
                    from value in attribute.Values
                    select new XStreamingElement("value", value)))
                ));

            Document.Add(mainElement);
        }

        private XElement AppendObjectToXElement(XElement subElem, ExportedObject expObj)
        {
            //объект
            XElement newElem = new XElement("item");

            //свойства объекта
            newElem.Add(new XAttribute("name", expObj.caprion));
            newElem.Add(new XAttribute("type", expObj.ObjectType));
            newElem.Add(new XAttribute("id", expObj.ObjectID));
            newElem.Add(new XAttribute("guid", expObj.Guid));

            //атрибуты
            newElem.Add
                (new XElement
                    ("attributs",
                        from attribute in expObj.exportedAttributes
                        select new XStreamingElement("attribute", attribute.Name,
                        //свойства атрибута
                        new XAttribute("name", attribute.Name),
                        new XAttribute("type", attribute.AttributeId),
                        new XAttribute("guid", attribute.Guid),
                        //значения атрибута
                        new XStreamingElement("attribute_values",
                            from value in attribute.Values
                            select new XStreamingElement("value", value)))
                ));

            subElem.Add(newElem);
            return newElem;
        }

        public void Write(ExportedAsm mainAsm)
        {
            object locker = new object();

            if (!Document.Elements().Any())
                AddFirst(mainAsm);

            Writing(Document.Root, mainAsm);

            // если найдем добавим всех потомков
            lock (locker)
            {
                Document.Save(_filePath);
            }
        }

        /// <summary>
        /// Рекурсивное добавление новых XElement
        /// </summary>
        /// <param name="xElem"></param>
        /// <param name="asm"></param>
        private void Writing(XElement xElem, ExportedAsm asm)
        {
            foreach (ExportedObject item in asm.ChildObjects.Cast<ExportedObject>().ToList())
            {
                XElement newElem = AppendObjectToXElement(xElem, item);
                Writing(newElem, item as ExportedAsm);
            }
        }
    }
}