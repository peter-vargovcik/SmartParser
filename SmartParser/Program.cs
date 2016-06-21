using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartParser
{
    class LNGFileValue_DailyTotal
    {
        public string Country { get; set; }

        [TableHeader(0,"DATE")]
        public DateTime Date { get; set; }

        [TableHeader(1, "Inventory (103 m3 LNG)")]
        public double Inventory { get; set; }

        [TableHeader(2, "Send-Out (106 m3 NG)")]
        public double SendOut { get; set; }

        [TableHeader(3, "STATUS")]
        public string Status { get; set; }

        [TableHeader(4, "DTMI (103 m3 LNG)")]
        public double Dtmi { get; set; }

        [TableHeader(5, "DTRS (106 m3 NG)")]
        public double Dtrs { get; set; }
    }

    class Program
    {       

        static void Main(string[] args)
        {

            PetParser petParser = new PetParser("http://lngdataplatform.gie.eu/cron/daily_totals_2012-2014.xlsx"
                , typeof(LNGFileValue_DailyTotal));


        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class TableHeader : Attribute
    {
        public TableHeader(int index, params string[] headers)
        {
            Index = index;
            Headers = headers;
        }
        public int Index { get; private set; }
        public string[] Headers { get; private set; }
    }


    class PetParser
    {
        private string _url;
        private Type _model;
        private string[] _headersCombinationsHash;


        public PetParser(string link, Type model)
        {
            _url = link;
            _model = model;

            _buildHeadersCombinationFromAttributes();
            _scan();
        }

        private void _buildHeadersCombinationFromAttributes()
        {
            PropertyInfo[] props = _model.GetProperties();
            
            // convert to dictionary
            var headders = props
                .Where(x => x.GetCustomAttribute<TableHeader>() != null)
                .Select
                (
                    x => new KeyValuePair<int, KeyValuePair<string, string[]>>
                    (
                        x.GetCustomAttribute<TableHeader>().Index,
                        new KeyValuePair<string, string[]>(x.Name, x.GetCustomAttribute<TableHeader>().Headers)
                    )
                )
                .OrderBy(x => x.Key)
                .ToList();

            // possible combination count & check

            //var combinationPerProperty = headders
            //    .Select(x => x.Value.Value.Length)
            //    .ToArray<int>();

            //int combinationCount = combinationPerProperty.Aggregate(1, (a, b) => a * b);

            List<string[]> combinations = _getCombinations(headders);

            _headersCombinationsHash = _getCombinations(headders).Select(x=> 
            {
                byte[] headder = Encoding.ASCII.GetBytes(string.Join("-", x.ToArray()));
                return _getCheckSumMD5(headder);
            }).ToArray<string>();           

        }

        private List<string[]> _getCombinations(List<KeyValuePair<int, KeyValuePair<string, string[]>>> headders)
        {
            var listOfLists = new List<List<string>>();
            listOfLists.Add(new List<string>());
            
            foreach (KeyValuePair<int, KeyValuePair<string, string[]>> item in headders)
            {
                var propertyParamsLengh = item.Value.Value.Length;

                if(propertyParamsLengh >1)
                {
                    var listCopy = listOfLists.Select(x => { return _cloneList(x); }).ToList();

                    for (int propertyParamIndex = 0; propertyParamIndex < propertyParamsLengh; propertyParamIndex++)
                    {
                        if(propertyParamIndex == 0)
                            listOfLists.ForEach(x => x.Add(item.Value.Value[propertyParamIndex]));
                        else
                        {
                            var copy = listCopy.Select(x => { return _cloneList(x); }).ToList();
                            copy.ForEach(x => x.Add(item.Value.Value[propertyParamIndex]));
                            listOfLists.AddRange(copy);
                        }

                    }
                }
                else
                    listOfLists.ForEach(x => x.Add(item.Value.Value[0]));                
                
            }
            return listOfLists.Select(x=>x.ToArray()).ToList();
        }

        private List<string> _cloneList(List<string> source)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < source.Count; i++)
            {
                output.Add("" +source[i]);
            }
            return output;
        }

        //private List<T> _cloneList<T>(List<T> source)
        //{
        //    List<T> output = new List<T>();
        //    for (int i = 0; i < source.Count; i++)
        //    {
        //        output.Add((T)source[i].MemberwiseClone());
        //    }
        //    return output;
        //}

        private byte[] _downloadDataFromWeb(string link)
        {
            using (WebClient wc = new WebClient())
            {
                byte[] outFile = wc.DownloadData(link);

                return outFile;
            }
        }

        //private IEnumerable<T> _parseFile<T>(Metadata<T> metadata, byte[] rawData)
        //{
        //    var parsedData = new List<T>();

        //    //create parser
        //    try
        //    {
        //        using (var stream = new MemoryStream(rawData))
        //        {
        //            XSSFWorkbook hssfwb = new XSSFWorkbook(stream);

        //            for (int i = 0; i < hssfwb.NumberOfSheets; i++)
        //            {
        //                XSSFSheet sheet = (XSSFSheet)hssfwb.GetSheetAt(i);

        //                System.Collections.IEnumerator rows = sheet.GetRowEnumerator();
        //                rows.MoveNext();
        //                XSSFRow row = (XSSFRow)rows.Current;

        //                _headerList = _getHeaderList(row);
        //                _headerLookup = LNGProvider_Parser_HeaderLookup.BuildHeaderLookup(_headerList, metadata);

        //                if (_headerList == null)
        //                    throw new Exception("Unable to find Header row");

        //                while (rows.MoveNext())
        //                {
        //                    row = (XSSFRow)rows.Current;

        //                    ParsedRowList.Add(LNGProvider_Parser_Mapper._buildLNGFileValueDailyTotal(row, _headerLookup, LNGProvider_Parser_Constants.Countries[i]));

        //                }

        //            }

        //        }
        //        parsedData.ParsedData = ParsedRowList.ToArray();
        //        parsedData.RawData = rawData;

        //        return parsedData;
        //    }
        //    catch (Exception ex)
        //    {
        //        parsedData.RawData = rawData;
        //        return parsedData;
        //    }

        //    throw new NotImplementedException();
        //}

        private void _scan()
        {
            try
            {
                using (var stream = new MemoryStream(File.ReadAllBytes("C:\\Users\\PeterVargovcik\\Documents\\_PetProjects\\SmartParser\\SmartParser\\Files\\daily_totals_2012-2014.xlsx")))
                {
                    XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
                    MyLinkedList<XSSFRow> list = new MyLinkedList<XSSFRow>();
                    
                    for (int i = 0; i < hssfwb.NumberOfSheets; i++)
                    {
                        XSSFSheet sheet = (XSSFSheet)hssfwb.GetSheetAt(i);

                        IEnumerator rows = sheet.GetRowEnumerator();

                        int rowNumber = 0;

                        while (rows.MoveNext())
                        {
                            XSSFRow row = (XSSFRow)rows.Current;
                                                        
                            // hash of the row types 
                            var rowTypeString =String.Join("-", row.Cells.Select(x => x.CellType.ToString()).ToArray());
                           var currentHashRow = _getCheckSumMD5(Encoding.ASCII.GetBytes(rowTypeString));

                            list.Add(row, currentHashRow);
                        }

                        foreach (var node in list)
                        {
                            var cellCount = node.Cells;
                        }

                        list.evaluate();
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

  

        private bool _hashNotSame(string previousHash, string currentHashRow)
        {
            StringComparer stringComparer = StringComparer.InvariantCulture;

            if (stringComparer.Compare(previousHash, currentHashRow) == 0)
                return false;
            else
                return true;
        }

        private string _getCheckSumMD5(byte[] rawData)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(rawData);

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }
    }

    class Metadata<ModelObj>
    {

    }

    static class Extensions
    {
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }
}