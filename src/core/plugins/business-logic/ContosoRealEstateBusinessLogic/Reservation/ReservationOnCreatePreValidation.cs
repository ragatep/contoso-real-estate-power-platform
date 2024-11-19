// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using ContosoRealEstate.BusinessLogic.Models;
using Microsoft.Xrm.Sdk;
using System;

namespace ContosoRealEstate.BusinessLogic.Plugins;

/// <summary>
/// Plugin development guide: https://docs.microsoft.com/powerapps/developer/common-data-service/plug-ins
/// Best practices and guidance: https://docs.microsoft.com/powerapps/developer/common-data-service/best-practices/business-logic/
/// </summary>
[CrmPluginRegistration(MessageNameEnum.Create, contoso_Reservation.EntityLogicalName, StageEnum.PreValidation, ExecutionModeEnum.Synchronous,
    "contoso_from,contoso_to,contoso_listing,contoso_nights", 
    "ReservationOnCreatePreValidation", 1000, IsolationModeEnum.Sandbox)]
public class ReservationOnCreatePreValidation : ReservationLogic, IPlugin
{
    public ReservationOnCreatePreValidation(string unsecureConfiguration, string secureConfiguration)
        : base(typeof(ReservationOnCreatePreValidation), unsecureConfiguration, secureConfiguration)
    {
    }

    // Entry point for custom business logic execution
    public override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
    {
        if (localPluginContext == null)
        {
            throw new ArgumentNullException(nameof(localPluginContext));
        }

        if (UsePowerFxPlugins(localPluginContext)) return;

        ValidatePluginExecutionContext(localPluginContext, MessageNameEnum.Create, StageEnum.PreValidation, contoso_Reservation.EntityLogicalName);
        IsSelectedListingAvailable(localPluginContext);
        ValidateFields(localPluginContext);
        IsSelectedListingAvailable(localPluginContext);
        SetNameField(localPluginContext);
        SetReservationDateOnCreate(localPluginContext);
        
    }
}
