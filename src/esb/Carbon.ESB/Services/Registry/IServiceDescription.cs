using System;

namespace Kharbon.Registries.For.BackgroundServices
{
    public interface IServiceDescription
    {
        string Name { get;}
        Type Contract { get;  }
        Type Service { get; }
    }
}