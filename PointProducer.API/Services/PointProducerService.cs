
using PointProducer.API.Models;
using System;
using Point = PointProducer.API.Models.Point;

namespace PointProducer.API.Services
{
    public class PointProducerService : IHostedService, IDisposable
    {
        private readonly ILogger<PointProducerService> _logger;
        private readonly PointPublisherService _pointPublisherService;
        private readonly PointDataService _pointDataService;
        private Timer? _timer;

        public PointProducerService(
            ILogger<PointProducerService> logger,
            PointPublisherService pointPublisherService,
            PointDataService pointDataService)
        {
            _logger = logger;
            _pointPublisherService = pointPublisherService;
            _pointDataService = pointDataService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Point Producer Service Started.");

            _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            var updated = new List<Point>();
            var _points = this._pointDataService.GetAllPoints; 
            var random = new Random();
            var updateCount = random.Next(1, 11);

            _logger.LogInformation($"Updating {updateCount} random points.");

            var shuffledPoints = _points.OrderBy(p => random.Next()).Take(updateCount).ToArray();

            foreach (var point in shuffledPoints)
            {
                var deltaX = NextGaussian(random, 0, 0.01);
                var deltaY = NextGaussian(random, 0, 0.01);

                var newX = point.Geometry.Coordinates[0] + deltaX;
                var newY = point.Geometry.Coordinates[1] + deltaY;

                point.Geometry.Coordinates[0] = newX;
                point.Geometry.Coordinates[1] = newY;

                _logger.LogInformation("Publishing a new point: PointId={PointId}, X={X}, Y={Y}", point.Id, newX, newY);
            }
            try
            {
                _pointPublisherService.PublishPointChangeAsync(shuffledPoints).GetAwaiter().GetResult();
                _pointDataService.UpdatePoints(shuffledPoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish point change message.");
            }

        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Point Producer Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0); // Timer'ı durdur
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private double NextGaussian(Random random, double mean = 0, double stdDev = 0.0008)
        {
            // Box–Muller transform
            double u1 = 1.0 - random.NextDouble();
            double u2 = 1.0 - random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                   Math.Sin(2.0 * Math.PI * u2);
            return mean + stdDev * randStdNormal;
        }
    }
}
