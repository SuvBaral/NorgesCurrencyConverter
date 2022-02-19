using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorgesCurrencyConverter.BLL
{
   public class DataAccess
    {
        private Database _NNDB = null;
        protected Database NBDB { get { return _NNDB; } }
        private string _dbconnectionString = ConfigurationManager.ConnectionStrings["Norges"].ConnectionString;
        public DataAccess( )
        {
            InitDefaultDatabases();
        }

        private void InitDefaultDatabases()
        {
            if (!string.IsNullOrEmpty(_dbconnectionString) && !string.IsNullOrWhiteSpace(_dbconnectionString))
            { _NNDB = new SqlDatabase(_dbconnectionString); }
        }
    }

   
}
