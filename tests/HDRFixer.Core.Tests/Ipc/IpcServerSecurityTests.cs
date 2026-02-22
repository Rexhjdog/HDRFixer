using System.IO.Pipes;
using System.Security.Principal;
using HDRFixer.Core.Ipc;
using Xunit;

namespace HDRFixer.Core.Tests.Ipc;

public class IpcServerSecurityTests
{
    [Fact]
    public async Task ServerStartsWithSecureConfiguration()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        // Arrange
        using var server = new IpcServer();

        // Act
        server.Start();

        // Wait briefly for server to start listening
        await Task.Delay(500);

        try
        {
            // Assert: Verify we can connect as the current user (Authenticated User)
            // Note: This verifies that the ACLs allow the current user access.
            // It does not verify that others are blocked, which is hard to test without impersonation.
            using var client = new NamedPipeClientStream(".", "HDRFixer_Service", PipeDirection.InOut);
            await client.ConnectAsync(2000);
            Assert.True(client.IsConnected);
        }
        finally
        {
            server.Stop();
        }
    }
}
