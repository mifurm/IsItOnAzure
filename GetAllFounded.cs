#r "System.Data"
#r "Newtonsoft.Json"

using System;
using System.Net;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{

    dynamic data = await req.Content.ReadAsAsync<object>();
       
    string connectionString = ConfigurationManager.ConnectionStrings["iprangesdbconnstring"]?.ConnectionString;
   
    List<FoundRegion> foundRegions=new List<FoundRegion>();
    using (SqlConnection con = new SqlConnection(connectionString.ToString()))
    {
        con.Open();

        
        var foundOnAzureQuery = "Select Domain, Region, IP from FoundOnAzure";
        
        
        SqlCommand sqlCommand = new SqlCommand(foundOnAzureQuery, con);
        SqlDataReader reader=sqlCommand.ExecuteReader();
        

        while (reader.Read())
        {
            String domain = (String)reader[0];
            String region = (String)reader[1];
            String ip     = (String)reader[2];
            foundRegions.Add(new FoundRegion(){ Domain=domain, Region=region, IP=ip});   
        }
        log.Info(foundRegions.Count.ToString());
       
        con.Close();
    }
    var json = JsonConvert.SerializeObject(new
        {
            operations = foundRegions
        });
        log.Info(json.ToString());
    return req.CreateResponse(HttpStatusCode.OK, json);
}        

public class FoundRegion
{
    public string Domain {get; set;}
    public string Region {get; set;}
    public string IP {get; set;}

}

/*
 class MyClass
        {
             public string email_address { get; set; }
             public string status { get; set; }
        }

        List<MyClass> data = new List<MyClass>() { new MyClass() { email_address = "email1@email.com", status = "good2go" }, new MyClass() { email_address = "email2@email.com", status = "good2go" } };
        var json = JsonConvert.SerializeObject(new
        {
            operations = data
        });
*/
