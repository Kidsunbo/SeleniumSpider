using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumSpider
{
    partial class Form1
    {

        private string webDriverAddress;

        private readonly string webAddress = @"https://stores.ashleyfurniture.com/";


        Dictionary<string, string> dict = new Dictionary<string, string>();

        private static object locker = new object();


        private static int getMin(params int[] args)
        {
            int temp = int.MaxValue;
            foreach(int i in args)
            {
                temp = temp < i ? temp : i;
            }
            return temp;
        }



    }
}
