

// Updated C# program to support mixed-format interval input with optional frequencies

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Statistik
{

    class Interval
    {
        public double Lower { get; set; }
        public double Upper { get; set; }
        public long Hyppighed { get; set; } = 0;

        public override string ToString()
        {
            if (true == Program.leftClosed)
            {
                return ($"[{this.Lower:F2} - {this.Upper:F2}[");
            }
            else
            {
                return ($"]{this.Lower:F2} - {this.Upper:F2}]");
            }
        }
    }

    public enum ExcelKvartilMetode
    {
        EXC, // QUARTILE.EXC (ekskluderer median)
        INC  // QUARTILE.INC (inkluderer median)
    }

    class Program
    {
        public static bool leftClosed = true;

        static void Main()
        {
            string inputPath = string.Empty;
            string groupedOutputPath = string.Empty;
            string ungroupedOutputPath = string.Empty;

            List<double> observations = new();
            List<Interval> intervals = new();
           
            Console.Clear();
            Console.Write("Indtast navn på datafil med observationer : ");
            inputPath = Console.ReadLine();
            Console.WriteLine("");
            Console.WriteLine("");

            foreach (var line in File.ReadAllLines(inputPath))
            {
                var trimmed = line.Trim();

                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    continue;
                }

                if (trimmed.StartsWith("INTERVALS:", StringComparison.OrdinalIgnoreCase))
                {
                    if (trimmed.ToUpper().Contains("LEFT_OPEN"))
                    {
                        leftClosed = false;
                    }
                    else
                    {
                        if (trimmed.ToUpper().Contains("LEFT_CLOSED"))
                        {
                            leftClosed = true;
                        }
                    }
                    continue;
                }

                var parts = trimmed.Split('-');
                if (parts.Length >= 2 && double.TryParse(parts[0], out double low) && double.TryParse(parts[1], out double high))
                {
                    long hyppighed = 0;
                    if (parts.Length == 3 && long.TryParse(parts[2], out long explicitHyppighed))
                    {
                        hyppighed = explicitHyppighed;
                    }
                    intervals.Add(new Interval { Lower = low, Upper = high, Hyppighed = hyppighed });
                }
                else if (double.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                {
                    observations.Add(val);
                }
            }

            bool hasGrouped = intervals.Any();
            bool hasUngrouped = observations.Any();

            if (hasGrouped)
            {
                groupedOutputPath = inputPath.Split('.')[0];
                groupedOutputPath = groupedOutputPath + "_Grupperet_Out.txt";

                // Count observations into intervals
                foreach (var obs in observations)
                {
                    bool matched = false;
                    foreach (var interval in intervals)
                    {
                        bool inInterval = leftClosed
                            ? obs >= interval.Lower && obs < interval.Upper
                            : obs > interval.Lower && obs <= interval.Upper;

                        if (inInterval)
                        {
                            interval.Hyppighed++;
                            matched = true;
                            break;
                        }
                    }
                    if (!matched)
                    {
                        Console.WriteLine($"Warning: Observation {obs} does not fit any interval");
                    }
                }
                Console.WriteLine("");
                WriteGroupedOutput(intervals, groupedOutputPath);
            }

            if (hasUngrouped)
            {
                ungroupedOutputPath = inputPath.Split('.')[0];
                ungroupedOutputPath = ungroupedOutputPath + "_Ugrupperet_Out.txt";
                WriteUngroupedOutput(observations, ungroupedOutputPath);
            }

            Console.ReadLine();
        }

        static void WriteGroupedOutput(List<Interval> intervals, string path)
        {
            intervals = intervals.OrderBy(i => i.Lower).ToList();
            var output = new List<string> { "Grupperet Data Tabel" };
            output.Add("--------------------");
            //output.Add("");
            long totalAntalObservationer = intervals.Sum(i => i.Hyppighed);
            double mean = intervals.Sum(i => ((i.Lower + i.Upper) / 2.0) * i.Hyppighed) / totalAntalObservationer;
            double variance = intervals.Sum(i => Math.Pow((i.Lower + i.Upper) / 2.0 - mean, 2) * i.Hyppighed) / totalAntalObservationer;
            long summeretHyppighed = 0;
            double summeretFrekvens = 0;
            double intervalMidtpunktGangeHyppighedSamlet = 0;
            long maxGangetypeIntervalForekommer = intervals.Max(i => i.Hyppighed);
                        
            output.Add("Interval\t\tMidtpunkt\tHyppighed h(x)\tSummeret Hyppighed H(x)\tFrekvens f(x)\tSummeret Frekvns F(x)\tObservation * hyppighed");

            foreach (var i in intervals)
            {
                summeretHyppighed += i.Hyppighed;
                double mid = (i.Lower + i.Upper) / 2.0;
                double frekvens = (double)i.Hyppighed / totalAntalObservationer;
                summeretFrekvens += frekvens;
                double intervalMidtpunktGangeHyppighed = i.Hyppighed * mid;
                intervalMidtpunktGangeHyppighedSamlet += intervalMidtpunktGangeHyppighed;
                if (true == leftClosed)
                {
                    output.Add($"[{i.Lower:F2} - {i.Upper:F2}[\t{mid:F2}\t\t{i.Hyppighed}\t\t{summeretHyppighed}\t\t\t{frekvens:F2}\t\t{summeretFrekvens:F2}\t\t\t{intervalMidtpunktGangeHyppighed:F2}");
                }
                else
                {
                    output.Add($"]{i.Lower:F2} - {i.Upper:F2}]\t{mid:F2}\t\t{i.Hyppighed}\t\t{summeretHyppighed}\t\t\t{frekvens:F2}\t\t{summeretFrekvens:F2}\t\t\t{intervalMidtpunktGangeHyppighed:F2}");
                }
            }

            double stdDev = Math.Sqrt(variance);
            Interval minInterval = intervals.First(i => i.Hyppighed > 0);
            Interval maxInterval = intervals.Last(i => i.Hyppighed > 0);
            double range = maxInterval.Upper - minInterval.Lower;

            double modeLower = intervals.First(i => i.Hyppighed == maxGangetypeIntervalForekommer).Lower;
            double modeUpper = intervals.First(i => i.Hyppighed == maxGangetypeIntervalForekommer).Upper;
            var typeIntervaller = intervals.Where(i => i.Hyppighed == maxGangetypeIntervalForekommer).Select(i => (i.Lower, i.Upper));

            double Q1 = ComputeGroupedQuartile(intervals, totalAntalObservationer, 0.25);
            double Q2 = ComputeGroupedQuartile(intervals, totalAntalObservationer, 0.50);
            double Q3 = ComputeGroupedQuartile(intervals, totalAntalObservationer, 0.75);
            var (q1Exc, q2Exc, q3Exc) = BeregnKvartilerGrupperet(intervals, ExcelKvartilMetode.EXC);
            var (q1Inc, q2Inc, q3Inc) = BeregnKvartilerGrupperet(intervals, ExcelKvartilMetode.INC);

            output.Add("");
            output.Add("Grupperet Statistik");
            output.Add("-------------------");
            output.Add($"Observationer * Hyppighed samlet    : {intervalMidtpunktGangeHyppighedSamlet}");
            output.Add($"Antal observationer                 : {totalAntalObservationer}");
            output.Add($"Middelværdi                         : {intervals.Sum(i => ((i.Lower + i.Upper) / 2.0) * i.Hyppighed).ToString()} / {totalAntalObservationer} = {mean:F2}");
            output.Add($"Varians                             : {variance:F2}");
            output.Add($"Standardafvigelse/Spredning         : {stdDev:F2}");
            if (true == leftClosed)
            {
                output.Add($"Min Interval                        : [{minInterval.Lower:F2} - {minInterval.Upper:F2}[");
            }
            else
            {
                output.Add($"Min Interval                        : ]{minInterval.Lower:F2} - {minInterval.Upper:F2}]");
            }

            if (true == leftClosed)
            {
                output.Add($"Max Interval                        : [{maxInterval.Lower:F2} - {maxInterval.Upper:F2}[");
            }
            else
            {
                output.Add($"Max Interval                        : ]{maxInterval.Lower:F2} - {maxInterval.Upper:F2}]");
            }
            output.Add($"Interval Range                      : {maxInterval.Upper:F2} - {minInterval.Lower:F2} = {range:F2}");

            if (true == leftClosed)
            {
                output.Add($"Type Intervaller                    : {string.Join(" ", typeIntervaller.Select(i => $"[{i.Lower:F2} - {i.Upper:F2}["))}");
            }
            else
            {
                output.Add($"Type Intervaller                    : {string.Join(" ", typeIntervaller.Select(i => $"]{i.Lower:F2} - {i.Upper:F2}]"))}");
            }
            //output.Add($"Type Intervaller                 : {string.Join(" - ", typeIntervaller.Select(i => (i.Lower.ToString("F2"), i.Upper.ToString("F2"))))}");
            //output.Add($"Type Intervaller                 : {string.Join(" , ", typeIntervaller.Select(i => i.ToString()))}");
            output.Add($"Antal gange Typeinterval forekommer : {maxGangetypeIntervalForekommer}");
            output.Add("");
            output.Add("Kvartilsæt");
            output.Add("----------");
            //output.Add($"Nedre Kvartil                       : {Q1:F2}");
            //output.Add($"Median                              : {Q2:F2}");
            //output.Add($"Øvre Kvartil                        : {Q3:F2}");
            output.Add($"Nedre Kvartil (Median inkluderet)      : {q1Inc:F2}");
            output.Add($"Median (Median inkluderet)             : {q2Inc:F2}");
            output.Add($"Øvre Kvartil (Median inkluderet)       : {q3Inc:F2}");
            output.Add($"Kvartil Afstand (Median inkluderet)    : {q3Inc:F2} - {q1Inc:F2} = {q3Inc - q1Inc:F2}");
            output.Add("");
            output.Add($"Nedre Kvartil (Median ekskluderet)     : {q1Exc:F2}");
            output.Add($"Median (Median ekskluderet)            : {q2Exc:F2}");
            output.Add($"Øvre Kvartil (Median ekskluderet)      : {q3Exc:F2}");
            output.Add($"Kvartil Afstand (Median ekskluderet)   : {q3Exc:F2} - {q1Exc:F2} = {q3Exc - q1Exc:F2}");
            output.Add("");
            output.Add("");

            File.WriteAllLines(path, output);
            output.ForEach(Console.WriteLine);
        }

        static double ComputeGroupedQuartile(List<Interval> intervals, long totalFrequency, double fraction)
        {
            double cumulative = 0;
            foreach (var interval in intervals)
            {
                cumulative += interval.Hyppighed;
                if (cumulative >= fraction * totalFrequency)
                {
                    double cumulativeBefore = cumulative - interval.Hyppighed;
                    double h = interval.Upper - interval.Lower;
                    double result = interval.Lower + ((fraction * totalFrequency - cumulativeBefore) / interval.Hyppighed) * h;
                    return result;
                }
            }
            return double.NaN;
        }

        static void WriteUngroupedOutput(List<double> observations, string path)
        {
            var ordered = observations.OrderBy(x => x).ToList();
            var freqDict = ordered.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

            var output = new List<string> { "Ugrupperet Data Tabel" };
            output.Add("---------------------");
            //output.Add("");
            output.Add("Observation\tHyppighed h(x)\tSummeret Hyppighed H(x)\tFrekvens f(x)\tSummeret Frekvns F(x)\tObservation * hyppighed");

            long summeretHyppighed = 0;
            double summeretFrekvens = 0;
            double ObservationHyppighedProduktSamlet = 0;
            long totalAntalObservationer = observations.Count;
            
            foreach (var kvp in freqDict.OrderBy(x => x.Key))
            {
                double observation = kvp.Key;
                long hyppighed = kvp.Value;
                summeretHyppighed += hyppighed;
                double frekvens = (double)hyppighed / totalAntalObservationer;
                summeretFrekvens += frekvens;
                double observationHyppighedProdukt = observation * hyppighed;
                ObservationHyppighedProduktSamlet += observationHyppighedProdukt;
                output.Add($"\t{observation}\t\t{hyppighed}\t\t{summeretHyppighed}\t\t{frekvens:F2}\t\t{summeretFrekvens:F2}\t\t\t{observationHyppighedProdukt}");
            }

            double mean = ObservationHyppighedProduktSamlet / totalAntalObservationer;
            double variance = observations.Sum(x => Math.Pow(x - mean, 2)) / totalAntalObservationer;
            double stdDev = Math.Sqrt(variance);
            double min = ordered.First();
            double max = ordered.Last();
            double range = max - min;

            var typeTal = freqDict.Where(x => x.Value == freqDict.Values.Max()).Select(x => x.Key);
            //double q1 = Percentile(ordered, 1);
            //double q2 = Percentile(ordered, 2);
            //double q3 = Percentile(ordered, 3);
            var (q1Exc, q2Exc, q3Exc) = BeregnKvartilsætUgrupperet(ordered, ExcelKvartilMetode.EXC);
            var (q1Inc, q2Inc, q3Inc) = BeregnKvartilsætUgrupperet(ordered, ExcelKvartilMetode.INC);

            output.Add("");
            output.Add("Ugrupperet Statistik");
            output.Add("--------------------");
            //output.Add("");
            output.Add($"Observationer * Hyppighed samlet       : {ObservationHyppighedProduktSamlet}");
            output.Add($"Antal Observationer                    : {totalAntalObservationer}");
            output.Add($"Middelværdi                            : {ObservationHyppighedProduktSamlet} / {totalAntalObservationer} = {mean:F2}");
            output.Add($"Varians                                : {variance:F2}");
            output.Add($"Standardafvigelse/Spredning            : {stdDev:F2}");
            output.Add("");
            output.Add($"Minimums værdi                         : {min:F2}");
            output.Add($"Maksimums værdi                        : {max:F2}");
            output.Add($"Variationsbredde                       : {max:F2} - {min:F2} = {range:F2}");
            output.Add($"Typetal                                : {string.Join(", ", typeTal.Select(x => x.ToString("F2")))}");
            output.Add($"Antal gange Typetal forekommer         : {freqDict.Values.Max()}");
            output.Add("");
            output.Add("Kvartilsæt");
            output.Add("----------");
            output.Add($"Nedre Kvartil (Median inkluderet)      : {q1Inc:F2}");
            output.Add($"Median (Median inkluderet)             : {q2Inc:F2}");
            output.Add($"Øvre Kvartil (Median inkluderet)       : {q3Inc:F2}");
            output.Add($"Kvartil Afstand (Median inkluderet)    : {q3Inc:F2} - {q1Inc:F2} = {q3Inc - q1Inc:F2}");
            output.Add("");
            output.Add($"Nedre Kvartil (Median ekskluderet)     : {q1Exc:F2}");
            output.Add($"Median (Median ekskluderet)            : {q2Exc:F2}");
            output.Add($"Øvre Kvartil (Median ekskluderet)      : {q3Exc:F2}");
            output.Add($"Kvartil Afstand (Median ekskluderet)   : {q3Exc:F2} - {q1Exc:F2} = {q3Exc - q1Exc:F2}");
            output.Add("");

            File.WriteAllLines(path, output);
            output.ForEach(Console.WriteLine);
        }

        static double Percentile(List<double> sortedList, int fractileValue)
        {
            double value = 0;
            
            //value = sortedList.OrderBy(x => x)
            //        .Skip(sortedList.Count() * fractileValue / 4 -
            //                (sortedList.Count() * fractileValue % 4 == 0 ? 1 : 0))
            //        .Take(sortedList.Count() * fractileValue % 4 == 0 ? 2 : 1)
            //        .Average();

            value = sortedList.OrderBy(x => x)
                    .Skip(sortedList.Count() * fractileValue / 4 -
                            (sortedList.Count() * fractileValue % 4 == 0 ? 1 : 0))
                    .Take(sortedList.Count() * fractileValue % 4 == 0 ? 2 : 1)
                    .Average();

            return (value);
        }

        //public static (double Q1, double Q2, double Q3) BeregnKvartilsætUgrupperet(List<double> data)
        //{
        //    if (data == null || data.Count == 0)
        //        throw new ArgumentException("Data må ikke være tom.");

        //    var sorted = data.OrderBy(x => x).ToList();
        //    int n = sorted.Count;

        //    double Median(List<double> list)
        //    {
        //        int count = list.Count;
        //        if (count % 2 == 0)
        //            return (list[count / 2 - 1] + list[count / 2]) / 2.0;
        //        else
        //            return list[count / 2];
        //    }

        //    double q2 = Median(sorted);

        //    List<double> lowerHalf = sorted.Take(n / 2).ToList();
        //    List<double> upperHalf = sorted.Skip((n + 1) / 2).ToList();

        //    double q1 = Median(lowerHalf);
        //    double q3 = Median(upperHalf);

        //    return (q1, q2, q3);
        //}

        public static double BeregnKvartilUgrupperet(List<double> data, double kvartil, ExcelKvartilMetode metode)
        {
            if (data == null || data.Count < 2)
                throw new ArgumentException("Mindst 2 observationer kræves.");

            if (kvartil < 0 || kvartil > 1)
                throw new ArgumentOutOfRangeException(nameof(kvartil), "Kvartil skal være mellem 0 og 1.");

            var sorted = data.OrderBy(x => x).ToList();
            int n = sorted.Count;

            double pos = metode == ExcelKvartilMetode.EXC
                ? kvartil * (n + 1)
                : kvartil * (n - 1) + 1;

            if (pos <= 1)
                return sorted[0];
            if (pos >= n)
                return sorted[n - 1];

            int lowerIndex = (int)Math.Floor(pos) - 1;
            int upperIndex = lowerIndex + 1;
            double fraction = pos - Math.Floor(pos);

            return sorted[lowerIndex] + fraction * (sorted[upperIndex] - sorted[lowerIndex]);
        }

        public static (double Q1, double Q2, double Q3) BeregnKvartilsætUgrupperet(
            List<double> data,
            ExcelKvartilMetode metode = ExcelKvartilMetode.EXC)
        {
            return (
                BeregnKvartilUgrupperet(data, 0.25, metode),
                BeregnKvartilUgrupperet(data, 0.50, metode),
                BeregnKvartilUgrupperet(data, 0.75, metode)
            );
        }

        private static double BeregnKvartilGrupperet(List<Interval> intervaller, long N, double p, ExcelKvartilMetode metode)
        {
            double pos = metode switch
            {
                ExcelKvartilMetode.INC => p * (N - 1) + 1,
                ExcelKvartilMetode.EXC => p * (N + 1),
                _ => throw new ArgumentOutOfRangeException()
            };

            long kumuleret = 0;
            foreach (var interval in intervaller)
            {
                long før = kumuleret;
                kumuleret += interval.Hyppighed;

                if (kumuleret >= pos)
                {
                    double L = interval.Lower;
                    double f = interval.Hyppighed;
                    double w = interval.Upper - interval.Lower;

                    return L + ((pos - før) / f) * w;
                }
            }

            // Hvis vi ender udenfor (burde ikke ske)
            return intervaller.Last().Upper;
        }

        public static (double Q1, double Q2, double Q3) BeregnKvartilerGrupperet(List<Interval> intervaller, ExcelKvartilMetode metode)
        {
            if (intervaller == null || intervaller.Count == 0)
                throw new ArgumentException("Der kræves mindst ét interval");

            long N = intervaller.Sum(i => i.Hyppighed);

            double p1 = 0.25;
            double p2 = 0.50;
            double p3 = 0.75;

            return (
                BeregnKvartilGrupperet(intervaller, N, p1, metode),
                BeregnKvartilGrupperet(intervaller, N, p2, metode),
                BeregnKvartilGrupperet(intervaller, N, p3, metode)
            );
        }

    }
}