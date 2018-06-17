#r "System.Data"

using NetTools;
using System;
using System.Net;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    // parse query parameter
    string name = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
        .Value;

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    // Set name to query string or body data
    name = name ?? data?.name;
    /*
    var value1=GetEnvironmentVariable("WEBSITE_CONTENTSHARE");
    var value2=GetEnvironmentVariable("mifurmCustomValue");
    var value3=GetEnvironmentVariable("SQLCONNSTR_iprangesdbconnstring");
    log.Info(value1);
    log.Info(value2);
    log.Info(value3);
    log.Info(value4);
    */
    var value4=ConfigurationManager.ConnectionStrings["iprangesdbconnstring"]?.ConnectionString;
    
    try
    {
    IPHostEntry host;

    host = Dns.GetHostEntry(name);
    
    
            
    var ip = host.AddressList[0].ToString();
    var ipToCheck = IPAddress.Parse(ip);
    log.Info(ipToCheck.ToString());
            
           
    string connectionString = value4;
    var isItOnAzure=false;
    var resultRegion="none";
    //var connectionString = ConfigurationManager.ConnectionStrings["connection"];
    int count = 0;
    using (SqlConnection con = new SqlConnection(connectionString.ToString()))
    {
         con.Open();


        //UpdateDatabaseOfNewIpRanges(con);
        var checkRegionQuery = "Select * from Regions";
        var checkIfAlreadyFound="SELECT count(Domain) FROM FoundOnAzure Where Domain='{0}'";
        var insertIfNotFoundEarlier="INSERT INTO FoundOnAzure VALUES('{0}','{1}','{2}')";
        
        
        SqlCommand sqlCommand = new SqlCommand(checkRegionQuery, con);
        SqlDataReader reader=sqlCommand.ExecuteReader();
        while (reader.Read())
        {
            String region = (String)reader[1];
            String subnet = (String)reader[2];
            var range = IPAddressRange.Parse(subnet);
            if (range.Contains(ipToCheck))
            {
                log.Info("Found in " + region.ToString());
                isItOnAzure=true;
                resultRegion=region.ToString();
                break;
            }
        }
        
        if (isItOnAzure)
        {
            //checkIfAlreadyFound=checkIfAlreadyFound+name;
            checkIfAlreadyFound=String.Format(checkIfAlreadyFound, name);
            log.Info(checkIfAlreadyFound);
            SqlCommand checkIfAlreadyFoundcommand = new SqlCommand(checkIfAlreadyFound, con);
            
            var resultOfChecking=(int)checkIfAlreadyFoundcommand.ExecuteScalar();
            //var resultOfChecking=0;
            if (resultOfChecking!=1)
            {
                insertIfNotFoundEarlier=String.Format(insertIfNotFoundEarlier, name, resultRegion,ipToCheck);
                SqlCommand insertIfNotFoundEarliercommand = new SqlCommand(insertIfNotFoundEarlier, con);
                var resultOfInsert=insertIfNotFoundEarliercommand.ExecuteNonQuery();
            }
            
        
        }
        con.Close();
   
    }

    return name == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
        : isItOnAzure == false ? req.CreateResponse(HttpStatusCode.OK, "It might be on Azure one day!"):req.CreateResponse(HttpStatusCode.OK, "It looks it is on Azure, in <b>" + resultRegion + "</b> region.");
 }
        catch (Exception ex)
        {
       return req.CreateResponse(HttpStatusCode.BadRequest, "This host does not exsist!"); 
        }
}

public static string GetEnvironmentVariable(string name)
{
    return name + ": " +
        System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
}
