using NorgesCurrencyConverter.BLL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NorgesCurrencyConverter
{
    public partial class CurrencyExchangeRateSaver : Form
    {
        #region Global variables
        List<string> Currencies = ConfigurationManager.AppSettings["Currencies"].Split(',').ToList();
        DateTimePicker StartDate = new DateTimePicker();
        DateTimePicker EndDate = new DateTimePicker();
        string currencyNames = string.Empty;
        #endregion
        public CurrencyExchangeRateSaver()
        {
            InitializeComponent();
            ccb.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ccb_ItemCheck);
        }

        private void CurrencyExchangeRateSaver_Load(object sender, EventArgs e)
        {
            CCBoxItem firstItem = new CCBoxItem();
            firstItem.Name = "Select All";
            firstItem.Value = 0;
            ccb.Items.Add(firstItem);
            for (int i = 1; i < Currencies.Count; i++)
            {
                CCBoxItem item = new CCBoxItem(Currencies[i], i);
                ccb.Items.Add(item);
            }
            ccb.Name = "CurrencyCode";
            ccb.DisplayMember = "Name";
            ccb.ValueSeparator = ", ";
        }

        private void ccb_DropDownClosed(object sender, EventArgs e)
        {
            // Display all checked items.
            StringBuilder sb = new StringBuilder("Items checked: ");
            foreach (CCBoxItem item in ccb.CheckedItems)
            {
                sb.Append(item.Name).Append(ccb.ValueSeparator);
            }
            sb.Remove(sb.Length - ccb.ValueSeparator.Length, ccb.ValueSeparator.Length);
        }
        private void ccb_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.Index == 0 && e.NewValue.ToString() == "Checked")
            {
                for (int i = 1; i < Currencies.Count; i++)
                {
                    ccb.SetItemChecked(i, true);
                }
            }
            else
            {
                CCBoxItem item = ccb.Items[e.Index] as CCBoxItem;
            }
        }
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            CurrencyRateRepository currencyRateRepository = new CurrencyRateRepository();
            string CurrencyType = comboBox1.SelectedItem.ToString();
            string startDateTime = dateTimePicker1.Value.ToString("yyyy-MM-dd");
            string endDateTime = dateTimePicker2.Value.ToString("yyyy-MM-dd"); 
            List<string> checkedItems = (from CCBoxItem c in ccb.CheckedItems where c is CCBoxItem select c.Name).ToList();
            if (checkedItems.Contains("Select All")) checkedItems.RemoveAt(0);
            CurrencyRateManager currencyManager = new CurrencyRateManager();
            currencyManager.AddCurrencyValue(startDateTime, endDateTime, string.Join(",", checkedItems),CurrencyType);
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
