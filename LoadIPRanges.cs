#r "System.Xml.Serialization"

using System.Net;
using System.Xml.Serialization;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    AzurePublicIpAddressService azurePublic=new AzurePublicIpAddressService();
    Region[] azureAddresses= azurePublic.GetRegions();

    // parse query parameter
    string name = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
        .Value;

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    // Set name to query string or body data
    name = name ?? data?.name;

    return name == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, "Hello " + name);
}

    public interface IAzurePublicIpAddressService
    {
        Region[] GetRegions();
    }
    public class AzurePublicIpAddressService : IAzurePublicIpAddressService
    {
        public Region[] GetRegions()
        {
            XmlSerializer ser = new XmlSerializer(typeof(AzurePublicIpAddresses));
            FileStream myFileStream = new FileStream(@"D:\home\site\wwwroot\LoadIPRanges\IPRanges\PublicIPs.xml", FileMode.Open);
            return ((AzurePublicIpAddresses)ser.Deserialize(myFileStream)).Regions;
        }
    }

    public class IpRange
    {
        [XmlAttribute]
        public string Subnet { get; set; }
    }

    public class Region
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlElement("IpRange")]
        public IpRange[] Ranges { get; set; }
    }
    
    [XmlRoot("AzurePublicIpAddresses")]
    public class AzurePublicIpAddresses
    {
        [XmlElement("Region")]
        public Region[] Regions { get; set; }
    }
