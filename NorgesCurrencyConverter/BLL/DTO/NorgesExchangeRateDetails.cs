using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorgesCurrencyConverter.BLL
{
    public class CurrencyConverterDetail
    {
        public List<CurrencyConverter> CurrencyConverters { get; set; }
    }

    public class CurrencyConverter
    {
        public string MetaId { get; set; }
        public string BaseCurrency { get; set; }
        public string QuoteCurrency { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public List<ExchangeRateBasedOnDate> ExchangeRateBasedOnDates { get; set; }
    }

    public class ExchangeRateBasedOnDate
    {
        public decimal CurrencyValue { get; set; }
        public System.DateTime CurrencyRateDate { get; set; }
        public decimal? AdjustedExchangeRate { get; set; }
    }
}
