using System;

namespace Hypergiant.HIVE
{
    public class Attitude
    {
        public DateTime? Timestamp { get; set; }
        public FrameofReference FrameofReference { get; set; }
        public double Roll { get; set; }
        public double Pitch { get; set; }
        public double Yaw { get; set; }

        public override string ToString()
        {
            return $"YPR:{Yaw:0.0} {Pitch:0.0} {Roll:0.0}";
        }
    }
}
