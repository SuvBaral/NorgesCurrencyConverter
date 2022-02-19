using NorgesCurrencyConverter.SweaWebService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Westwind.Utilities;
using YAXLib;
using Exception = System.Exception;

namespace NorgesCurrencyConverter.BLL
{
    public class CurrencyRateRepository : DataAccess
    {
        public bool AddCurrencyValue(CurrencyConverterDetail currencyConverterDetail)
        {
            if (null == currencyConverterDetail) throw new Exception("Request Object cannot be null or empty");
            try
            {
                foreach (CurrencyConverter currencyConverter in currencyConverterDetail.CurrencyConverters)
                {
                    YAXSerializer serializer = new YAXSerializer(typeof(CurrencyConverter));
                    string CurrencyConverterXML = serializer.Serialize(currencyConverter);
                    using (DbCommand command = NBDB.GetStoredProcCommand("usp_InsertCurrencyDetail"))
                    {
                        // Adding parameters to the command
                        // YAXSerializer serializer = new YAXSerializer(typeof(CurrencyConverterDetail));
                        // string CurrencyConverterXML = serializer.Serialize(currencyConverterDetail);
                        if (!string.IsNullOrEmpty(CurrencyConverterXML))
                        {
                            CurrencyConverterXML = CurrencyConverterXML.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "").Trim('\r').Trim('\n');
                            NBDB.AddInParameter(command, "@p_Request", DbType.Xml, CurrencyConverterXML);
                            NBDB.AddOutParameter(command, "@op_Message", DbType.Int64, 32);

                            // Executing the stored procedure
                            NBDB.ExecuteNonQuery(command);

                            if (null != command.Parameters["@op_Message"] && null != command.Parameters["@op_Message"].Value)
                                if (!string.IsNullOrEmpty(command.Parameters["@op_Message"].Value.ToString()))
                                    return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            { throw new Exception(string.Format("(AddCurrencyValue) Error while inserting the given Currency Value : {0}", ex.ToString())); }
        }
        public CurrencyConverterDetail GetCurrencyConverter(string startDate, string EndDate, string Currencies, string currencyTypes)
        {
            CurrencyConverterDetail currencyConverterDetail = new CurrencyConverterDetail();
            currencyConverterDetail.CurrencyConverters = CurrencyRateMapping(startDate, EndDate, Currencies, currencyTypes);
            return currencyConverterDetail;
        }

        #region CurrencyRateMapping
        /// <summary>
        /// CurrencyRateMapping
        /// </summary>
        /// <param name="startDate">startDate</param>
        /// <param name="endDate">endDate</param>
        /// <returns>list of currency converter</returns>
        public List<CurrencyConverter> CurrencyRateMapping(string startDate, string endDate, string Currencies, string currencyTypes)
        {
            #region Local Variables
            string metaId = string.Empty;
            DateTime createdOn;
            if (currencyTypes == "Norges")
            {
                string jsonObjectResponse = ExecuteNorgesApi(startDate, endDate, Currencies);
                CurencyExchanger curencyExchanger = CurencyExchanger.FromJson(jsonObjectResponse);
                metaId = curencyExchanger.Meta.Id;
                createdOn = Convert.ToDateTime(curencyExchanger.Meta.Prepared.ToString("yyyy-MM-dd"));
                List<CurrencyConverter> lstOfCurrencyConverter = new List<CurrencyConverter>();
                #endregion

                foreach (SeriesElement baseseries in curencyExchanger.Data.Structure.Dimensions.Series.Where(obj => obj.Id == "BASE_CUR").ToList())
                {
                    int i = 0;
                    foreach (SeriesValueClass seriesValueClass in baseseries.Values)
                    {
                        List<ExchangeRateBasedOnDate> lstOfExchangeRateBasedOnDate = new List<ExchangeRateBasedOnDate>();
                        CurrencyConverter currencyConverter = new CurrencyConverter();
                        currencyConverter.MetaId = metaId;
                        currencyConverter.CreatedOn = createdOn;
                        currencyConverter.BaseCurrency = seriesValueClass.Name;
                        currencyConverter.QuoteCurrency = curencyExchanger.Data.Structure.Dimensions.Series.Where(obj => obj.Id == "QUOTE_CUR").Select(obj => obj.Values[0].Name).FirstOrDefault();
                        int priceIterator = 0;
                        foreach (KeyValuePair<string, string[]> seriesValue1 in curencyExchanger.Data.DataSets[0].Series.ElementAt(i).Value.Observations)
                        {

                            foreach (string value in seriesValue1.Value)
                            {
                                ExchangeRateBasedOnDate exchangeRateBasedOnDate = new ExchangeRateBasedOnDate();
                                exchangeRateBasedOnDate.CurrencyValue = Convert.ToDecimal(value);
                                exchangeRateBasedOnDate.CurrencyRateDate = Convert.ToDateTime(curencyExchanger.Data.Structure.Dimensions.Observation[0].Values[priceIterator].Id.ToString("yyyy-MM-dd"));
                                lstOfExchangeRateBasedOnDate.Add(exchangeRateBasedOnDate);
                                ++priceIterator;
                            }
                        }
                        currencyConverter.ExchangeRateBasedOnDates = lstOfExchangeRateBasedOnDate;
                        ++i;
                        lstOfCurrencyConverter.Add(currencyConverter);
                    }
                }
                return lstOfCurrencyConverter;
            }
            else
                return GetCurrencyInfoOfSwedish(startDate, endDate, Currencies);
        }
        #endregion



        #region GetCurrencyInfoOfSwedish
        /// <summary>
        /// Get Currency Info of swedish
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="currencies"></param>
        /// <returns>List<CurrencyConverter></returns>
        private List<CurrencyConverter> GetCurrencyInfoOfSwedish(string startDate, string endDate, string currencies)
        {
            int exchangeRatePercentage = Convert.ToInt32(ConfigurationManager.AppSettings["ExchangeRatePercentage"]);
            List<CurrencyConverter> lstOfCurrencyConverter = new List<CurrencyConverter>();
            CrossRequestParameters requestParameters = new CrossRequestParameters();
            IFormatProvider culture = new CultureInfo("en-US", true);
            requestParameters.datefrom = Convert.ToDateTime(startDate);
            requestParameters.dateto = Convert.ToDateTime(endDate);
            requestParameters.languageid = LanguageType.en;
            requestParameters.aggregateMethod = AggregateMethodType.D;
            List<CurrencyCrossPair> lstOfCurrencyCrossPair = new List<CurrencyCrossPair>();
            currencies.Split(',').ToList().ForEach(obj =>
            {
                lstOfCurrencyCrossPair.Add(new CurrencyCrossPair()
                {
                    seriesid1 = "SEK" + obj.Trim() + "PMI",
                    seriesid2 = "SEK"
                }
                );
            });
            requestParameters.crossPair = lstOfCurrencyCrossPair.ToArray();
            SweaWebServicePortTypeClient client = new SweaWebServicePortTypeClient();
            CrossResult response = client.getCrossRates(requestParameters);
            List<Series> lstOfSeries = client.getAllCrossNames(LanguageType.en).ToList();
            foreach (CrossResultSeries data in response.groups[0].series)
            {
                if (!data.resultrows.Any(obj => obj.value == null) && data.seriesname.Contains("1 SEK = ?"))
                {
                    List<ExchangeRateBasedOnDate> exchangeRateBasedOnDates = new List<ExchangeRateBasedOnDate>();
                    CurrencyConverter currencyConverter = new CurrencyConverter();
                    currencyConverter.BaseCurrency = lstOfSeries.Where(obj => obj.seriesname == data.seriesname.Substring(data.seriesname.Length - 3, 3).Trim()).Select(val => val.seriesdescription).FirstOrDefault();
                    currencyConverter.QuoteCurrency = lstOfSeries.Where(obj => obj.seriesname == data.seriesname.Substring(1, 4).Trim()).Select(val => val.seriesdescription).FirstOrDefault(); 
                    currencyConverter.CreatedOn = DateTime.Now;
                    foreach (ResultRow row in data.resultrows)
                    {
                        ExchangeRateBasedOnDate exchangeRate = new ExchangeRateBasedOnDate();
                        exchangeRate.CurrencyRateDate = row.date.Value;
                        exchangeRate.CurrencyValue = Convert.ToDecimal(row.value.GetValueOrDefault());
                        exchangeRate.AdjustedExchangeRate = Math.Round(((exchangeRatePercentage * exchangeRate.CurrencyValue) / 100), 4) + exchangeRate.CurrencyValue;
                        exchangeRateBasedOnDates.Add(exchangeRate);
                    }
                    currencyConverter.ExchangeRateBasedOnDates = exchangeRateBasedOnDates;
                    lstOfCurrencyConverter.Add(currencyConverter);
                }
            }
            return lstOfCurrencyConverter;
        }
        #endregion

        #region ExecuteNorgesApi
        /// <summary>
        /// Executes the norges api to get currency info
        /// </summary>
        /// <returns></returns>
        public static string ExecuteNorgesApi(string StartDate, string EndDate, string Currencies)
        {
            try
            {
                string apiurl = ConfigurationManager.AppSettings["ApiUrl"].ToString();
                Currencies = Currencies.Replace(",", "+");
                string ApiAuthUrl = $"{apiurl}B.{Currencies}.NOK.SP?format=sdmx-json&startPeriod={StartDate}&endPeriod={EndDate}&locale=en";
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(ApiAuthUrl);
                request.Accept = "application/json";
                request.ContentType = "application/json";
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                    else
                        throw new Exception($"Status Code : {response.StatusCode} ;Status Description : {response.StatusDescription}");
                }
            }
            catch (WebException ex)
            {
                string webError = string.Empty;
                if (ex.Response != null)
                {
                    using (var stream = ex.Response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        webError = reader.ReadToEnd();
                        Console.WriteLine(webError);
                    }
                }
                throw new Exception($"(ExecuteApiService) Error while processing in Execute, ErrorMsg : {ex.Message}, webError :{webError}");
            }
        }

        #endregion
    }
}
