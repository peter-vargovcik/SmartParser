using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartParser.Helpers
{
    internal static class HelpersMethods
    {
        internal static string GetMD5(byte[] rawData)
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

        internal static string GetXSSFCellStringValue(XSSFCell cell)
        {
            string output;
            switch (cell.CellType)
            {
                case CellType.Unknown:
                    output = "Unknown Cell";
                    break;
                case CellType.Numeric:
                    output = "" + cell.NumericCellValue;
                    break;
                case CellType.String:
                    output = cell.StringCellValue;
                    break;
                case CellType.Formula:
                    output = "Cell is Formula Type";
                    break;
                case CellType.Blank:
                    output = "";
                    break;
                case CellType.Boolean:
                    output = cell.BooleanCellValue ? "true" : "false";
                    break;
                case CellType.Error:
                    output = cell.ErrorCellString;
                    break;
                default:
                    throw new ApplicationException("GetXSSFCellStringValue Error - XSSFCell cell type is not defined");
            }
            return output;
        }

        internal static string GetICellStringValue(ICell cell)
        {
            string output;
            switch (cell.CellType)
            {
                case CellType.Unknown:
                    output = "Unknown Cell";
                    break;
                case CellType.Numeric:
                    output = "" + cell.NumericCellValue;
                    break;
                case CellType.String:
                    output = cell.StringCellValue;
                    break;
                case CellType.Formula:
                    output = "Cell is Formula Type";
                    break;
                case CellType.Blank:
                    output = "";
                    break;
                case CellType.Boolean:
                    output = cell.BooleanCellValue ? "true" : "false";
                    break;
                case CellType.Error:
                    output = "ICell have Error!!!";
                    break;
                default:
                    throw new ApplicationException("GetXSSFCellStringValue Error - XSSFCell cell type is not defined");
            }
            return output;
        }

        internal static string StringArrayToString(string[] array)
        {
            return string.Join(",", array);
        }
    }
}
