using Tfl.Api.Presentation.Entities;
using System.Linq;

namespace p.route.service;

public readonly record struct StopPointId(string Id)
{
    public static readonly StopPointId ArnosGrove = new StopPointId("940GZZLUASG");
    public static readonly StopPointId CockFosters = new StopPointId("940GZZLUCKS");
    public static readonly StopPointId AbbeyWood = new StopPointId("910GABWDXR");
    public static readonly StopPointId Shenfield = new StopPointId("910GSHENFLD");
    public static readonly StopPointId WoodGreen = new StopPointId("940GZZLUWOG");

    public static readonly StopPointId NorthfieldsTube = new StopPointId("940GZZLUNFD");
    public static readonly StopPointId WestEaling = new StopPointId("910GWEALING");
    public static readonly StopPointId EalingBroadway = new StopPointId("910GEALINGB");

    public static readonly StopPointId BostonManor = new StopPointId("940GZZLUBOS");
}

public readonly record struct LineId(string Name)
{
    public static readonly LineId Piccadilly = new LineId("piccadilly");
    public static readonly LineId ElizabethLine = new LineId("elizabeth");
}

public sealed class WalkingRoute : Route
{
    public TimeSpan Duration { get; }
    public WalkingRoute(TimeSpan duration)
    {
        Duration = duration;
    }

    public override Task<(string VehicleId, DateTimeOffset ExpectedArrival)[]> NextDeparturesAfter(HttpClient client, DateTimeOffset time)
    {
        var response = (VehicleId: "Foot", ExpectedArrival: time);
        return Task.FromResult(new [] { response });
    }
}

public static class RouteFactory
{
    public static readonly Route WalkHomeToNorthfieldBusStation = new WalkingRoute(TimeSpan.FromMinutes(5));

    public static readonly Route WalkHomeToWestEaling = new WalkingRoute(TimeSpan.FromMinutes(25));
    public static readonly Route BusNorthfieldsToWestEaling;
    public static readonly Route ElizabethLineWestEalingToWork = new TubeRoute(
        StopPointId.WestEaling,
        LineId.ElizabethLine,
        StopPointId.AbbeyWood,
        StopPointId.Shenfield
        );

    public static readonly Route WalkHomeToEalingBroadway = new WalkingRoute(TimeSpan.FromMinutes(35));
    public static readonly Route BusNorthfieldsToEalingBroadway;
    public static readonly Route ElizabethLineEalingBroadwayToWork = new TubeRoute(
        StopPointId.EalingBroadway,
        LineId.ElizabethLine,
        StopPointId.AbbeyWood,
        StopPointId.Shenfield
        );

    public static readonly Route WalkHomeToNorthfieldsTube = new WalkingRoute(TimeSpan.FromMinutes(7));
    public static readonly Route TubeNorthfieldsToWork = new TubeRoute(
        StopPointId.NorthfieldsTube, 
        LineId.Piccadilly, 
        StopPointId.ArnosGrove,
        StopPointId.CockFosters,
        StopPointId.WoodGreen
        );

    public static readonly Route TubeBostonManorToWork = new TubeRoute(
        StopPointId.BostonManor,
        LineId.Piccadilly,
        StopPointId.ArnosGrove,
        StopPointId.CockFosters,
        StopPointId.WoodGreen
        );
}

public class TubeRoute : Route
{
    public StopPointId From { get; }
    public LineId LineId { get; }
    public List<StopPointId> AcceptableDestinations { get; }

    public TubeRoute(StopPointId from, LineId lineId, StopPointId acceptableDestination, params StopPointId[] acceptableDestinations)
    {
        From = from;
        LineId = lineId;
        AcceptableDestinations = new[] { acceptableDestination }.Concat(acceptableDestinations).ToList();
    }

    public override async Task<(string VehicleId, DateTimeOffset ExpectedArrival)[]> NextDeparturesAfter(HttpClient client, DateTimeOffset time)
    {
        var sp = new StopPointClient(client);
        var arrivals = await sp.ArrivalsAsync(From.Id);
        if (arrivals is null)
            return [];

        return arrivals
            .Where(a => a.LineId == LineId.Name)
            .Where(a => AcceptableDestinations.Contains(new StopPointId(a.DestinationNaptanId)))
            .DistinctBy(a => a.VehicleId)
            .Where(a => a.ExpectedArrival >= time)
            .Select(a => (VehicleId: a.VehicleId, ExpectedArrival: (DateTimeOffset)a.ExpectedArrival!))
            .OrderBy(a => a.ExpectedArrival)
            .ToArray();
    }
}

public abstract class Route
{
    public abstract Task<(string VehicleId, DateTimeOffset ExpectedArrival)[]> NextDeparturesAfter(HttpClient client, DateTimeOffset time);
}

