using Intermech.Interfaces;
using Intermech.Interfaces.XmlExchange;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IPStoXmlDataExport
{
    public class Logger : ILogger
    {
        private string _filePath;
        private static Logger _logger;
        private int _counter = 0;

        private Logger(string filePath)
        {
            _filePath = filePath;
            this.AddToLog("\t\t\tЗапись лога начата:");
        }

        public static Logger GetLogger(string dir = @"D:\ClientExportIPSDataToXML")
        {
            if (_logger != null)
                return _logger;
            else
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (!File.Exists(dir + "\\" + "log.txt"))
                    File.Create(dir + "\\" + "log.txt");

                _logger = dir is null ? null : new Logger(dir + "\\" + "log.txt");
                return _logger;
            }
        }

        private void WriteLine(string text)
        {
            _counter++;
            tryAppend:
            try
            {
                System.Threading.Thread.Sleep(10);
                File.AppendAllText(_filePath, ($"\r\nN{_counter} [{DateTime.Now}] - {text}"));
            }
            catch (Exception)
            {
                goto tryAppend;
            }
        }

        public void AddToLog(params string[] text)
        {
            foreach (string line in text)
            {
                WriteLine(line);
            }
        }

        public void LogException(Exception e)
        {
            WriteLine($"Ошибка: {e.Message} \r\nСтек вызовов:\r\n {e.StackTrace}");
        }
    }

    /// <summary>
    /// Задача экспорта. Содержит дополнительные методы по добавлению условий фильтрафии экспортируемых данных.
    /// </summary>
    public class ExportTask
    {
        private IUserSession _session;
        private Logger _logger;
        private ExportedObject _mainAsm;
        private XMLDataWriter _dataWriter;
        private IPSDataReader _dataReader;
        private class ObjAttrPairsAndClipboard
        {
            internal ObjAttrPairsAndClipboard(Tuple<int, int> objWithValue, Tuple<int, int> objWithAttr)
            {
                this.objWithAttr = objWithAttr;
                this.objWithValue = objWithValue;
            }

            /// <summary>
            /// Объект-источник данных. 1 - тип объекта, 2 - атрибут
            /// </summary>
            internal Tuple<int, int> objWithValue;

            /// <summary>
            /// Объект в который записывают данные. 1 - тип объекта, 2 - атрибут
            /// </summary>
            internal Tuple<int, int> objWithAttr;

            internal ExportedAttribute sourseAttr;
        }

        #region Записанные условия экспорта
        /// <summary>
        /// key - родитель, value - новый объект
        /// </summary>
        private Dictionary<long, ExportedObject> _objectsToAdd = new Dictionary<long, ExportedObject>();

        /// <summary>
        /// Объекты, которые следует грузить без состава
        /// </summary>
        private List<long> _singleObjects = new List<long>();

        /// <summary>
        /// Типы объектов, которые следует грузить без состава
        /// </summary>
        private List<int> _singleObjectsType = new List<int>();

        /// <summary>
        /// Объекты, которые не нужно грузить
        /// </summary>
        private List<long> _objectsToRemove = new List<long>();

        /// <summary>
        /// Типы объектов, которые не нужно грузить
        /// </summary>
        private List<int> _objectsTypesToRemove = new List<int>();

        private List<Predicate<ExportedObject>> _excludeConditions = new List<Predicate<ExportedObject>>();
        private List<Predicate<ExportedObject>> _singleConditions = new List<Predicate<ExportedObject>>();

        /// <summary>
        ///  Словарь содержит условие и действие ему соответствующее
        /// </summary>
        private Dictionary<Predicate<ExportedObject>, Action<ExportedObject>> _changeObjectConditionsActions = new Dictionary<Predicate<ExportedObject>, Action<ExportedObject>>();

        /// <summary>
        /// Атрибуты объектов определенного типа (key) в значения которых записывается значение других (value)
        /// </summary>
        private List<ObjAttrPairsAndClipboard> _copyAttrValueToAnotherAttr_forType = new List<ObjAttrPairsAndClipboard>();
        #endregion

        /// <summary>
        /// Объект конфигурации экспорта Intermech.Interfaces.XmlExchange
        /// </summary>
        public XmlExchangeExportSettings Settings { get; set; }

        /// <summary>
        /// Путь к файлу лога
        /// </summary>
        public string LoggerFilePath
        {
            set
            {
                _logger = Logger.GetLogger(value);
            }
        }

        /// <summary>
        /// Создает экземпляр задачи экспорта. Без settings выведет все объекты.
        /// </summary>
        /// <param name="headerObject">Экспортируемый объект</param>
        /// <param name="settings">Настройки из ips</param>
        /// <param name="filePath">Путь к файлу</param>
        /// <param name="writeCompleteAttributeData">true - выводит все свойства атрибутов, false - только наименование и значение, но зато быстро (получает данные через datatable)</param>
        public ExportTask(IUserSession session, long headerObject, string filePath, XmlExchangeExportSettings settings = null, bool writeCompleteAttributeData = true)
        {
            try
            {
                this._session = session;
                this.Settings = settings;
                if (_logger is null)
                    _logger = Logger.GetLogger();
                _mainAsm = new ExportedObject(headerObject, Settings, _session);
                //Data = new List<ExportData>();
                ExportData header = new ExportData(_mainAsm);
                header.HeaderObject = _mainAsm;
                _dataWriter = XMLDataWriter.DataWriter(filePath);
                _dataReader = IPSDataReader.DataReader(_session, Settings, writeCompleteAttributeData ? IPSDataReader.ReadingMode.readObjectAsID : IPSDataReader.ReadingMode.readObjectAsDataRow);
            }
            catch (Exception exc)
            {
                _logger.LogException(exc);
            }
        }

        /// <summary>
        /// Начать выгрузку
        /// </summary>
        public void StartExportTask(bool writeExceptionToLog = true, bool readAndWriteInOneProcess = true)
        {
            DateTime start = DateTime.Now;
            _logger.AddToLog($"Экспорт данных [{_mainAsm}] начат.");
            if (readAndWriteInOneProcess)
                Processing(_mainAsm);
            DateTime end = DateTime.Now;
            _logger.AddToLog($"Экспорт данных [{_mainAsm}] завершен (время выполнения: {(end - start).ToString("hh\\:mm\\:ss")}).");
            try
            {

            }
            catch (Exception exc)
            {
                if (writeExceptionToLog)
                    _logger.LogException(exc);
                else
                    throw exc;
            }
        }

        private void Processing(ExportedAsm asm)
        {

        }

        /// <summary>
        /// Запись полного состава изделия
        /// </summary>
        /// <param name="obj"></param>
        private void Processing(ExportedObject obj)
        {
            ExportData data = _dataReader.Read(_session, obj);

            bool isSingle = _singleObjects.Contains(obj.ObjectID) || _singleObjectsType.Contains(obj.ObjectType);
            if (!isSingle)
            {
                foreach (Predicate<ExportedObject> condition in _singleConditions)
                {
                    if (condition.Invoke(obj))
                    {
                        isSingle = true;
                        break;
                    }
                }
            }

            if (!isSingle)
            {
                if (_objectsToAdd.ContainsKey(obj.ObjectID))
                {
                    data.ExportedObjectsList.Add(_objectsToAdd[obj.ObjectID]);
                    _objectsToAdd.Remove(obj.ObjectID);
                }

                data.ExportedObjectsList
                    .RemoveAll(item => _objectsToRemove.Contains(item.ObjectID) || _objectsTypesToRemove.Contains(item.ObjectType));

                _excludeConditions.ForEach(cond => data.ExportedObjectsList.RemoveAll(e => cond.Invoke(e)));

                foreach (ExportedObject objExp in data.ExportedObjectsList)
                {
                    foreach (Predicate<ExportedObject> predicate in _changeObjectConditionsActions.Keys)
                    {
                        if (predicate.Invoke(objExp))
                        {
                            _changeObjectConditionsActions[predicate].Invoke(objExp);
                            _logger.AddToLog($"Для [{objExp}] выполнено условие [{predicate.Method.Name}] и действие [{_changeObjectConditionsActions[predicate].Method.Name}]");
                        }
                    }

                    // копирование значения атрибута
                    foreach (var objAttr in _copyAttrValueToAnotherAttr_forType)
                    {
                        // если найден объект-источник
                        if (objAttr.objWithValue.Item1 == objExp.ObjectType)
                        {
                            ExportedAttribute sourseAttr = objExp.exportedAttributes.FirstOrDefault(attr => attr.AttributeId == objAttr.objWithValue.Item2);
                            objAttr.sourseAttr = sourseAttr is null ? null : sourseAttr.Clone() as ExportedAttribute; // запись атрибута в буфер
                        }

                        // если найден объект-приемник
                        if (objAttr.objWithAttr.Item1 == objExp.ObjectType)
                        {
                            var attribute = objExp.exportedAttributes.FirstOrDefault(attr => attr.AttributeId == objAttr.objWithAttr.Item2);

                            if (attribute != null)
                            {
                                attribute.Values = objAttr.sourseAttr.Values;
                                _logger.AddToLog($"К [{objExp}] добавлено значение [{objAttr.sourseAttr}] [{objAttr.sourseAttr.Values.First()}].");
                            }
                            else
                            {
                                //добавление нового атрибута
                                if (objAttr.sourseAttr != null)
                                {
                                    ExportedAttribute newAttr = objAttr.sourseAttr.Clone() as ExportedAttribute;
                                    objExp.exportedAttributes.Add(newAttr);
                                    _logger.AddToLog($"К [{objExp}] добавлен [{newAttr}] со значением [{newAttr.Values.First()}].");
                                }
                            }
                        }
                    }
                }
                // запись потомков
                _dataWriter.Write(data);
                // вызов для потомков
                foreach (ExportedObject item in data.ExportedObjectsList)
                    Processing(item);
            }

        }

        #region Методы для добавления новых условий экспорта
        /// <summary>
        /// Добавляет к родителю потомка
        /// </summary>
        /// <param name="newExportedObject"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public ExportTask AddObject(ExportedObject newExportedObject, long parent)
        {
            _objectsToAdd.Add(parent, newExportedObject);
            return this;
        }

        /// <summary>
        /// Не выгружает состав объекта
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public ExportTask Single(long objectId)
        {
            _singleObjects.Add(objectId);
            return this;
        }

        /// <summary>
        /// Не выгружает состав объекта типа
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public ExportTask Single(int objectTypeID)
        {
            _singleObjectsType.Add(objectTypeID);
            return this;
        }

        /// <summary>
        /// Запрещает выгружать состав объекта
        /// </summary>
        /// <param name="predicate">условие</param>
        /// <returns></returns>
        public ExportTask Single(Predicate<ExportedObject> predicate)
        {
            _singleConditions.Add(predicate);
            return this;
        }

        /// <summary>
        /// Не выгружает объект (и состав)
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public ExportTask Exclude(long objectId)
        {
            _objectsToRemove.Add(objectId);
            return this;
        }

        /// <summary>
        /// Не выгружает типы объектов
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public ExportTask Exclude(int objectTypeID)
        {
            _objectsTypesToRemove.Add(objectTypeID);
            return this;
        }

        /// <summary>
        /// Запрещает выгружать объект с составом
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public ExportTask Exclude(Predicate<ExportedObject> predicate)
        {
            _excludeConditions.Add(predicate);
            return this;
        }

        /// <summary>
        /// Выполняет действие если условие истинно
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public ExportTask DoAction(Predicate<ExportedObject> predicate, Action<ExportedObject> action)
        {
            _changeObjectConditionsActions.Add(predicate, action);
            return this;
        }

        /// <summary>
        /// Меняет или добавляет значение атрибута объекта по значению атрибута другого.
        /// </summary>
        /// <param name="objTypeAttribute_toChange">Кортеж из id типа объекта и id атрибута, который должен быть изменен</param>
        /// <param name="objTypeAttribute_value">Кортеж из id типа объекта и id атрибута, значение которого должно использоваться для изменения</param>
        /// <returns></returns>
        public ExportTask CreateOrChangeAttribute(Tuple<int, int> objTypeAttribute_toChange, Tuple<int, int> objTypeAttribute_value)
        {
            _copyAttrValueToAnotherAttr_forType.Add(new ObjAttrPairsAndClipboard(objTypeAttribute_value, objTypeAttribute_toChange));
            return this;
        }
        #endregion
    }
}