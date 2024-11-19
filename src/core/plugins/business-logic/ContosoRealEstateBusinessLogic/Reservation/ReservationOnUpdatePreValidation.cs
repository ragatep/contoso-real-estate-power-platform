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
[CrmPluginRegistration(
    MessageNameEnum.Update,
    contoso_Reservation.EntityLogicalName,
    StageEnum.PreValidation,
    ExecutionModeEnum.Synchronous,
    "contoso_from,contoso_to,contoso_listing",
    "ReservationOnUpdatePreValidation",
    1000,
    IsolationModeEnum.Sandbox,
    Image1Name = "PreImage",
    Image1Type = ImageTypeEnum.PreImage,
    Image1Attributes =
        contoso_Reservation.Fields.contoso_From + "," +
        contoso_Reservation.Fields.contoso_To + "," +
        contoso_Reservation.Fields.contoso_Listing
)]
public class ReservationOnUpdatePreValidation : ReservationLogic, IPlugin
{
    public ReservationOnUpdatePreValidation(string unsecureConfiguration, string secureConfiguration)
        : base(typeof(ReservationOnUpdatePreValidation), unsecureConfiguration, secureConfiguration)
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

        ValidatePluginExecutionContext(localPluginContext, MessageNameEnum.Update, StageEnum.PreValidation, contoso_Reservation.EntityLogicalName);
        CheckForReadOnlyFields(localPluginContext);
        ValidateFields(localPluginContext);
        IsSelectedListingAvailable(localPluginContext);
        SetNameField(localPluginContext);
    }

    
}
