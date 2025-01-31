using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PlateRecognizer
{
    public class Utils
    {


        public static string es_reg = "^[0-9]{4}[BCDFGHJKLMNPRSTVWXYZ]{3}$";
        public static string pt_reg = "^[A-Za-z]{2}\\-?[0-9]{2}\\-?[0-9]{2}$|^[0-9]{2}\\-?[0-9]{2}\\-?[A-Za-z]{2}$|^[0-9]{2}\\-?[A-Za-z]{2}\\-?[0-9]{2}$|^[A-Za-z]{2}\\-?[0-9]{2}\\-?[A-Za-z]{2}$";



        public static bool isValidPlate(string? matricula, string region)
        {
       
            if (matricula == null) return false;
            if (Regex.IsMatch(matricula, pt_reg, RegexOptions.IgnoreCase) && region == "pt") { 
                return true; 
            }
            if ( Regex.IsMatch(matricula, es_reg, RegexOptions.IgnoreCase) && region == "es")
            {
                Console.WriteLine("ismatch");
                return true;
            }
            return false;
        }

        public static void SaveImages(string path,string db_name, byte[] data)
        {
            string base_dir = "C:\\images\\"+db_name +"\\";

            if (!Directory.Exists(base_dir))
            {
                Directory.CreateDirectory(base_dir);
            }

            string imagepath = base_dir + path + ".jpg";

            // Save the image bytes to a file
            File.WriteAllBytes(imagepath, data);


        }

    }







}
