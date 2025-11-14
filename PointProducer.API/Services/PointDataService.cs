using PointProducer.API.Models;
using System;

namespace PointProducer.API.Services
{
    public class PointDataService
    {
        private readonly ILogger<PointDataService> _logger;
        private List<Point> _points = new List<Point>();
        public List<Point> GetAllPoints { get { return _points; } }

        public PointDataService(ILogger<PointDataService> logger)
        {
            _logger = logger;
            this.InitializedPoints();
        }

        public void UpdatePoint(int pointId, double x, double y)
        {
            var pointToUpdate = _points.FirstOrDefault(p => p.Id == pointId);
            if (pointToUpdate != null)
            {
                pointToUpdate.Geometry.Coordinates[0] = x;
                pointToUpdate.Geometry.Coordinates[1] = y;
            }
        }

        public void UpdatePoints(Point[] points)
        {
            foreach (var item in points)
            {
                var pointToUpdate = _points.FirstOrDefault(p => p.Id == item.Id);
                if (pointToUpdate != null)
                {
                    pointToUpdate.Geometry.Coordinates[0] = item.Geometry.Coordinates[0];
                    pointToUpdate.Geometry.Coordinates[1] = item.Geometry.Coordinates[1];
                }
            }
            
        }

        private void InitializedPoints()
        {
            _logger.LogInformation("Initializing points...");

            var random = new Random();
            for (int i = 1; i <= 100; i++)
            {
                double x = 29 + random.NextDouble() * 5; // 26-36
                double y = 38 + random.NextDouble() * 3;  // 36-43
                _points.Add(new Point
                {
                    Id = i,
                    Geometry = new Geometry
                    {
                        Type = "Point",
                        Coordinates = new[] { x, y }
                    },
                Properties = new Dictionary<string, string>()
                {
                    { "Id", $"{i}" },
                    { "TimeStamp", $"{DateTime.UtcNow.ToString("o")}" }
                }, 
                    Type = "Feature",
                });
            }
        }

    }
}
