using System;

namespace Hypergiant.HIVE
{
    public class StateVector
    {
        private double[] m_position = new double[3];
        private double[] m_velocity = new double[3];

        public DateTime Timestamp { get; set; }
        public FrameofReference FrameofReference { get; set; }
        public double X => Position[0];
        public double Y => Position[1];
        public double Z => Position[2];
        public double VX => Velocity[0];
        public double VY => Velocity[1];
        public double VZ => Velocity[2];

        public StateVector(DateTime timestamp, double[] position, double[] velocity)
        {
            Timestamp = timestamp;

            for (int i = 0; i < 3; i++)
            {
                Position[i] = position[i];
                Velocity[i] = velocity[i];
            }
        }

        public double[] Position
        {
            get => m_position;
        }
        public double[] Velocity
        {
            get => m_velocity;
        }

        public override string ToString()
        {
            return $"{Timestamp.ToString("MM/dd/yy HH:mm:ss.ff")}: [{X},{Y},{Z}] [{VX},{VY},{VZ}]";
        }
    }
}
