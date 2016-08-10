using System.Configuration;

namespace YouTubeListManager.Test.Common.Helpers
{
    public class TestDbHelper
    {
        private const string TestConnectionName = "TestConnection";
        private const string FallbackConnectionString = "Data Source=LT-MKISS-001;Initial Catalog=TestYouTubeListManager;Integrated Security=True";

        public string GetConnectionString()
        {
            return FallbackConnectionString; //ConfigurationManager.ConnectionStrings[TestConnectionName].ConnectionString;
        }
    }
}