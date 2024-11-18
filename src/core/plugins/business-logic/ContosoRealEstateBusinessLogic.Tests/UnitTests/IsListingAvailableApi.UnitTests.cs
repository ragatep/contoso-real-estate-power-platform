// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using ContosoRealEstate.BusinessLogic.Resources;
using ContosoRealEstate.BusinessLogic.Models;
using ContosoRealEstate.BusinessLogic.Plugins;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk.Extensions;

namespace ContosoRealEstateBusinessLogic.Tests.UnitTests
{
    [TestClass]
    public class IsListingAvailableApiTests : PluginUnitTestBase
    {
        [TestMethod]
        public void ExecuteDataversePlugin_ShouldThrowInvalidPluginExecutionException_WhenInputParametersAreMissing()
        {
            // Arrange
            var (mockLocalPluginContext, mockOrganizationService) = SetupMockLocalPluginContext("contoso_IsListingAvailableAPI", StageEnum.PlatformOperation);
            mockLocalPluginContext.PluginExecutionContext.InputParameters["From"] = "2023-10-01";
            mockLocalPluginContext.PluginExecutionContext.InputParameters["To"] = "2023-10-10";
            mockLocalPluginContext.PluginExecutionContext.InputParameters["ExcludeReservation"] = Guid.Empty.ToString();

            var plugin = new IsListingAvailable(null, null);

            // Act
            void executePlugin() => plugin.ExecuteDataversePlugin(mockLocalPluginContext);

            // Assert
            var expectedException = Assert.ThrowsException<MissingInputParametersException>(executePlugin);
        }

        [TestMethod]
        public void ExecuteDataversePlugin_ShouldSetAvailableToTrue_WhenListingIsAvailable()
        {
            // Arrange
            var (mockLocalPluginContext, mockOrganizationService) = SetupMockLocalPluginContext("contoso_IsListingAvailableAPI", StageEnum.PlatformOperation);
            mockLocalPluginContext.PluginExecutionContext.InputParameters["FromDate"] = new DateTime(2023, 10, 1);
            mockLocalPluginContext.PluginExecutionContext.InputParameters["ToDate"] = new DateTime(2023, 10, 10);
            mockLocalPluginContext.PluginExecutionContext.InputParameters["ExcludeReservation"] = Guid.Empty.ToString();
            mockLocalPluginContext.PluginExecutionContext.InputParameters["ListingID"] = Guid.NewGuid().ToString();

            mockOrganizationService.Setup(x => x.RetrieveMultiple(It.IsAny<QueryBase>()))
                .Returns(new EntityCollection());

            var plugin = new IsListingAvailable(null, null);

            // Act
            plugin.ExecuteDataversePlugin(mockLocalPluginContext);

            // Assert
            Assert.IsTrue((bool)mockLocalPluginContext.PluginExecutionContext.OutputParameters["Available"]);
        }

        [TestMethod]
        public void ExecuteDataversePlugin_ShouldSetAvailableToFalse_WhenListingIsNotAvailable()
        {
            var (mockLocalPluginContext, mockOrganizationService) = SetupMockLocalPluginContext("contoso_IsListingAvailableAPI", StageEnum.PlatformOperation);
            mockLocalPluginContext.PluginExecutionContext.InputParameters["FromDate"] = new DateTime(2023, 10, 1);
            mockLocalPluginContext.PluginExecutionContext.InputParameters["ToDate"] = new DateTime(2023, 10, 10);
            mockLocalPluginContext.PluginExecutionContext.InputParameters["ExcludeReservation"] = Guid.Empty.ToString();
            mockLocalPluginContext.PluginExecutionContext.InputParameters["ListingID"] = Guid.NewGuid().ToString();

            var reservation = new Entity("contoso_Reservation")
            {
                Id = Guid.NewGuid()
            };
            reservation["contoso_From"] = DateTime.Parse("2023-10-05");
            reservation["contoso_To"] = DateTime.Parse("2023-10-15");
            reservation["contoso_Listing"] = new EntityReference("contoso_listing", Guid.NewGuid());

            mockOrganizationService.Setup(x => x.RetrieveMultiple(It.IsAny<QueryBase>()))
                .Returns(new EntityCollection(new List<Entity> { reservation }));

            var plugin = new IsListingAvailable(null, null);

            // Act
            plugin.ExecuteDataversePlugin(mockLocalPluginContext);

            // Assert
            Assert.IsFalse((bool)mockLocalPluginContext.PluginExecutionContext.OutputParameters["Available"]);
        }

    }

}
