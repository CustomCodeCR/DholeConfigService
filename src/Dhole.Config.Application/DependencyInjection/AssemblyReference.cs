using System.Reflection;

namespace Dhole.Config.Application.DependencyInjection;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
