using Edt.Application.Groups.Responses;
using Edt.Application.Services;
using Edt.Application.Timetables.Mappers;
using Edt.Domain.Groups;

namespace Edt.Application.Groups.Mappers;

public static class GroupMapper
{
    public static GroupResponse ToResponse(GroupEntity group) => new(group.ResourceId, group.Label, group.Parent);

    public static GetGroupTimetableResponse ToGetGroupTimetableResponse(
        GroupPlacement placement,
        string campusName,
        TimetableSnapshot timetable)
    {
        var (formation, year, group) = placement;
        var details = new GroupDetailsResponse(
            group.ResourceId,
            group.Label,
            group.Parent,
            formation.Id,
            formation.Name,
            year.Year,
            year.Label,
            formation.CampusId,
            campusName);

        return new GetGroupTimetableResponse(
            details,
            CourseMapper.ToResponses(timetable.Courses),
            timetable.FetchedAt,
            timetable.IsStale);
    }
}
