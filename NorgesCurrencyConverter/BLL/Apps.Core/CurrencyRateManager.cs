using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorgesCurrencyConverter.BLL
{
    public class CurrencyRateManager
    {

        CurrencyRateRepository currencyRate = null;
        #region AddCurrencyValue
        /// <summary>
        /// Performs insert operation of currencies info 
        /// </summary>
        /// <param name="request">request</param>
        /// <returns>Process resut</returns>
        public bool AddCurrencyValue(string startDate, string EndDate, string Currencies, string currencyType)
        {
            CurrencyConverterDetail currencyConverterDetail = GetCurrencyConverter(startDate, EndDate, Currencies, currencyType);
            if (null == currencyRate) currencyRate = new CurrencyRateRepository();
            return currencyRate.AddCurrencyValue(currencyConverterDetail);
        }
        #endregion

        #region GetCurrencyConverter
        /// <summary>
        /// Get Currency rate through norges api
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="Currencies"></param>
        /// <returns></returns>
        public CurrencyConverterDetail GetCurrencyConverter(string startDate, string EndDate, string Currencies,string currencyType)
        {
            if (null == currencyRate) currencyRate = new CurrencyRateRepository();
            return currencyRate.GetCurrencyConverter(startDate, EndDate, Currencies,currencyType);
        }
        #endregion 
    }
}
