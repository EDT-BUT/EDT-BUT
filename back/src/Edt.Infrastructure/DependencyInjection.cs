using Edt.Application.Providers;
using Edt.Application.Services;
using Edt.Domain.Campuses;
using Edt.Domain.Formations;
using Edt.Domain.Groups;
using Edt.Domain.Rooms;
using Edt.Domain.Timetables;
using Edt.Infrastructure.DataBase;
using Edt.Infrastructure.HttpClients.EdtReader;
using Edt.Infrastructure.Providers;
using Edt.Infrastructure.Repositories;
using Edt.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Edt.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string contentRootPath)
    {
        var catalogPath = configuration["Catalog:Path"] ?? "Data/catalog.json";
        services.AddSingleton(CatalogContext.Load(Path.Combine(contentRootPath, catalogPath)));

        services.AddSingleton<ICampusRepository, CampusRepository>();
        services.AddSingleton<IFormationRepository, FormationRepository>();
        services.AddSingleton<IGroupRepository, GroupRepository>();
        services.AddSingleton<IRoomRepository, RoomRepository>();
        services.AddSingleton<ITimetableRepository, InMemoryTimetableRepository>();

        var edtReader = configuration.GetSection(EdtReaderOptions.SectionName).Get<EdtReaderOptions>() ?? new();
        if (!Uri.TryCreate(edtReader.BaseUrl, UriKind.Absolute, out var baseAddress))
        {
            throw new InvalidOperationException(
                $"{EdtReaderOptions.SectionName}:BaseUrl manque dans la configuration (appsettings.json).");
        }

        services.AddSingleton(edtReader);
        services.AddHttpClient(EdtReaderHttpClient.Name, client =>
        {
            client.BaseAddress = baseAddress;
            client.Timeout = edtReader.Timeout;
            client.DefaultRequestHeaders.UserAgent.ParseAdd("EdtUnicaen-prototype/0.1 (projet etudiant)");
        });
        services.AddSingleton<EdtReaderHttpClient>();

        services.AddSingleton<ICourseProvider, EdtReaderCourseProvider>();

        services.AddSingleton<ITimetableService, TimetableService>();
        services.AddSingleton<ICalendarService, IcsCalendarService>();

        return services;
    }
}
