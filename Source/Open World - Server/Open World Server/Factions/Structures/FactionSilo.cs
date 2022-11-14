﻿using System;
using System.Collections.Generic;
using System.Text;

namespace OpenWorldServer
{
    public class FactionSilo : FactionStructure
    {
        public override Faction holdingFaction => base.holdingFaction;

        public override string structureName => "Silo";

        public override int structureType => (int)StructureType.Silo;

        public override int structureTile => base.structureTile;

        public Dictionary<int, List<string>> holdingItems = new Dictionary<int, List<string>>();

        public FactionSilo(Faction holdingFaction, int structureTile)
        {
            this.holdingFaction = holdingFaction;
            this.structureTile = structureTile;
        }
    }
}
