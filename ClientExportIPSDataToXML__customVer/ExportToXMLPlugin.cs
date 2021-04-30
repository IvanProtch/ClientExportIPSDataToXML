using Intermech.Bars;
using Intermech.Interfaces.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientExportIPSDataToXML
{
    public class ExportToXML : IPackage
    {
        public string Name
        {
            get
            {
                return "Экспорт данных IPS в формат XML";
            }
        }
        public void RunExportForm(object sender, EventArgs e)
        {
            ExportToXMLForm exportToXMLForm = new ExportToXMLForm();
            exportToXMLForm.ShowDialog();
        }

        public void Load(IServiceProvider serviceProvider)
        {
            BarManager barManager = serviceProvider.GetService(typeof(BarManager)) as BarManager;
            MenuBar menuBar = barManager.MenuBar;

            MenuItemBase item = new MenuBarItem("&Экспорт в XML");
            item.CommandName = "ExportIPSDataToXML_ClientPlugin";
            menuBar.Items.Add(item);

            MenuButtonItem executeMenuButton = new MenuButtonItem(this.Name,
                new EventHandler(RunExportForm));

            executeMenuButton.BeginGroup = true;
            item.Items.Add(executeMenuButton);
        }

        public void Unload()
        {

        }
    }
}
