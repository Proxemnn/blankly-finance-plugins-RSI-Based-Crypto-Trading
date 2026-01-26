using System;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.ContainerInstance.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace Crybot.TradingUtils
{
    public class AzureContainerManager
    {
        private const int Port = 3000;
        private const string AzureLoginPath = "azureauth";
        private static string ResourceGroupName = "acicontainer";
        private static string ContainerGroupName = "chart-analyzer";
        private static string ContainerImageApp = "thewindev/chart-analyzer";

        public string StartImageAnalyzer()
        {
            IAzure azure = GetAzureContext(Environment.GetEnvironmentVariable(AzureLoginPath));
            CreateResourceGroup(azure, ResourceGroupName, Region.EuropeWest);
            return RunTaskBasedContainer(azure, ResourceGroupName, ContainerGroupName, ContainerImageApp, null);
        }

        public void StopImageAnalyzer()
        {
            IAzure azure = GetAzureContext(Environment.GetEnvironmentVariable(AzureLoginPath));
            DeleteContainerGroup(azure, ResourceGroupName, ContainerGroupName);
            DeleteResourceGroup(azure, ResourceGroupName);
        }

        private static void DeleteResourceGroup(IAzure azure, string resourceGroupName)
        {
            Logger.Log($"\nDeleting resource group '{resourceGroupName}'...");

            azure.ResourceGroups.DeleteByNameAsync(resourceGroupName);
        }

        private static void DeleteContainerGroup(IAzure azure, string resourceGroupName, string containerGroupName)
        {
            IContainerGroup containerGroup = null;

            while (containerGroup == null)
            {
                containerGroup = azure.ContainerGroups.GetByResourceGroup(resourceGroupName, containerGroupName);

                SdkContext.DelayProvider.Delay(1000);
            }

            Logger.Log($"Deleting container group '{containerGroupName}'...");

            azure.ContainerGroups.DeleteById(containerGroup.Id);
        }

        private static string RunTaskBasedContainer(IAzure azure,
                                                 string resourceGroupName,
                                                 string containerGroupName,
                                                 string containerImage,
                                                 string startCommandLine)
        {
            Logger.Log($"\nCreating container group '{containerGroupName}' with start command '{startCommandLine}'");

            IResourceGroup resGroup = azure.ResourceGroups.GetByName(resourceGroupName);
            Region azureRegion = resGroup.Region;

            var containerGroup = azure.ContainerGroups.Define(containerGroupName)
                .WithRegion(azureRegion)
                .WithExistingResourceGroup(resourceGroupName)
                .WithLinux()
                .WithPublicImageRegistryOnly()
                .WithoutVolume()
                .DefineContainerInstance(containerGroupName + "-1")
                    .WithImage(containerImage)
                    .WithExternalTcpPort(Port)
                    .WithCpuCoreCount(1.0)
                    .WithMemorySizeInGB(1)
                    .Attach()
                .WithDnsPrefix(containerGroupName)
                .WithRestartPolicy(ContainerGroupRestartPolicy.Never)
                .CreateAsync();

            return "";
        }

        private static void CreateResourceGroup(IAzure azure, string resourceGroupName, Region azureRegion)
        {
            Logger.Log($"\nCreating resource group '{resourceGroupName}'...");

            azure.ResourceGroups.Define(resourceGroupName)
                .WithRegion(azureRegion)
                .Create();
        }

        private static IAzure GetAzureContext(string authFilePath)
        {
            IAzure azure;
            ISubscription sub;

            try
            {
                Logger.Log($"Authenticating with Azure using credentials in file at {authFilePath}");

                azure = Azure.Authenticate(authFilePath).WithDefaultSubscription();
                sub = azure.GetCurrentSubscription();

                Logger.Log($"Authenticated with subscription '{sub.DisplayName}' (ID: {sub.SubscriptionId})");
            }
            catch (Exception ex)
            {
                Logger.Log($"\nFailed to authenticate:\n{ex.Message}");

                if (String.IsNullOrEmpty(authFilePath))
                {
                    Logger.Log("Have you set the AZURE_AUTH_LOCATION environment variable?");
                }

                throw;
            }

            return azure;
        }
    }
}
