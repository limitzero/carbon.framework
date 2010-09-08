using System;
using System.Collections.Generic;
using System.Text;

namespace Carbon.Integration.Stereotypes.Accumulator.Impl
{
    public delegate bool AccumulationCompleteStrategy<T>(T message) where T : class;
}