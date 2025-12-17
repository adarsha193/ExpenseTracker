using System;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Services;

/// <summary>
/// Minimal service locator that holds the application's <see cref="IServiceProvider"/>.
/// The provider is expected to be assigned during app startup (see <see cref="MauiProgram"/>).
/// This helper is a pragmatic bridge for parts of the app that cannot yet use
/// constructor injection. For reviewer clarity: it intentionally does NOT create
/// the Maui app itself — that would hide lifecycle issues and make behavior surprising.
/// </summary>
public static class ServiceLocator
{
    public static IServiceProvider? Provider { get; set; }

    public static T GetRequiredService<T>() where T : notnull
    {
        if (Provider == null)
            throw new InvalidOperationException("ServiceLocator.Provider is not initialized. Ensure MauiProgram sets ServiceLocator.Provider after building the app and prefer constructor DI.");

        return Provider.GetRequiredService<T>();
    }
}
