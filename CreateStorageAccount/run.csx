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

    DateTime now = DateTime.Now;
    string name = now.ToString("G").Replace("/","").Replace(":","").Replace(" ","").ToLower();
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
                        'name': 'stor" , name,  @"',
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

    var subscription = Guid.Parse(subscriptionId);
    string resourceGroup = resourceGroupName;
    string deploymentName = "deployment";

    //Create the resource manager client
            var resourceManagementClient = new ResourceManagementClient(new TokenCredentials(accessToken));
            resourceManagementClient.SubscriptionId = subscription.ToString() ;

    var deploymentResult = resourceManagementClient.Deployments.CreateOrUpdate(resourceGroup, deploymentName, deployment);
    Console.WriteLine(string.Format("Deployment status: {0}", deploymentResult.Properties.ProvisioningState));

    JObject val = JObject.Parse("{}");
    return req.CreateResponse(HttpStatusCode.OK, (JToken)val); 
}
