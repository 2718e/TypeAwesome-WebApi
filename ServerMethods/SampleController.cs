using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Http;

namespace ServerMethods
{
    public class SampleController : ApiController
    {

        public Samplemodel2 SampleGet()
        {
            var result = new Samplemodel2
            {
                CustomerName = "A. Person",
                CostCents = 5000,
                Things = new SampleModel[]
                {
                    new SampleModel {Amount=10, Id=1,Name="cellphones" },
                    new SampleModel {Amount=2, Id=2,Name="Wireless routers" },
                }
            };
            return result;
        }

        public Samplemodel2 DuplicateModel(SampleModel toClone, int nTimes)
        {
            var result = new Samplemodel2
            {
                CustomerName = "Evil Clone",
                CostCents = nTimes*100,
                Things = Enumerable.Range(0,nTimes).Select(_=>toClone).ToArray()
            };
            return result;
        }

        public double Multiply3Numbers(double n1, double n2, double n3) { return n1 * n2 * n3; }

    }
}
