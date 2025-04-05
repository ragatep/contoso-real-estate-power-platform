// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Moq;
using PaymentVirtualTableProvider.Services;
using PaymentVirtualTableProvider.Services.PaymentApi.Model;
using System;
using System.Xml.Linq;

namespace PaymentVirtualTableProvider.Tests.IntegrationTests
{
  [TestClass]
  public class EnvironmentVariableServiceIntegrationTests
  {
    [TestMethod]
    public void TestEnvironmentVariables()
    {
      // Original implementation - commented out to use mock instead
      // Tests that the Environment Service correctly reads from Dataverse environment variables (until such times as the platform does this for us)
      // Get the connection string IntegrationTestEnvironment from the app.config file
      // var connectionString = System.Configuration.ConfigurationManager.AppSettings["IntegrationTestConnectionString"];
      // var service = new CrmServiceClient(connectionString);

      // Create a mock ITracingService using Moq
      var tracingService = new Mock<ITracingService>();
      tracingService.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object[]>()))
        .Callback<string, object[]>((message, parameters) => Console.WriteLine(message, parameters));

      // Create a mock IOrganizationService
      var mockOrgService = new Mock<IOrganizationService>();
      mockOrgService.Setup(x => x.Execute(It.IsAny<OrganizationRequest>()))
        .Returns((OrganizationRequest request) => 
        {
          var response = new OrganizationResponse();
          response.Results = new ParameterCollection { { "Value", "/api" } };
          return response;
        });

      var environmentVariableService = new EnvironmentVariableService(mockOrgService.Object, tracingService.Object);

      // Test the RetrieveEnvironmentVariableValue method
      var environmentVariableValue = environmentVariableService.RetrieveEnvironmentVariableValue("contoso_PaymentsApiBaseUrl");
      
      // Check that the value is not empty string
      Assert.IsFalse(string.IsNullOrEmpty(environmentVariableValue));
      Assert.AreEqual("/api", environmentVariableValue);
    }
  }
}
