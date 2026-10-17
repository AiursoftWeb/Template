using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Options;

namespace Aiursoft.Template.Tests.IntegrationTests;

[TestClass]
public class TestPortReservationTests
{
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(5) };

    [TestMethod]
    public void OccupiedPortFallsBackToAnotherReservedPort()
    {
        using var occupied = new TcpListener(IPAddress.Any, 0);
        occupied.Start();
        var port = ((IPEndPoint)occupied.LocalEndpoint).Port;
        using var reservation = new TestPortReservation(port);
        Assert.AreNotEqual(port, reservation.Port);
        AssertPortIsHeld(reservation.Port);
    }

    [TestMethod]
    public void DisposeReleasesAnUntransferredSocket()
    {
        var reservation = new TestPortReservation();
        try
        {
            AssertPortIsHeld(reservation.Port);
            reservation.Dispose();
            reservation.Dispose();
            using var listener = new TcpListener(IPAddress.Any, reservation.Port);
            listener.Start();
        }
        finally
        {
            reservation.Dispose();
        }
    }

    [TestMethod]
    public async Task KestrelAdoptsTheHeldSocketWithoutRebinding()
    {
        var reservation = new TestPortReservation();
        Socket? transferredSocket = null;
        var port = reservation.Port;
        try
        {
            var server = BuildServer(reservation);
            try
            {
                var options = server.Services.GetRequiredService<IOptions<SocketTransportOptions>>().Value;
                var transfer = options.CreateBoundListenSocket;
                options.CreateBoundListenSocket = endpoint =>
                {
                    AssertPortIsHeld(port);
                    transferredSocket = transfer(endpoint);
                    AssertPortIsHeld(port);
                    return transferredSocket;
                };

                await server.StartAsync();
                reservation.Dispose();
                Assert.IsNotNull(transferredSocket);
                Assert.IsFalse(transferredSocket.SafeHandle.IsClosed);
                AssertPortIsHeld(port);
                Assert.AreEqual("ok", await Client.GetStringAsync($"http://127.0.0.1:{port}/"));
                await server.StopAsync();
            }
            finally
            {
                await server.DisposeAsync();
            }
            Assert.IsNotNull(transferredSocket);
            Assert.IsTrue(transferredSocket.SafeHandle.IsClosed);
        }
        finally
        {
            reservation.Dispose();
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task StartupFailureReleasesTheSocket(bool afterTransfer)
    {
        var reservation = new TestPortReservation();
        try
        {
            var server = BuildServer(reservation, afterTransfer);
            try
            {
                await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => server.StartAsync());
            }
            finally
            {
                await server.DisposeAsync();
            }
        }
        finally
        {
            reservation.Dispose();
        }
        using var listener = new TcpListener(IPAddress.Any, reservation.Port);
        listener.Start();
    }

    private static WebApplication BuildServer(TestPortReservation reservation, bool? failAfterTransfer = null)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
        {
            Args = [],
            ContentRootPath = AppContext.BaseDirectory
        });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls($"http://0.0.0.0:{reservation.Port}");
        if (failAfterTransfer.HasValue)
        {
            builder.Services.AddSingleton<IHostedService>(new StartupFailure(failAfterTransfer.Value));
        }
        var server = builder.Build();
        reservation.ConfigureServer(server);
        server.MapGet("/", () => "ok");
        return server;
    }

    private static void AssertPortIsHeld(int port)
    {
        using var contender = new TcpListener(IPAddress.Any, port);
        try
        {
            contender.Start();
            Assert.Fail("Another listener acquired the reserved port.");
        }
        catch (SocketException exception)
        {
            Assert.AreEqual(SocketError.AddressAlreadyInUse, exception.SocketErrorCode);
        }
    }

    private sealed class StartupFailure(bool afterTransfer) : IHostedLifecycleService
    {
        public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StartAsync(CancellationToken cancellationToken) => afterTransfer
            ? Task.CompletedTask
            : Task.FromException(new InvalidOperationException("Deliberate startup failure."));
        public Task StartedAsync(CancellationToken cancellationToken) => afterTransfer
            ? Task.FromException(new InvalidOperationException("Deliberate startup failure."))
            : Task.CompletedTask;
        public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
