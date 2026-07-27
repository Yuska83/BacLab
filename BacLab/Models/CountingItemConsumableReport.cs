using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BacLab.Models
{
    internal class CountingItemConsumableReport
    {
        public d_ConsumablesStock Stock { get; }
        public double? QtyAtStart { get; }
        public double? Received { get; }
        public double? WrittenOff { get; }
        public double? QtyAtEnd { get; }

        public CountingItemConsumableReport(d_ConsumablesStock stock, double? qtyAtStart, double? received, double? writtenOff, double? qtyAtEnd)
        {
            Stock = stock;
            QtyAtStart = qtyAtStart;
            Received = received;
            WrittenOff = writtenOff;
            QtyAtEnd = qtyAtEnd;
        }

        public override bool Equals(object obj)
        {
            return obj is CountingItemConsumableReport other &&
                   EqualityComparer<d_ConsumablesStock>.Default.Equals(Stock, other.Stock) &&
                   QtyAtStart == other.QtyAtStart &&
                   Received == other.Received &&
                   WrittenOff == other.WrittenOff &&
                   QtyAtEnd == other.QtyAtEnd;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Stock, QtyAtStart, Received, WrittenOff, QtyAtEnd);
        }
    }
}
