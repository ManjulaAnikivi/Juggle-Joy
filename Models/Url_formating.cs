using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{
    public class Url_formating
    {

        public string encode_url(string input)
        {
            if (input.Length > 0)
            {
                //input = input.Replace("-", "~");
                input = input.Replace(" ", "-");
                input = input.Replace("/", "_");
                input = input.Replace(".", "");
                input = input.Replace("&", "~");
                input = input.Replace(":", "-");
                input = input.Replace(",", "");
                input = input.Replace("?", "-");
                input = input.Replace("*", "");


                input = input.Replace("á", "a");
                input = input.Replace("é", "e");
                input = input.Replace("í", "i");
                input = input.Replace("ó", "o");
                input = input.Replace("ú", "u");

                input = input.Replace("ü", "u");
                input = input.Replace("ñ", "n");

                input = input.Replace("Á", "A");
                input = input.Replace("É", "E");
                input = input.Replace("Í", "I");
                input = input.Replace("Ó", "O");
                input = input.Replace("Ú", "U");

                input = input.Replace("Ñ", "N");
                input = input.Replace("Ü", "Ü");
                return input;
            }
            else
            {
                return "";
            }
        }
        public string decode_url(string input)
        {
            if (input.Length > 0)
            {

                input = input.Replace("-", " ");
                //input = input.Replace("~", "-");        
                input = input.Replace("_", "/");
                input = input.Replace("~", "&");
                //input = input.Replace("_", ",");

                //input = input.Replace("a", "á");
                //input = input.Replace("e", "é");
                //input = input.Replace("i", "í");
                //input = input.Replace("o", "ó");
                //input = input.Replace("u", "ú");

                //input = input.Replace("u", "ü");
                //input = input.Replace("n", "ñ");

                //input = input.Replace("A", "Á");
                //input = input.Replace("E", "É");
                //input = input.Replace("I", "Í");
                //input = input.Replace("O", "Ó");
                //input = input.Replace("U", "Ú");

                //input = input.Replace("N", "Ñ");
                //input = input.Replace("U", "Ü");
                return input;
            }
            else
            {
                return "";
            }
        }

    }
}