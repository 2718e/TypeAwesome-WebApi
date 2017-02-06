using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMethods
{

    public class SampleModel
    {
        public string Name { get; set; }

        public int Id { get; set; }

        public double Amount { get; set; }
    }

    public class Samplemodel2
    {

        public SampleModel[] Things { get; set; }

        public string CustomerName { get; set; }

        public int CostCents { get; set; }

    }

}
