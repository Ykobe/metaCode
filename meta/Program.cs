﻿using System; using ThermoFisher.CommonCore.RawFileReader; using ThermoFisher.CommonCore.Data.Business; using ThermoFisher.CommonCore.Data.Interfaces; using System.IO; using System.Text; using System.Collections; using System.Text.RegularExpressions; using ThermoFisher.CommonCore.Data.FilterEnums; using meta.util; using System.Linq; using ThermoFisher.CommonCore.Data; using System.Globalization;  namespace meta {     class Program     {         static void Main(string[] args)         {
            //const String rawFile = @"E:\Python_1\meta\meta\bin\x64\Debug\10_Ile.RAW";

            String rawFile = @"E:\Python_1\meta\meta\bin\x64\Debug\dataset\REF_NEG.RAW";              //string resFilePath = "123kl.csv";             //string paraIndex = "3,4";             //const String filename = "E:/wamp/www/tp5/application/index/controller/21_Val.RAW";             //string filename = args[0];             //showheaderInfo(filename);              //string resFilePath = args[0];             //string rawFile = args[1];             //string paraIndex = args[2];             //string headerInfo = Encoding.UTF8.GetString(Encoding.Default.GetBytes(""));             //for (int i = 3; i < args.Length; i++)             //{             //    headerInfo += Encoding.UTF8.GetString(Encoding.Default.GetBytes(args[i]));             //}             //getentityData(resFilePath, rawFile, headerInfo, paraIndex);              //getTuneInfo(filename);               //getMSdataFromScanNum(46, 48, rawFile, 50, 500);              getSRMIacdMZ(rawFile, 201.1687, 1000);
            //Console.ReadKey();
        }          public static void getentityData(string resFilePath, string rawFile, string headerInfo, string paraIndex)         {             IRawDataPlus plus = RawFileReaderAdapter.FileFactory(rawFile);             plus.SelectInstrument(Device.MS, 1);              ArrayList indexOfpara = new ArrayList();             string[] selectIndex = paraIndex.Split(',');              foreach (string ele in selectIndex)             {                 int index = Int32.Parse(ele);                 indexOfpara.Add(index);             }              //string fileName = DateTime.Now.ToLocalTime().ToString().Replace("/", "").Replace(":", "").Replace(" ", "_") + ".csv";             FileStream stream = new FileStream(resFilePath, FileMode.Create, FileAccess.Write);             StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);              headerInfo = "RT," + headerInfo;             headerInfo = "#," + headerInfo;             writer.WriteLine(headerInfo);              int offset = 0;             int maxoffset = plus.GetStatusLogEntriesCount();             while (offset < maxoffset)             {                 string temp = "";                 IStatusLogEntry statusLogEntry = plus.GetStatusLogEntry(offset);                 temp += offset.ToString() + ",";                 temp += statusLogEntry.Time.ToString() + ",";                  foreach (int ele in indexOfpara)                 {                     temp += Convert.ToString(statusLogEntry.Values[ele]) + ",";                 }                  writer.WriteLine(temp.Substring(0, temp.Length - 1));                 offset += 1;             }              writer.Close();         }          public static void showheaderInfo(string filePath)         {             IRawDataPlus plus = RawFileReaderAdapter.FileFactory(filePath);             plus.SelectInstrument(Device.MS, 1);             HeaderItem[] statuslogheaderinformation = plus.GetStatusLogHeaderInformation();             int index = 0;              Console.OutputEncoding = Encoding.UTF8;              while (index < statuslogheaderinformation.Length)             {                 Console.WriteLine(statuslogheaderinformation[index].Label);                 //headerInfo[index] = statuslogheaderinformation[index].Label;                 //writer.WriteLine(statuslogheaderinformation[index].Label);                 index += 1;             }             //writer.Close();              //while (true)             //{             //if (index >= statuslogheaderinformation.Length)             //{             //    Console.WriteLine(plus.RunHeaderEx.FirstSpectrum.ToString());             //    Console.WriteLine(plus.RunHeaderEx.LastSpectrum.ToString());             //    Console.WriteLine(plus.RunHeaderEx.StartTime.ToString("F2"));             //    Console.WriteLine(plus.RunHeaderEx.EndTime.ToString("F2"));             //    Console.WriteLine(plus.RunHeaderEx.LowMass.ToString());             //    Console.WriteLine(plus.RunHeaderEx.HighMass.ToString());             //    break;             //}             //    Console.WriteLine(statuslogheaderinformation[index].Label);             //    writer.WriteLine(statuslogheaderinformation[index].Label);             //    index++;             //}              //writer.Close();          }         public static void getTuneInfo(string filePath)         {             IRawDataPlus plus = RawFileReaderAdapter.FileFactory(filePath);             plus.SelectInstrument(Device.MS, 1);             string fileName = DateTime.Now.ToLocalTime().ToString().Replace("/", "").Replace(":", "").Replace(" ", "_") + ".txt";             FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);             StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);             writer.WriteLine("*********************InstrumentMethod(0)***************************");             writer.WriteLine(plus.GetInstrumentMethod(0));              writer.WriteLine("*********************InstrumentMethod(1)****************************");             writer.WriteLine(plus.GetInstrumentMethod(1));              HeaderItem[] tuneDataHeaderInfo = plus.GetTuneDataHeaderInformation();              writer.WriteLine("*********************Tune DATA ****************************");             int tuneCnt = plus.GetTuneDataCount();              for (int i = 0; i < tuneDataHeaderInfo.Length; ++i)             {                 writer.WriteLine(tuneDataHeaderInfo[i].Label);             }             writer.Close();         }          public static void getMSdataFromScanNum(int startNum, int endNum, string filePath, double lowMS = 0.00, double highMS = 0.00)         {             IRawDataPlus plus = RawFileReaderAdapter.FileFactory(filePath);

            //////////////////////////////////
            plus.SelectInstrument(Device.MS, 1);             /////////////////////////////////                          const string FilterStringIsolationMzPattern = @"ms2 (.*?)@";             int precursorMs1ScanNumber = 0;             double precursorMS= 0.00;             IReaction reaction = null;             var value = "";

            var firstScanNumber = plus.RunHeaderEx.FirstSpectrum;
            var lastScanNumber = plus.RunHeaderEx.LastSpectrum;

            startNum = firstScanNumber;
            endNum = lastScanNumber;

            //int maxNum = plus.RunHeaderEx.LastSpectrum > endNum ? endNum : plus.RunHeaderEx.LastSpectrum;              LimitedSizeDictionary<string, int> precursorMs2ScanNumbers = new LimitedSizeDictionary<string, int>(40);              plus.SelectInstrument(Device.MS, 1);             if(highMS - 0.00 <= 0.01)             {                 highMS = plus.RunHeaderEx.HighMass;             }              double maxMs = plus.RunHeaderEx.HighMass;             double minMs = plus.RunHeaderEx.LowMass;              lowMS = minMs;             highMS = maxMs;              string fileName = DateTime.Now.ToLocalTime().ToString().Replace("/", "").Replace(":", "").Replace(" ", "_") + ".csv";             FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);             StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);                          writer.WriteLine("ScanNumber, RT, Mass, Resolution, Intensity, Noise, Filter,MSLevel,collisionEnergy, precursorNum,precursorMS");             while (startNum <= endNum)             {                 var levelInfo = "";                 var spectrumRef = "";                 Scan scan = Scan.FromFile(plus, startNum);                 IScanFilter scanFilter = plus.GetFilterForScanNumber(startNum);                  IScanEvent scanEvent = plus.GetScanEventForScanNumber(startNum);                  switch (scanFilter.MSOrder)                 {                     case MSOrderType.Ms:                         // Keep track of scan number for precursor reference                         precursorMs1ScanNumber = startNum;                         reaction = scanEvent.GetReaction(0);                         value = reaction.CollisionEnergy.ToString(CultureInfo.InvariantCulture).ToString();                         levelInfo = scanFilter.MSOrder.ToString() + "," + "" + "," + "" + "," + "";                         break;                     case MSOrderType.Ms2:                          // Keep track of scan number and isolation m/z for precursor reference                                            var result = Regex.Match(scanEvent.ToString(), FilterStringIsolationMzPattern);                         if (result.Success)                         {                             if (precursorMs2ScanNumbers.ContainsKey(result.Groups[1].Value))                             {                                 precursorMs2ScanNumbers.Remove(result.Groups[1].Value);                             }                              precursorMs2ScanNumbers.Add(result.Groups[1].Value, startNum);                         }                         spectrumRef = precursorMs1ScanNumber.ToString();                         reaction = scanEvent.GetReaction(0);                         value = reaction.CollisionEnergy.ToString(CultureInfo.InvariantCulture).ToString();                         precursorMS = reaction.PrecursorMass;                         levelInfo = scanFilter.MSOrder.ToString() + "," + value + "," + spectrumRef.ToString() + "," +  precursorMS.ToString()  ;                         break;                     case MSOrderType.Ms3:                         var precursorMs2ScanNumber = precursorMs2ScanNumbers.Keys.FirstOrDefault(isolationMz => scanEvent.ToString().Contains(isolationMz));                         spectrumRef = precursorMs2ScanNumbers[precursorMs2ScanNumber].ToString();                         if (!precursorMs2ScanNumber.IsNullOrEmpty())                         {                             reaction = scanEvent.GetReaction(1);                         }                         else                         {                             throw new InvalidOperationException("Couldn't find a MS2 precursor scan for MS3 scan " + scanEvent);                         }                         precursorMS = reaction.PrecursorMass;                         value = reaction.CollisionEnergy.ToString(CultureInfo.InvariantCulture).ToString();                         levelInfo = scanFilter.MSOrder.ToString() + "," + value +  "," + spectrumRef.ToString() + "," + precursorMS.ToString();                         break;                     default:                         throw new ArgumentOutOfRangeException();                 }                  //var scanStatistics = plus.GetScanStatsForScanNumber(startNum);                 //// Get the segmented (low res and profile) scan data                 //var segmentedScan = plus.GetSegmentedScanFromScanNumber(startNum, scanStatistics);                 //double[] masses = segmentedScan.Positions;                 //double[] intensitis = segmentedScan.Intensities;                  //Console.WriteLine(masses.Length);                 //Console.WriteLine(intensitis.Length);                  int cnt = 0;                  if (scanFilter.MSOrder == MSOrderType.Ms)
                {
                    while (cnt < scan.PreferredMasses.Length)
                    {
                        if (lowMS < scan.PreferredMasses[cnt] && scan.PreferredMasses[cnt] < highMS)
                        {
                            double rt = plus.RetentionTimeFromScanNumber(startNum);
                            double msval = scan.PreferredMasses[cnt];
                            double resolution = (scan.PreferredResolutions.Length == 0) ? 0.0 : scan.PreferredResolutions[cnt];
                            double intensity = (scan.PreferredIntensities.Length == 0) ? 0.0 : scan.PreferredIntensities[cnt];
                            double noise = (scan.PreferredNoises.Length == 0) ? 0.0 : scan.PreferredNoises[cnt];

                            //msval.ToString("f4");Return a number to a given precision,carry bit
                            //msval.ToString("n4");Return a number to a given precision,not carry bit

                            //writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7}", startNum.ToString(), rt.ToString(),
                            //    msval.ToString("n4"), resolution.ToString(), intensity.ToString(), noise.ToString(),
                            //    scanFilter.ToString(), levelInfo);

                            writer.WriteLine("{0},{1},{2},{3}", startNum.ToString(), rt.ToString(), msval.ToString(), intensity.ToString());
                        }
                        cnt += 1;
                    }                 }                 writer.Flush();                 startNum += 1;             }             writer.Close();          }          public static void getMSdataFromRT(double startRT, int endRT, string filePath, double lowMS = 0.00, double highMS = 0.00)         {             IRawDataPlus plus = RawFileReaderAdapter.FileFactory(filePath);              plus.SelectInstrument(Device.MS, 1);             if (highMS - 0.00 <= 0.01)             {                 highMS = plus.RunHeaderEx.HighMass;             }             int startNum = plus.ScanNumberFromRetentionTime(startRT);             int endNum = plus.ScanNumberFromRetentionTime(endRT);             int maxNum = plus.RunHeaderEx.LastSpectrum > endNum ? endNum : plus.RunHeaderEx.LastSpectrum;              string fileName = DateTime.Now.ToLocalTime().ToString().Replace("/", "").Replace(":", "").Replace(" ", "_") + ".csv";             FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);             StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);             writer.WriteLine("ScanNumber, RT, Mass, Resolution, Intensity, Noise, Filter");             while (startNum <= endNum)             {                 Scan scan = Scan.FromFile(plus, startNum);                 string filter = plus.GetFilterForScanNumber(startNum).ToString();                 int cnt = 0;                 while (cnt < scan.PreferredMasses.Length)                 {                     if (lowMS < scan.PreferredMasses[cnt] && scan.PreferredMasses[cnt] < highMS)                     {                         double rt = plus.RetentionTimeFromScanNumber(startNum);                         double msval = scan.PreferredMasses[cnt];                         double resolution = (scan.PreferredResolutions.Length == 0) ? 0.0 : scan.PreferredResolutions[cnt];                         double intensity = (scan.PreferredIntensities.Length == 0) ? 0.0 : scan.PreferredIntensities[cnt];                         double noise = (scan.PreferredNoises.Length == 0) ? 0.0 : scan.PreferredNoises[cnt];                         //writer.WriteLine("{0},{1},{2},{3},{4},{5},{6}", startNum.ToString(), rt.ToString(), msval.ToString(), resolution.ToString(), intensity.ToString(), noise.ToString(), filter.ToString());

                        writer.WriteLine("{0},{1},{2},{3}", startNum.ToString(), rt.ToString(), msval.ToString(), intensity.ToString());                     }                     cnt += 1;                 }                 startNum += 1;             }             writer.Close();         }          public static void getSRMIacdMZ(string filePath, double valMS,int recVal)
        {
           IRawDataPlus plus = RawFileReaderAdapter.FileFactory(filePath);

            //////////////////////////////////
            plus.SelectInstrument(Device.MS, 1);             /////////////////////////////////

            var startNum = plus.RunHeaderEx.FirstSpectrum;
            var endNum = plus.RunHeaderEx.LastSpectrum;              string fileName = DateTime.Now.ToLocalTime().ToString().Replace("/", "").Replace(":", "").Replace(" ", "_") + ".csv";             FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);             StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);              double[] masses = null;
            double[] intensities = null;              writer.WriteLine("ScanNumber, RT, Mass, Intensity");             while (startNum <= endNum)             {
                double rt = plus.RetentionTimeFromScanNumber(startNum);                 IScanFilter scanFilter = plus.GetFilterForScanNumber(startNum);
                if (scanFilter.MSOrder == MSOrderType.Ms)
                {
                    Scan scan = Scan.FromFile(plus, startNum);
                    IScanEvent scanEvent = plus.GetScanEventForScanNumber(startNum);
                    if (scan.HasCentroidStream && (scanEvent.ScanData == ScanDataType.Centroid || scanEvent.ScanData == ScanDataType.Profile))
                    {
                        var centroidStream = plus.GetCentroidStream(startNum, false);
                        if (scan.CentroidScan.Length > 0)
                        {
                            masses = centroidStream.Masses;
                            intensities = centroidStream.Intensities;
                        }
                    }
                    else
                    {
                        // Get the scan statistics from the RAW file for this scan number
                        var scanStatistics = plus.GetScanStatsForScanNumber(startNum);                         // Get the segmented (low res and profile) scan data                         var segmentedScan = plus.GetSegmentedScanFromScanNumber(startNum, scanStatistics);
                        if (segmentedScan.Positions.Length > 0)
                        {
                            masses = segmentedScan.Positions;
                            intensities = segmentedScan.Intensities;
                        }
                    }

                    for(int i = 0; i < masses.Length; ++ i)
                    {
                        masses[i] = Double.Parse(masses[i].ToString("n4"));
                    }

                    int front = 0;
                    int rear = masses.Length - 1;
                    int mid = front + ((rear - front) >> 1);
                    bool findVal = false;
                    while(front <= rear)
                    {
                        if (Double.Equals(masses[mid],valMS))
                        {
                            findVal = true;
                            break;
                        }
                        else if(masses[mid] < valMS)
                        {
                            front = mid + 1;
                        }
                        else
                        {
                            rear = mid  - 1;
                        }
                        mid = front + ((rear - front) >> 1);
                    }
                    if(findVal && intensities[mid] >= recVal)
                    {
                        writer.WriteLine("{0},{1},{2},{3}",startNum.ToString(), rt.ToString(), masses[mid].ToString(), intensities[mid].ToString());
                    }
                }
                writer.Flush();
                startNum += 1;             }             writer.Close();
        }     } } 