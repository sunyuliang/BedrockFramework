﻿using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;

namespace Bedrock.Framework;

public class Client(IConnectionFactory connectionFactory, ConnectionDelegate application) : IConnectionFactory
{
    public async ValueTask<ConnectionContext> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
    {
        var connection = await connectionFactory.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);

        // Since nothing is being returned from this middleware, we need to wait for the last middleware to run
        // until we yield this call. Stash a tcs in the items bag that allows this code to get notified
        // when the middleware ran
        var connectionContextWithDelegate = new ConnectionContextWithDelegate(connection, application);

        // Execute the middleware pipeline
        connectionContextWithDelegate.Start();

        // Wait for it the most inner middleware to run
        return await connectionContextWithDelegate.Initialized.Task.ConfigureAwait(false);
    }
}
