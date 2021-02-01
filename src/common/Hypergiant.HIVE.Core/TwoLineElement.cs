using System;

namespace Hypergiant.HIVE
{
    /// <summary>
    /// Encapsulates the NORAD Two-Line Element set
    /// </summary>
    public class TwoLineElement
    {
        // https://celestrak.com/NORAD/documentation/tle-fmt.php

        /*

        Line 1
        Column	Description
        01	    Line Number of Element Data
        03-07	Satellite Number
        08	    Classification (U=Unclassified)
        10-11	International Designator (Last two digits of launch year)
        12-14	International Designator (Launch number of the year)
        15-17	International Designator (Piece of the launch)
        19-20	Epoch Year (Last two digits of year)
        21-32	Epoch (Day of the year and fractional portion of the day)
        34-43	First Time Derivative of the Mean Motion
        45-52	Second Time Derivative of Mean Motion (Leading decimal point assumed)
        54-61	BSTAR drag term (Leading decimal point assumed)
        63	    Ephemeris type
        65-68	Element number
        69	    Checksum (Modulo 10) (Letters, blanks, periods, plus signs = 0; minus signs = 1)

        Line 2
        Column	Description
        01	    Line Number of Element Data
        03-07	Satellite Number
        09-16	Inclination [Degrees]
        18-25	Right Ascension of the Ascending Node [Degrees]
        27-33	Eccentricity (Leading decimal point assumed)
        35-42	Argument of Perigee [Degrees]
        44-51	Mean Anomaly [Degrees]
        53-63	Mean Motion [Revs per day]
        64-68	Revolution number at epoch [Revs]
        69	    Checksum (Modulo 10)

        Example:
        NOAA 14
                 1         2         3         4         5         6          
        123456789012345678901234567890123456789012345678901234567890123456789
        ---------------------------------------------------------------------
        1 23455U 94089A   97320.90946019  .00000140  00000-0  10191-3 0  2621
        2 23455  99.0090 272.6745 0008546 223.1686 136.8816 14.11711747148495

        */
        public TwoLineElement(string tle)
        {
            var elements = tle.Split("\n");
            Parse(elements[0], elements[1]);
        }

        public TwoLineElement(string line1, string line2)
        {
            Parse(line1, line2);
        }

        private void Parse(string line1, string line2)
        {
            if (line1 == null || line2 == null)
            {
                throw new ArgumentNullException();
            }

            if (line1.Length != 69)
            {
                throw new ArgumentException("Invalid TLE data: Line 1 invalid length");
            }
            if (line2.Length != 69)
            {
                throw new ArgumentException("Invalid TLE data: Line 2 invalid length");
            }

            // line numbers must match
            if (line1[0] != '1')
            {
                throw new ArgumentException("Invalid TLE data: Invalid start of line 1");
            }
            if (line2[0] != '2')
            {
                throw new ArgumentException("Invalid TLE data: Invalid start of line 2");
            }

            Line1 = line1;
            Line2 = line2;

            // parse line 1
            SatelliteNumber = int.Parse(line1.Substring(2, 5));
            // satellite numbers must match
            var satnum2 = int.Parse(line2.Substring(2, 5));
            if (SatelliteNumber != satnum2)
            {
                throw new ArgumentException("Invalid TLE data: Satellite numbers do not match");
            }

            Classification = line1[7];
            LaunchYear = int.Parse(line1.Substring(9, 2));
            LaunchNumber = int.Parse(line1.Substring(11, 3));
            LaunchPiece = line1.Substring(14, 3).Trim();
            var year = int.Parse(line1.Substring(18, 2));
            year += (year < 57) ? 2000 : 1900;
            var epochDay = double.Parse(line1.Substring(20, 12));
            Timestamp = new DateTime(year, 1, 1).AddDays(epochDay - 1);
            FirstTimeDerivative = float.Parse(line1.Substring(33, 10));
            var secondTimeVal = line1.Substring(44, 6).Trim();
            var secondTimePrefix = secondTimeVal[0] == '-' ? "-" : "";
            SecondTimeDerivative =
                double.Parse($"{secondTimePrefix}0.{secondTimeVal.Trim('-')}")
                * Math.Pow(10, int.Parse(line1.Substring(50, 2)));

            var bStarVal = line1.Substring(53, 6).Trim();
            var bStarPrefix = bStarVal[0] == '-' ? "-" : "";
            BStar =
                double.Parse($"{bStarPrefix}0.{bStarVal.Trim('-')}")
                * Math.Pow(10, int.Parse(line1.Substring(59, 2)));

            int sum = 0;
            for (int i = 0; i < line1.Length - 1; i++)
            {
                if (char.IsDigit(line1[i]))
                {
                    sum += (int)char.GetNumericValue(line1[i]);
                }
                else if (line1[i] == '-')
                {
                    sum += 1;
                }
            }
            var cs1 = int.Parse(line1.Substring(68, 1));
            if (cs1 != (sum % 10))
            {
                throw new ArgumentException("Invalid TLE data: Line 1 checksum failure");
            }

            // parse line 2
            Inclination = float.Parse(line2.Substring(8, 8));
            RightAscension = float.Parse(line2.Substring(17, 8));
            Eccentricity = float.Parse($"0.{line2.Substring(26, 7)}");
            ArgumentOfPerigee = float.Parse(line2.Substring(34, 8));
            MeanAnomaly = float.Parse(line2.Substring(43, 8));
            RevolutionsPerDay = double.Parse(line2.Substring(52, 11));
            RevolutionNumber = int.Parse(line2.Substring(63, 5));
            var cs2 = int.Parse(line2.Substring(68, 1));
            sum = 0;
            for (int i = 0; i < line2.Length - 1; i++)
            {
                if (char.IsDigit(line2[i]))
                {
                    sum += (int)char.GetNumericValue(line2[i]);
                }
                else if (line2[i] == '-')
                {
                    sum += 1;
                }
            }
            if (cs2 != (sum % 10))
            {
                throw new ArgumentException("Invalid TLE data: Line 2 checksum failure");
            }
        }

        public string Line1 { get; private set; }
        public string Line2 { get; private set; }

        // Line 1 fields
        public int SatelliteNumber { get; private set; }
        public char Classification { get; private set; }
        public int LaunchYear { get; private set; }
        public int LaunchNumber { get; private set; }
        public string LaunchPiece { get; private set; }
        public DateTime Timestamp { get; private set; }

        public float FirstTimeDerivative { get; private set; }
        public double SecondTimeDerivative { get; private set; }
        public double BStar { get; private set; }
        public int EphemerisType { get; private set; }
        public int ElementNumber { get; private set; }
        // Line 2 fields
        public float Inclination { get; private set; }
        public float RightAscension { get; private set; }
        public float Eccentricity { get; private set; }
        public float ArgumentOfPerigee { get; private set; }
        public float MeanAnomaly { get; private set; }
        public double RevolutionsPerDay { get; private set; }
        public int RevolutionNumber { get; private set; }
    }
}
