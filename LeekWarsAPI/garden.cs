using System;
using System.Collections.Generic;
using System.Linq;

namespace LeekWarsAPI
{
    public class Garden
    {
        public int Fight;
        public List<Leek> Opponents;
        
        public Garden()
        {}

        public Leek GetWeakestOpponent()
        {
            Opponents = Opponents.OrderBy(o => o.Level).ThenBy(o => o.Talent).ToList();
            return Opponents[0];
        }
    }
}