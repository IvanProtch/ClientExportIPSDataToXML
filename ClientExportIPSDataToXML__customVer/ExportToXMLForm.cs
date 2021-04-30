using Intermech;
using Intermech.Interfaces;
using Intermech.Interfaces.Client;
using Intermech.Interfaces.Workflow;
using Intermech.Navigator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Concurrent;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using Intermech.Navigator.Views;
using Intermech.Kernel.Search;
using Intermech.Interfaces.XmlExchange;
using Intermech.Expert;
using Intermech.Interfaces.Briefcase;
using Intermech.Interfaces.XmlExchange.Services;

namespace ClientExportIPSDataToXML
{
    public partial class ExportToXMLForm : Form
    {
        private const int xmlExpCnfgType = 1673;
        private string _path = "D:\\ExportToXMLResults\\";
        public long _xmlConfigID = 3628574;

        private enum ExportModes
        {
            byII = 1066,
            byASM = 1074
        }
        private ExportModes exportMode;
        private List<AttachmentType> _objsToExport;

        public string exportPath
        {
            get
            {
                return _path;
            }
            private set
            {
                _path = exportPath_textBox.Text;
            }
        }
        public IXmlExchangeService xmlExchangeService;

        /// <summary>
        /// Вложенный класс для записи основных параметров вложений скрипта.
        /// Получать эти параметры через IDBObject не удобно
        /// </summary>
        private class AttachmentType
        {
            public long AttachmentID { get; set; }
            public string Name { get; set; }
        }

        public ExportToXMLForm()
        {
            InitializeComponent();
            _objsToExport = new List<AttachmentType>();
            folderBrowserDialog1.SelectedPath = _path;
            exportPath_textBox.Text = _path;

        }

        public string Name4ExportFile(long objectID, IUserSession session)
        {
            string Name = string.Empty;
            if (session.GetObject(objectID).Attributes.AddAttribute(9, false).AsString.Count() > 0)
            {
                Name = session.GetObject(objectID).Attributes.AddAttribute(9, false).AsString; // Обозначение
            }
            else
            {
                Name = session.GetObject(objectID).Attributes.AddAttribute(10, false).AsString; // Наименование
            }
            return Name;
        }

        private void exportModes_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (exportModes_comboBox.SelectedIndex == 0)
                exportMode = ExportModes.byII;

            if (exportModes_comboBox.SelectedIndex == 1)
                exportMode = ExportModes.byASM;
        }
        
        private void selectObjsToExp_button_Click(object sender, EventArgs e)
        {
            _objsToExport.RemoveAll(el => el.AttachmentID != 0);
            selectedObjs_comboBox.DataSource = null;
            selectedObjs_comboBox.DisplayMember = "";

            long[] selectedObjs = SelectionWindow.SelectObjects("Выберите объект вложения",
                "Выберите объект, состав которого будет показан в окне, а также открыт в отдельном окне \"Навигатора\"",
                (int)exportMode,
                SelectionOptions.SelectObjects |                    // Выбирать в окне можно узлы, содержащие объекты
                SelectionOptions.DisableSelectAbstractTypes |
                SelectionOptions.DisableSelectFromTree);

            // Ничего не выбрано
            if (selectedObjs == null || selectedObjs.Length == 0)
                return;
            using (SessionKeeper sessionKeeper = new SessionKeeper())
            {
                foreach (var item in selectedObjs)
                    _objsToExport.Add(new AttachmentType { AttachmentID = item, Name = sessionKeeper.Session.GetObject(item).Caption });
            }
            selectedObjs_comboBox.DataSource = _objsToExport;
            selectedObjs_comboBox.DisplayMember = "Name";
        }


        private void selectIPSConfig_button_Click(object sender, EventArgs e)
        {
            configurationIPS_label.Text = string.Empty;

            long[] selectedObjs = SelectionWindow.SelectObjects("Выберите объект вложения",
                "Выберите объект, состав которого будет показан в окне, а также открыт в отдельном окне \"Навигатора\"",
                xmlExpCnfgType,
                SelectionOptions.SelectObjects |                    // Выбирать в окне можно узлы, содержащие объекты
                SelectionOptions.DisableSelectFromTree |
                SelectionOptions.DisableMultiselect);

            // Ничего не выбрано
            if (selectedObjs == null || selectedObjs.Length == 0)
                return;

            _xmlConfigID = selectedObjs[0];
            using (SessionKeeper sessionKeeper = new SessionKeeper())
            {
                configurationIPS_label.Text = sessionKeeper.Session.GetObject(_xmlConfigID).Caption;
            }
        }


        private async void runExport_button_Click(object sender, EventArgs e)
        {
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            runExport_button.Enabled = false;
            progressBar.Step = (int)((1.0 / Convert.ToDouble(_objsToExport.Count)) * 100);
            for ( int i = 0; i < _objsToExport.Count; i++)
            {
                await Task.Run(() => RunExport2Obj(_objsToExport[i].AttachmentID));

                progressBar.Invoke(new MethodInvoker(delegate ()
                {
                    progressBar.PerformStep();
                }));

                procInfo_label.Text = string.Format("Документ {0} загружен на диск.", _objsToExport[i].Name);
            }
            procInfo_label.Text = "Выгрузка завершена.";
            progressBar.Value = 100;
            runExport_button.Enabled = true;
        }

        private void RunExport2Obj(long objectID)
        {
            #region Проверка параметров
            if (_xmlConfigID == Intermech.Consts.UnknownObjectId)
            {
                return;
            }

            #endregion

            // Конфигурация экспорта данных
            XmlExchangeExportSettings exportSett = null;

            using (SessionKeeper sessionKeeper = new SessionKeeper())
            {
                #region Анализ наличия служб
                // Служба экспорта XML
                IXmlExchangeService xmlExchangeSrv = sessionKeeper.Session.GetCustomService(typeof(IXmlExchangeService)) as IXmlExchangeService;
                // Проверим наличие службы
                if (xmlExchangeSrv == null)
                {
                    string errorMsg = String.Format("Служба {0} не найдена",
                                                    typeof(IXmlExchangeService).ToString());

                    // Кинем сообщение об ошибке (дальше никак нельзя)
                    throw new Exception(errorMsg);
                }
                #endregion

                #region Загрузка конфига
                // Наим. конфигурации для экспорта объектов
                string xmlConfigName = sessionKeeper.Session.GetObjectInfo(_xmlConfigID).Caption;

                // Попытка загрузки настроек экспорта
                if (!XmlExchangeExportHelper.LoadSettings(_xmlConfigID, sessionKeeper.Session, out exportSett))
                {
                    QuickObjectInfo xmlConfigInfo = sessionKeeper.Session.GetObjectInfo(_xmlConfigID);
                    string errorMsg = String.Format("Ошибка загрузки настроек для конфигурации \"{0}\" (ObjID={1})",
                                                    xmlConfigInfo.Caption,
                                                    _xmlConfigID);

                    throw new Exception(errorMsg);
                }
                #endregion

                #region Префикс имени файла
                // Префикс имени файла результата
                string FileNameExp = string.Empty;
                string nameFileExp = Name4ExportFile(objectID, sessionKeeper.Session);
                if (FileNameExp.Length == 0)
                {
                    FileNameExp = nameFileExp;
                }
                else
                {
                    FileNameExp = FileNameExp + "^" + nameFileExp;
                }
                if (FileNameExp.Length > 0)
                {
                    FileNameExp = FileNameExp + "^";
                    FileNameExp = OSHelper.ReplaceForbiddenSymbols(FileNameExp);
                }

                #endregion

                #region Проверка / получение правила подбора версий объектов

                // Если правило не задано явно в настройках - получаем текущее у пользователя
                if (exportSett.ObjVerRule == String.Empty)
                {
                    // Если не задано - берем по-умолчанию у хелпера
                    exportSett.ObjVerRule = DataHelper.Consts.cnt_def_filtrationRule;
                }
                #endregion

                // Список объектов для экспорта
                List<object> taskChunkItemList = _objsToExport.ConvertAll<object>(value => value.AttachmentID);

                #region Вызов задачи
            //    // Экспортируемые данные
            //    List<ExportAttribute> exportData = new List<ExportAttribute>(1);
            //    exportData.Add(new ExportAttribute(Intermech.Consts.CategoryObjectVersion, taskChunkItemList.ToArray()));

            //    string subTaskDir = exportPath;

            //    if (!Directory.Exists(subTaskDir))
            //    {
            //        Directory.CreateDirectory(subTaskDir);
            //    }

            //    IXmlExchangeExportTask xmlExportTask = xmlExchangeSrv.CreateExportTask(sessionKeeper.Session.SessionGUID);

            //    try
            //    {
            //        #region Экспорт данных
            //        string errorMsg = String.Empty;

            //        //Статус задачи
            //        XmlExchangeTaskStatus taskStatus = xmlExportTask.TaskStatus;

            //        // Вызов экспорта
            //        if (!xmlExportTask.ExportData(exportData.ToArray(), new object[] { exportSett, _xmlConfigID }, out errorMsg))
            //        {
            //            throw new Exception(errorMsg);
            //        }

            //        #endregion

            //        #region Выгрузка экспортированных данных

            //        string[] exportFiles = null;

            //        if (!xmlExportTask.GetExportFiles(out exportFiles))
            //        {
            //            return;
            //        }

            //        foreach (string exportFile in exportFiles)
            //        {

            //            string localFile = subTaskDir +
            //                               Path.DirectorySeparatorChar + FileNameExp/*Префикс*/ +
            //                               Path.GetFileName(exportFile);

            //            if (File.Exists(localFile))
            //            {
            //                File.Delete(localFile);
            //            }
                        
            //            IBlobReader dataReader = xmlExportTask.GetExportData(exportFile);
            //            if (dataReader == null)
            //            {
            //                continue;
            //            }

            //            try
            //            {
            //                dataReader.OpenBlob(4096);
            //                using (FileStream fs = new FileStream(localFile, FileMode.Create))
            //                {
            //                    try
            //                    {
            //                        long fileLen = fs.Length;
            //                        while (true)
            //                        {
            //                            byte[] exportFileData = dataReader.ReadDataBlock();
            //                            if (exportFileData.Length == 0)
            //                            {
            //                                break;
            //                            }

            //                            fs.Write(exportFileData, 0, exportFileData.Length);
            //                        }
            //                    }
            //                    finally
            //                    {
            //                        fs.Flush();
            //                        fs.Close();
            //                    }
            //                }
            //            }
            //            finally
            //            {
            //                dataReader.CloseBlob();
            //            }

            //        }

            //        #endregion

            //    }
            //    finally
            //    {
            //        // Удаление задания
            //        //xmlExchangeSrv.DisposeExportTask(xmlExportTask.TaskGuid);
            //        xmlExportTask.Dispose();
            //    }

            #endregion
            }
        }

        private void exportPathSelect_button_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                exportPath_textBox.Text = folderBrowserDialog1.SelectedPath;
                _path = folderBrowserDialog1.SelectedPath;
            }
                
        }

    }
}
