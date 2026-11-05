using Edt.Application.Campuses.UseCases;
using Edt.Application.Groups.UseCases;
using Edt.Application.Rooms.UseCases;
using Edt.Application.Timetables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Edt.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, TimetableOptions options)
    {
        services.AddSingleton(options);
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<IGetCampusesUseCase, GetCampusesUseCase>();
        services.AddScoped<IGetCampusByIdUseCase, GetCampusByIdUseCase>();
        services.AddScoped<IGetGroupTimetableUseCase, GetGroupTimetableUseCase>();
        services.AddScoped<IExportGroupCalendarUseCase, ExportGroupCalendarUseCase>();
        services.AddScoped<IGetRoomTimetableUseCase, GetRoomTimetableUseCase>();
        services.AddScoped<IGetRoomsStatusUseCase, GetRoomsStatusUseCase>();

        return services;
    }
}
