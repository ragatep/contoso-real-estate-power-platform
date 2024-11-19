// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.Xrm.Sdk;
using Moq;
using ContosoRealEstate.BusinessLogic.Services;
using System.Collections.Generic;
using ContosoRealEstate.BusinessLogic.Plugins;
using System;
using Microsoft.Xrm.Sdk.PluginTelemetry;

namespace ContosoRealEstateBusinessLogic.Tests.UnitTests;

public class PluginUnitTestBase
{
    protected (ILocalPluginContext mockLocalPluginContext, Mock<IOrganizationService> mockOrganizationService) SetupMockLocalPluginContext(string messageName, StageEnum stage, Entity target = null, Entity preImage = null, bool UsePowerFxPlugins = false)
    {
        // Create a Service Provider 
        var serviceContainer = new ServiceIOCContainer();

        // Mock the EnvironmentService
        var mockEnvironmentVariableService = new Mock<IEnvironmentVariableService>();
        mockEnvironmentVariableService.Setup(x => x.RetrieveEnvironmentVariableValue("contoso_FCB_UsePowerFxPlugins"))
            .Returns(UsePowerFxPlugins ? "yes" : "no");
        serviceContainer.Register<IEnvironmentVariableService, IEnvironmentVariableService>(mockEnvironmentVariableService.Object);

        // Mock the OrganizationService
        var mockOrganizationService = new Mock<IOrganizationService>();
        serviceContainer.Register<IOrganizationService, IOrganizationService>(mockOrganizationService.Object);

        // Mock the Execution Context
        var mockPluginExecutionContext = new Mock<IPluginExecutionContext>();
        serviceContainer.Register<IPluginExecutionContext, IPluginExecutionContext>(mockPluginExecutionContext.Object);
        mockPluginExecutionContext.Setup(context => context.OutputParameters)
            .Returns(new ParameterCollection());
        mockPluginExecutionContext.Setup(context => context.InputParameters)
            .Returns(new ParameterCollection());

        // Mock the tracing service
        var executionContext = new Mock<IExecutionContext>();
        executionContext.Setup(context => context.OperationCreatedOn).Returns(DateTime.Now);
        serviceContainer.Register<IExecutionContext, IExecutionContext>(executionContext.Object);
        serviceContainer.Register<ITracingService, ITracingService>(new Mock<ITracingService>().Object);
        serviceContainer.Register<ILogger, ILogger>(new Mock<ILogger>().Object);

        // Mock the preimage if needed
        if (preImage is not null) {
            mockPluginExecutionContext.Setup(context => context.PreEntityImages).Returns(new EntityImageCollection
            {
                { "PreImage", preImage.ToEntity<Entity>() }
            });
        };

        if (target is not null)
        {
            mockPluginExecutionContext.Object.InputParameters["Target"] = target.ToEntity<Entity>();
        }
        mockPluginExecutionContext.Setup(context => context.MessageName).Returns(messageName);
        mockPluginExecutionContext.Setup(context => context.Stage).Returns((int)stage);
        var localPluginContext = new LocalPluginContext(serviceContainer);
        return (localPluginContext, mockOrganizationService);
    }

    // Helper method to create mock reservation entity
    public static Entity CreateReservationEntity(Guid listingId, DateTime from, DateTime to)
    {
        var reservation = new Entity("contoso_Reservation")
        {
            Id = Guid.NewGuid()
        };
        reservation["contoso_From"] = from;
        reservation["contoso_To"] = to;
        reservation["contoso_Listing"] = new EntityReference("contoso_listing", listingId);

        return reservation;
    }
}
