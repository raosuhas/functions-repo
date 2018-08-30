#r "Newtonsoft.Json"

using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Models;
using Microsoft.Azure.KeyVault;
using Microsoft.Rest;


public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, string subscriptionId, string resourceGroupName, string miniRpName, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    string template = string.Concat( @"
                    {
                    '$schema': 'https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#',
                    'contentVersion': '1.0.0.0',
                    'parameters': {
                    },
                    'variables': {
                        'storageAccountName': '[uniquestring(deployment().name)]'
                    },
                    'resources': [
                        {
                        'type': 'Microsoft.Storage/storageAccounts',
                        'name': 'from" , miniRpName,  @"coolestapp',
                        'apiVersion': '2016-01-01',
                        'location': 'eastus2',
                        'sku': {
                            'name': 'Standard_LRS'
                        },
                        'kind': 'Storage',
                        'properties': {}
                        }
                    ],
                    'outputs': {    
                    }
                    }");

   // template = string.Format(template,miniRpName);
    JObject templateFileContents = JObject.Parse(template);
// ...
    var azureServiceTokenProvider = new AzureServiceTokenProvider();
    string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://management.azure.com/");

    var deployment = new Deployment();
    JObject parameterFileContents = JObject.Parse("{}");

    deployment.Properties = new DeploymentProperties
    {
        Mode = DeploymentMode.Incremental,
        Template = templateFileContents
        //Parameters = parameterFileContents.ToObject<JObject>()
    };

    var subscription = Guid.Parse("{ec60880c-0517-4cf5-9d34-c8c4732633dc}");
    string resourceGroup = "actionsdemo";
    string deploymentName = "deployment";

    //Create the resource manager client
            var resourceManagementClient = new ResourceManagementClient(new TokenCredentials(accessToken));
            resourceManagementClient.SubscriptionId = subscription.ToString() ;

    var deploymentResult = resourceManagementClient.Deployments.CreateOrUpdate(resourceGroup, deploymentName, deployment);
    Console.WriteLine(string.Format("Deployment status: {0}", deploymentResult.Properties.ProvisioningState));

    /*
    // parse query parameter
    dynamic body = await req.Content.ReadAsStringAsync();
    
    JObject input = JObject.Parse(body);
    var namevalue = (string)input["name"];
    //var rev = new string(namevalue.Reverse().ToArray());
    //log.Info(rev);

    var rev = namevalue.StartsWith("H") ? " is Awesome" : " is Bleah";
    rev = namevalue + rev;

    string randomJson = "{'Review' : '" + rev + "'}";
    JObject val = JObject.Parse(randomJson);
*/
    JObject val = JObject.Parse("{}");
    return req.CreateResponse(HttpStatusCode.OK, (JToken)val); 
}
