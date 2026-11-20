using LabTrack.Application.Interfaces;
using LabTrack.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LabTrack.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<LabTrack.Application.Interfaces.IAiAssistantService, LabTrack.Application.Services.AiAssistantService>();
        services.AddScoped<LabTrack.Application.Interfaces.IAnalyticsService, LabTrack.Application.Services.AnalyticsService>();
        services.AddScoped<LabTrack.Application.Interfaces.IAssetService, LabTrack.Application.Services.AssetService>();
        services.AddScoped<LabTrack.Application.Interfaces.IAuditService, LabTrack.Application.Services.AuditService>();
        services.AddScoped<LabTrack.Application.Interfaces.IAuthService, LabTrack.Application.Services.AuthService>();
        services.AddScoped<LabTrack.Application.Interfaces.ICommunicationService, LabTrack.Application.Services.CommunicationService>();
        services.AddScoped<LabTrack.Application.Interfaces.IFinancialService, LabTrack.Application.Services.FinancialService>();
        // services.AddScoped<ILaboratoryService, LaboratoryService>();
        services.AddScoped<LabTrack.Application.Interfaces.IMaintenanceService, LabTrack.Application.Services.MaintenanceService>();
        services.AddScoped<LabTrack.Application.Interfaces.INetworkService, LabTrack.Application.Services.NetworkService>();
        services.AddScoped<LabTrack.Application.Interfaces.INotificationService, LabTrack.Application.Services.NotificationService>();
        services.AddScoped<LabTrack.Application.Interfaces.IPcUsageService, LabTrack.Application.Services.PcUsageService>();
        services.AddScoped<LabTrack.Application.Interfaces.IProblemReportService, LabTrack.Application.Services.ProblemReportService>();
        services.AddScoped<LabTrack.Application.Interfaces.IQrService, LabTrack.Application.Services.QrService>();
        services.AddScoped<LabTrack.Application.Interfaces.IRepairTicketService, LabTrack.Application.Services.RepairTicketService>();
        services.AddScoped<LabTrack.Application.Interfaces.IReplacementService, LabTrack.Application.Services.ReplacementService>();
        services.AddScoped<LabTrack.Application.Interfaces.IReservationService, LabTrack.Application.Services.ReservationService>();
        services.AddScoped<LabTrack.Application.Interfaces.IUserService, LabTrack.Application.Services.UserService>();

        return services;
    }
}
