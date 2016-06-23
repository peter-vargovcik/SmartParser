using NPOI.XSSF.UserModel;
using SmartParser.Algorithms;
using SmartParser.Helpers;
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

            StringCombinations sc = new StringCombinations(new string[] {
                "DATE",
                "Inventory (103 m3 LNG)" ,
                "Send-Out (106 m3 NG)" ,
                "STATUS" ,
                "DTMI (103 m3 LNG)" ,
                "DTRS (106 m3 NG)",
                "WARNING",
                "INFO"}, 6);




            var outputLists = sc.GetCombinations();

            HashSet<string> outputHash = new HashSet<string>();

            foreach (var item in outputLists)
            {
                outputHash.Add(HelpersMethods.GetMD5(item.ToArray()));
            }

            var headderHashes = petParser.HeadersCombinationsHash;

            foreach (var hash in headderHashes)
            {
                if(outputHash.Contains(hash))
                    throw new Exception("FoundHash!");
            }


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
        private Dictionary<string, string[]> _headderMap;
        private MyLinkedList<XSSFRow> _linkedList = new MyLinkedList<XSSFRow>();

        public PetParser(string link, Type model)
        {
            _url = link;
            _model = model;

            _buildHeadersCombinationFromAttributes();
            _scan();

        }


        public string[] HeadersCombinationsHash { get; private set; }

        public List<string[]> getRows()
        {
            return _linkedList.Select(x => { return x.Cells.Select(y => { return HelpersMethods.GetICellStringValue(y); }).ToArray(); }).ToList();
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

            HeadersCombinationsHash = _getCombinations(headders).Select(x=> 
            {
                byte[] headder = Encoding.ASCII.GetBytes(HelpersMethods.StringArrayToString(x.ToArray()));
                return HelpersMethods.GetMD5(headder);
            }).ToArray<string>();


            _headderMap = _getCombinations(headders).ToDictionary(k => 
            {
                return HelpersMethods.GetMD5(Encoding.ASCII.GetBytes(HelpersMethods.StringArrayToString(k.ToArray())));
            }
            ,v=> v.ToArray());

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
                    var listCopy = listOfLists.Select(x => { return x.CloneList(); }).ToList();

                    for (int propertyParamIndex = 0; propertyParamIndex < propertyParamsLengh; propertyParamIndex++)
                    {
                        if(propertyParamIndex == 0)
                            listOfLists.ForEach(x => x.Add(item.Value.Value[propertyParamIndex]));
                        else
                        {
                            var copy = listCopy.Select(x => { return x.CloneList(); }).ToList();
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
        //}C:\Users\Peter Vargovcik\Documents\Visual Studio 2015\Projects\SmartParser\SmartParser\Files\daily_totals_2012-2014.xlsx
        private void _scan()
        {
            byte[] file = null;
            try
            {
                file = File.ReadAllBytes("C:\\Users\\PeterVargovcik\\Documents\\_PetProjects\\SmartParser\\SmartParser\\Files\\daily_totals_2012-2014.xlsx");
            }
            catch (Exception e)
            {
                file = File.ReadAllBytes("C:\\Users\\Peter Vargovcik\\Documents\\Visual Studio 2015\\Projects\\SmartParser\\SmartParser\\Files\\daily_totals_2012-2014.xlsx");
            }

            try
            {
                using (var stream = new MemoryStream(file))
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
                            _linkedList.Add(row);

                            
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