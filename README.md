# ClientExportIPSDataToXML
Модуль расширения PDM-PLM системы IPS, осуществляющий экспорт состава выбранного объекта в документ xml.
Модуль работает в скрипте, который выполняется в определенное время автоматически.
    Пример:
            // Получение конфигурации экспорта
            XmlExchangeExportSettings exportSett = new XmlExchangeExportSettings();
            XmlExchangeExportHelper.LoadSettings(1110767, session, out exportSett);
            
            ExportTask exportTask = new ExportTask(session, 42292460, path, exportSett);
            
            // Перед запуском задачи экспорта можно задать ряд методов, которые выполнятся в процессе экспорта
            exportTask
                .AddObject(ExportedObject.Create(1001, "test", 12345), 42292460)
                .Exclude(el => el.ObjectType == 1317)
                .Single(el => el.ObjectType == 1052)
                .Single(el => el.ObjectType == 1176)
                .DoAction(obj => obj.ObjectType == 1001, obj => obj.typeName = "newType")
                .CreateOrChangeAttribute(new Tuple<int, int>(1075, 0), new Tuple<int, int>(1110, 1125))
                
                .StartExportTask(writeExceptionToLog: false);

    Результат записывается с учетом иерархии:
<?xml version="1.0" encoding="utf-8"?>
<main_item name="Сборочная единица '3ТЭ25К2М.000.00.000 (Тепловоз магистральный грузовой 3ТЭ25К2М)'" type="1074" id="42292460" guid="a1049b99-29c8-47d9-8f3b-472b09c3b4b4">
  <attributs>
    <attribute name="Идентификатор версии объекта" type="-2" guid="cad00029-306c-11d8-b4e9-00304f19f545">Идентификатор версии объекта<attribute_values><value>42292460</value></attribute_values></attribute>
    <attribute name="Заголовок объекта" type="-50" guid="cad00047-306c-11d8-b4e9-00304f19f545">Заголовок объекта<attribute_values><value>3ТЭ25К2М.000.00.000 (Тепловоз магистральный грузовой 3ТЭ25К2М)</value></attribute_values></attribute>
    <attribute name="Тип объекта" type="-7" guid="cad0002e-306c-11d8-b4e9-00304f19f545">Тип объекта<attribute_values><value>1074</value></attribute_values></attribute>
    <attribute name="Глобальный идентификатор версии объекта" type="-12" guid="cad00130-306c-11d8-b4e9-00304f19f545">Глобальный идентификатор версии объекта<attribute_values><value>36c1cf93-9fc2-4a7e-b5b4-848f5ed007c7</value></attribute_values></attribute>
  </attributs>
  <item name="Маршрут обработки '3ТЭ25К2М.000.00.000 1 МО БМЗ'" type="1037" id="37971432" guid="c0ea35ec-3975-46b2-9384-c7e3c744daeb">
    <attributs>
      <attribute name="Идентификатор версии объекта" type="-2" guid="cad00029-306c-11d8-b4e9-00304f19f545">Идентификатор версии объекта<attribute_values><value>37971432</value></attribute_values></attribute>
      <attribute name="Заголовок объекта" type="-50" guid="cad00047-306c-11d8-b4e9-00304f19f545">Заголовок объекта<attribute_values><value>3ТЭ25К2М.000.00.000 1 МО БМЗ</value></attribute_values></attribute>
      <attribute name="Тип объекта" type="-7" guid="cad0002e-306c-11d8-b4e9-00304f19f545">Тип объекта<attribute_values><value>1037</value></attribute_values></attribute>
      <attribute name="Глобальный идентификатор версии объекта" type="-12" guid="cad00130-306c-11d8-b4e9-00304f19f545">Глобальный идентификатор версии объекта<attribute_values><value>9d70ba5a-c8a5-4956-b8b6-4a873eb7eef5</value></attribute_values></attribute>
    </attributs>
    <item name="Расцеховочный маршрут '995Сб-995О'" type="1176" id="37971430" guid="1960c010-f4a4-478c-8454-6383514fefb7">
      <attributs>
        <attribute name="Идентификатор версии объекта" type="-2" guid="cad00029-306c-11d8-b4e9-00304f19f545">Идентификатор версии объекта<attribute_values><value>37971430</value></attribute_values></attribute>
        <attribute name="Заголовок объекта" type="-50" guid="cad00047-306c-11d8-b4e9-00304f19f545">Заголовок объекта<attribute_values><value>995Сб-995О</value></attribute_values></attribute>
        <attribute name="Тип объекта" type="-7" guid="cad0002e-306c-11d8-b4e9-00304f19f545">Тип объекта<attribute_values><value>1176</value></attribute_values></attribute>
        <attribute name="Глобальный идентификатор версии объекта" type="-12" guid="cad00130-306c-11d8-b4e9-00304f19f545">Глобальный идентификатор версии объекта<attribute_values><value>d84eace2-8fda-474b-b423-2fc332b8c4a1</value></attribute_values></attribute>
      </attributs>
    </item>
    <item name="Единичный техпроцесс '3ТЭ25К2М.000.00.000 Б1 ТП'" type="1237" id="37971554" guid="ccd2ca7d-692e-4507-a5d1-cbcc95611eb1">
      <attributs>
        <attribute name="Идентификатор версии объекта" type="-2" guid="cad00029-306c-11d8-b4e9-00304f19f545">Идентификатор версии объекта<attribute_values><value>37971554</value></attribute_values></attribute>
        <attribute name="Заголовок объекта" type="-50" guid="cad00047-306c-11d8-b4e9-00304f19f545">Заголовок объекта<attribute_values><value>3ТЭ25К2М.000.00.000 Б1 ТП</value></attribute_values></attribute>
        <attribute name="Тип объекта" type="-7" guid="cad0002e-306c-11d8-b4e9-00304f19f545">Тип объекта<attribute_values><value>1237</value></attribute_values></attribute>
        <attribute name="Глобальный идентификатор версии объекта" type="-12" guid="cad00130-306c-11d8-b4e9-00304f19f545">Глобальный идентификатор версии объекта<attribute_values><value>87642f1a-3ae0-4ad6-bd12-a1eb27db5bde</value></attribute_values></attribute>
      </attributs>
      <item name="Цехозаход 'Цех995'" type="1110" id="37971556" guid="94637cf1-522d-48c7-ae6b-420d4a73eb72">
        <attributs>
          <attribute name="Идентификатор версии объекта" type="-2" guid="cad00029-306c-11d8-b4e9-00304f19f545">Идентификатор версии объекта<attribute_values><value>37971556</value></attribute_values></attribute>
          <attribute name="Заголовок объекта" type="-50" guid="cad00047-306c-11d8-b4e9-00304f19f545">Заголовок объекта<attribute_values><value>Цех995</value></attribute_values></attribute>
          <attribute name="Тип объекта" type="-7" guid="cad0002e-306c-11d8-b4e9-00304f19f545">Тип объекта<attribute_values><value>1110</value></attribute_values></attribute>
          <attribute name="Глобальный идентификатор версии объекта" type="-12" guid="cad00130-306c-11d8-b4e9-00304f19f545">Глобальный идентификатор версии объекта<attribute_values><value>cc47921d-ed3e-4e7d-8aef-bc8df725504d</value></attribute_values></attribute>
        </attributs>
      </item>
    </item>
  </item>
  <item name="test" type="1001" id="12345" guid="a65a81f7-0756-4d2f-8d20-e3360ee77b96">
    <attributs>
      <attribute name="Идентификатор версии объекта" type="-2" guid="cad00029-306c-11d8-b4e9-00304f19f545">Идентификатор версии объекта<attribute_values><value>12345</value></attribute_values></attribute>
      <attribute name="Заголовок объекта" type="-50" guid="cad00047-306c-11d8-b4e9-00304f19f545">Заголовок объекта<attribute_values><value>test</value></attribute_values></attribute>
      <attribute name="Тип объекта" type="-7" guid="cad0002e-306c-11d8-b4e9-00304f19f545">Тип объекта<attribute_values><value>1001</value></attribute_values></attribute>
      <attribute name="Идентификатор версии объекта" type="-12" guid="cad00130-306c-11d8-b4e9-00304f19f545">Идентификатор версии объекта<attribute_values><value>a65a81f7-0756-4d2f-8d20-e3360ee77b96</value></attribute_values></attribute>
    </attributs>
  </item>
</main_item>

