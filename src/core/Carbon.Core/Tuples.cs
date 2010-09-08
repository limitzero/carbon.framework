using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Carbon.Core
{
    public class Tuple<TItem1, TItem2>
    {
        public TItem1 Item1 { get; private set; }
        public TItem2 Item2 { get; private set; }

        public Tuple(TItem1 item1, TItem2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }

    public class Tuple<TItem1, TItem2, TItem3>
    {
        public TItem1 Item1 { get; private set; }
        public TItem2 Item2 { get; private set; }
        public TItem3 Item3 { get; private set; }

        public Tuple(TItem1 item1, TItem2 item2,TItem3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }
    }

    public class Tuple<TItem1, TItem2, TItem3, TItem4>
    {
        public TItem1 Item1 { get; private set; }
        public TItem2 Item2 { get; private set; }
        public TItem3 Item3 { get; private set; }
        public TItem4 Item4 { get; private set; }

        public Tuple(TItem1 item1, TItem2 item2, TItem3 item3, TItem4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }
    }
}