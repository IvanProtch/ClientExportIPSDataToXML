using Intermech.Interfaces;
using Intermech.Interfaces.Compositions;
using Intermech.Interfaces.XmlExchange;
using Intermech.Kernel.Search;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace IPStoXmlDataExport
{
    public class ExportData
    {
        public ExportedObject HeaderObject { get; set; }

        public List<ExportedObject> ExportedObjectsList { get; set; }

        public ExportData(ExportedObject headerObject)
        {
            HeaderObject = headerObject;
            ExportedObjectsList = new List<ExportedObject>();
        }

        public override bool Equals(object obj)
        {
            if (this.HeaderObject == (obj as ExportData).HeaderObject)
            {
                foreach (ExportedObject eobj1 in this.ExportedObjectsList)
                {
                    foreach (ExportedObject eobj2 in (obj as ExportData).ExportedObjectsList)
                    {
                        if (eobj1 != eobj2)
                            return false;
                    }
                }
                return true;
            }
            else return false;
        }

        public override string ToString() => $"Данные объекта {HeaderObject.caprion}({HeaderObject.ObjectID}), в составе {ExportedObjectsList.Count}ед.";
    }

    public class ExportedObject : ICloneable
    {
        protected IUserSession Session;
        protected List<int> _attributsFromSettings;
        internal bool allowToLoadConsistance = true;

        //Обязательные атрибуты
        public long ObjectID;

        public int ObjectType;
        public string caprion;
        public string Guid;
        public string typeName;

        public List<ExportedAttribute> exportedAttributes = new List<ExportedAttribute>();

        public ExportedObject(long objectID, XmlExchangeExportSettings settings, IUserSession session)
        {
            Session = session;
            ObjectID = objectID;
            try
            {
                if (settings != null)
                {
                    _attributsFromSettings = settings
                        .AttrSettings
                        .Select(attrSet => Session.GetAttributeType((attrSet as XmlExchangeExportTypedBase).TypeGuid).AttributeID)
                        .ToList();
                }
            }
            catch (Exception exc)
            {
                throw;
            }

            // инициализация
            try
            {
                IDBObject obj = Session.GetObject(ObjectID);

                ObjectType = obj.ObjectType;
                caprion = obj.NameInMessages;
                Guid = obj.GUID.ToString();
                typeName = Session.GetObjectType(ObjectType).ObjectTypeName;

                AttributeValues[] attrs = obj.GetAttributesValues(GetAttributeValuesModes.IncludeGuid | GetAttributeValuesModes.IncludeObligatoryAttributes);

                // добавляем дополнительные атрибуты в _attributsFromSettings
                XmlExchangeExportObj attrsForType = settings.ObjSettings
                    .Find(objSet => Session.GetObjectType((objSet as XmlExchangeExportObj).TypeGuid).ObjectType == ObjectType) as XmlExchangeExportObj;
                //var attrsForType = settings.GetAdditionalObjTypeAttributs()
                if (attrsForType != null && attrsForType.AttrList.Any())
                {
                    _attributsFromSettings
                    .AddRange(attrsForType.AttrList
                    .Select(attrSet => Session.GetAttributeType((attrSet as XmlExchangeExportTypedBase).TypeGuid).AttributeID)
                    .ToList());
                }

                //Добавляем атрибуты к объекту
                exportedAttributes.Add(new ExportedAttribute(attrs.FirstOrDefault(attr => attr.AttributeID == (int)ObligatoryObjectAttributes.F_OBJECT_ID), obj));
                exportedAttributes.Add(new ExportedAttribute(attrs.FirstOrDefault(attr => attr.AttributeID == (int)ObligatoryObjectAttributes.CAPTION), obj));
                exportedAttributes.Add(new ExportedAttribute(attrs.FirstOrDefault(attr => attr.AttributeID == (int)ObligatoryObjectAttributes.F_OBJECT_TYPE), obj));
                exportedAttributes.Add(new ExportedAttribute(attrs.FirstOrDefault(attr => attr.AttributeID == (int)ObligatoryObjectAttributes.F_GUID), obj));

                if (_attributsFromSettings != null)
                {
                    foreach (int attribute in _attributsFromSettings)
                    {
                        AttributeValues objAttribute = attrs.FirstOrDefault(attr => attr.AttributeID == attribute);

                        if (attribute == (int)ObligatoryObjectAttributes.CAPTION || attribute == (int)ObligatoryObjectAttributes.F_OBJECT_ID
                        || attribute == (int)ObligatoryObjectAttributes.F_OBJECT_TYPE || attribute == (int)ObligatoryObjectAttributes.F_GUID)
                            continue;

                        if (objAttribute != null)
                            exportedAttributes.Add(new ExportedAttribute(objAttribute, obj));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Из строки таблицы в ExportedObject
        /// </summary>
        /// <param name="row"></param>
        public ExportedObject(DataRow row)
        {
            exportedAttributes = new List<ExportedAttribute>();

            ObjectID = row.Field<long>("Идентификатор версии объекта");
            ObjectType = row.Field<int>("Тип объекта");
            caprion = row.Field<string>("Заголовок объекта");
            Guid = row.Field<Guid>("Глобальный идентификатор версии объекта").ToString();

            List<AttributeValues> attributes = new List<AttributeValues>();

            DataColumnCollection columns = row.Table.Columns;
            for (int i = 0; i < columns.Count; i++)
            {
                AttributeValues attr = new AttributeValues(-1);
                attr.AttributeName = columns[i].Caption;
                attr.Values = row.ItemArray;
                attributes.Add(attr);
            }

            foreach (AttributeValues attribute in attributes)
            {
                if (attribute != null)
                    exportedAttributes.Add(new ExportedAttribute(attribute));
            }
        }

        public ExportedObject()
        {
        }

        public static ExportedObject Create(int type, string caption, long objectID = 0)
        {
            Guid guid = System.Guid.NewGuid();
            ExportedObject exportedObject = new ExportedObject();
            exportedObject.Guid = guid.ToString();
            exportedObject.ObjectID = objectID == 0 ? (long)Math.Abs(guid.GetHashCode()) : objectID;
            exportedObject.ObjectType = type;
            exportedObject.caprion = caption;
            exportedObject.allowToLoadConsistance = false;

            exportedObject.exportedAttributes.Add(new ExportedAttribute((int)ObligatoryObjectAttributes.F_OBJECT_ID, "Идентификатор версии объекта", "cad00029-306c-11d8-b4e9-00304f19f545", new List<object>() { exportedObject.ObjectID }));
            exportedObject.exportedAttributes.Add(new ExportedAttribute((int)ObligatoryObjectAttributes.CAPTION, "Заголовок объекта", "cad00047-306c-11d8-b4e9-00304f19f545", new List<object>() { caption }));
            exportedObject.exportedAttributes.Add(new ExportedAttribute((int)ObligatoryObjectAttributes.F_OBJECT_TYPE, "Тип объекта", "cad0002e-306c-11d8-b4e9-00304f19f545", new List<object>() { type }));
            exportedObject.exportedAttributes.Add(new ExportedAttribute((int)ObligatoryObjectAttributes.F_GUID, "Идентификатор версии объекта", "cad00130-306c-11d8-b4e9-00304f19f545", new List<object>() { exportedObject.Guid }));

            return exportedObject;
        }

        //<attributs>
        //  <attribute name = "Идентификатор версии объекта" type="-2" guid="cad00029-306c-11d8-b4e9-00304f19f545">Идентификатор версии объекта<attribute_values><value>66828407</value></attribute_values></attribute>
        //  <attribute name = "Заголовок объекта" type="-50" guid="cad00047-306c-11d8-b4e9-00304f19f545">Заголовок объекта<attribute_values><value>ТЭМ23.030.05.000 МС (Установка переходной площадки)</value></attribute_values></attribute>
        //  <attribute name = "Тип объекта" type="-7" guid="cad0002e-306c-11d8-b4e9-00304f19f545">Тип объекта<attribute_values><value>1317</value></attribute_values></attribute>
        //  <attribute name = "Глобальный идентификатор версии объекта" type= "-12" guid= "cad00130-306c-11d8-b4e9-00304f19f545" > Глобальный идентификатор версии объекта<attribute_values><value>e5e40c42-2345-4ee3-9246-a376cb26d106</value></attribute_values></attribute>
        //</attributs>

        public override bool Equals(object obj)
        {
            if (this.Guid == (obj as ExportedObject).Guid)
            {
                foreach (ExportedAttribute attr in this.exportedAttributes)
                {
                    foreach (ExportedAttribute attrObj in (obj as ExportedObject).exportedAttributes)
                    {
                        if (attr != attrObj)
                            return false;
                    }
                }
                return true;
            }
            else return false;
        }

        public override string ToString() => $"Объект {caprion}({ObjectID}) типа {typeName}";

        public object Clone()
        {
            return ExportedObject.Create(ObjectType, caprion);
        }
    }

    /// <summary>
    /// Полный состав СЕ с иерархией
    /// </summary>
    public class ExportedAsm : ExportedObject
    {
        private XmlExchangeExportSettings _settings;
        private List<int> _objTypes;
        private List<int> _relations;
        private List<ExportedAsm> _childObjects = new List<ExportedAsm>();

        public ExportedAsm RootObject
        {
            get
            {
                ExportedAsm parent = ParentObject;
                while (parent?.ParentObject != null)
                    parent = parent.ParentObject;
                return parent;
            }
        }

        public ExportedAsm ParentObject { get; set; }

        public List<ExportedAsm> ChildObjects
        {
            get
            {
                if(_childObjects.Count == 0)
                    GetAllConsistance(this);

                return _childObjects;
            }
        }

        public ExportedAsm(long headerObject, XmlExchangeExportSettings settings, IUserSession session) :
            base(headerObject, settings, session)
        {
            _settings = settings;

            if (_settings != null)
            {
                _objTypes = _settings.ObjSettings.Select(objSetting => session.GetObjectType((objSetting as XmlExchangeExportTypedBase).TypeGuid).ObjectType).ToList();
                _relations = _settings.RelSettings.Select(relSettings => session.GetRelationType((relSettings as XmlExchangeExportTypedBase).TypeGuid).RelationType).ToList();
            }
            else
            {
                _objTypes = null;
                _relations = new List<int>() { 1, 1002, 1011, 1004, 1007, 1060 };
            }
        }

        private void GetAllConsistance(ExportedAsm asm)
        {
            List<ExportedAsm> childs = GetObjectConsistance(asm);
            foreach (ExportedAsm item in childs)
            {
                item.ParentObject = asm;
                GetAllConsistance(item);
                asm._childObjects.Add(item);
            }
        }

        private List<ExportedAsm> GetObjectConsistance(ExportedAsm asm)
        {
            ICompositionLoadService _loadService = Session.GetCustomService(typeof(ICompositionLoadService)) as ICompositionLoadService;
            List<ColumnDescriptor> col = new List<ColumnDescriptor>
            {
                new ColumnDescriptor(ObligatoryObjectAttributes.F_OBJECT_ID, AttributeSourceTypes.Object, ColumnContents.ID, ColumnNameMapping.Index, SortOrders.NONE, 0),
            };
            List<long> reslt = new List<long>();
            Session.EditingContextID = asm.ObjectID;
            DataTable dt = _loadService.LoadComplexCompositions(
            Session.SessionGUID,
            new List<ObjInfoItem> { new ObjInfoItem(asm.ObjectID) },
            _relations,
            _objTypes,
            col,
            true, // Режим получения данных (true - состав, false - применяемость)
            false, // Флаг группировки данных
            null, // Правило подбора версий
            null, // Условия на объекты при получении состава / применяемости
                  //SystemGUIDs.filtrationLatestVersions, // Настройки фильтрации объектов (последние)
                  //SystemGUIDs.filtrationBaseVersions, // Настройки фильтрации объектов (базовые)
            Intermech.SystemGUIDs.filtrationLatestVersions,
            null,
            1 // Количество уровней для обработки (-1 = загрузка полного состава / применяемости)
            );
            if (dt == null)
                return new List<ExportedAsm>();
            else
                return dt.Rows.OfType<DataRow>()
            .Select(element => (long)element[0])
            .Select(element => new ExportedAsm(element, _settings, Session))
            .ToList();
        }        
        
        public override string ToString() => $"{caprion}({ObjectID})";
    }

    public class ExportedAttribute : ICloneable
    {
        public int AttributeId;
        public string Name;
        public string Guid;
        public List<object> Values;

        public ExportedAttribute(int attributeID, string name, string guid, List<object> values)
        {
            this.AttributeId = attributeID;
            this.Name = name;
            this.Values = values;
            this.Guid = guid;
        }

        public ExportedAttribute(AttributeValues objAttribute, IDBObject obj)
        {
            this.AttributeId = objAttribute.AttributeID;
            IDBAttributeType attr = obj.GetAttributeType(AttributeId);

            this.Name = attr.Name;
            this.Guid = objAttribute.AttributeGuid.ToString();
            this.Values = objAttribute.Values.ToList();
        }

        public ExportedAttribute(AttributeValues objAttribute)
        {
            this.AttributeId = objAttribute.AttributeID;
            this.Name = objAttribute.AttributeName;
            this.Guid = objAttribute.AttributeGuid.ToString();
            this.Values = objAttribute.Values.ToList();
        }

        public override bool Equals(object obj) => this.Guid == (obj as ExportedAttribute).Guid;

        public override string ToString() => $"{Name}({AttributeId}) = {Values.First().ToString()}";

        public object Clone()
        {
            return new ExportedAttribute(AttributeId, Name, Guid, Values);
        }
    }
}