using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartParser
{
    class LNGFileValue_DailyTotal
    {
        public string Country { get; set; }
        public DateTime Date { get; set; }
        public double Inventory { get; set; }
        public double SendOut { get; set; }
        public string Status { get; set; }
        public double Dtmi { get; set; }
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

    class PetParser
    {
        private string _url;
        private Type _model;


        public PetParser(string link, Type model)
        {
            _url = link;
            _model = model;
            _scan();
        }

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
                using (var stream = new MemoryStream(File.ReadAllBytes("c:\\Users\\Peter Vargovcik\\Documents\\Visual Studio 2015\\Projects\\SmartParser\\SmartParser\\Files\\daily_totals_2012-2014.xlsx")))
                {
                    XSSFWorkbook hssfwb = new XSSFWorkbook(stream);

                    Dictionary<XSSFRow, IEnumerable<XSSFRow>> rowDictionary = new Dictionary<XSSFRow, IEnumerable<XSSFRow>>();
                    string previousHash = "";

                    for (int i = 0; i < hssfwb.NumberOfSheets; i++)
                    {
                        XSSFSheet sheet = (XSSFSheet)hssfwb.GetSheetAt(i);

                        IEnumerator rows = sheet.GetRowEnumerator();

                        int rowNumber = 0;

                        while (rows.MoveNext())
                        {
                            XSSFRow row = (XSSFRow)rows.Current;

                            Console.Write("Sheet : {0}, Line {1}, ", sheet.SheetName, rowNumber);
                            
                            // hash of the row types 
                            var rowTypeString =String.Join("-", row.Cells.Select(x => x.CellType.ToString()).ToArray());

                           var currentHashRow = _getCheckSumMD5(Encoding.ASCII.GetBytes(rowTypeString));

                            if(_hashNotSame(previousHash, currentHashRow))
                            {
                                rowDictionary.Add(row, new List<XSSFRow>());
                            }
                            else
                            {
                                IEnumerable<XSSFRow> list = rowDictionary.Last().Value;
                                ((List<XSSFRow>)list).Add(row);
                            }


                            //for (int j = 0; j < row.Cells.Count; j++)
                            //{
                            //    Console.Write("[{0} : {1}], ", j, row.Cells[j].CellType.ToString());
                            //}

                            //Console.WriteLine();

                            //var firstCell = row.Cells[0].CellType;
                            //Console.WriteLine(firstCell.ToString());
                            rowNumber++;
                            previousHash = currentHashRow;
                        }

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
}